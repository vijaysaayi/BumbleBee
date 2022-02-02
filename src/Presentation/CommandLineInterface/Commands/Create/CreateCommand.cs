using CommandDotNet;
using LibGit2Sharp;
using MediatR;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.AppService.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Penguin.Code.Application.AzureSDKWrappers.Create.NewAppServicePlan;
using Penguin.Code.Application.AzureSDKWrappers.Create.NewBlessedAppService;
using Penguin.Code.Application.AzureSDKWrappers.Create.NewResourceGroup;
using Penguin.Code.Application.AzureSDKWrappers.Create.NewSqlServer;
using Penguin.Code.Application.AzureSDKWrappers.Create.NewStorageAccount;
using Penguin.Code.Application.AzureSDKWrappers.GetInputs.AzureRegion;
using Penguin.Code.Application.AzureSDKWrappers.Update.ConnectionStrings;
using Penguin.Code.Application.ExtensionMethods;
using Penguin.Code.Application.HelperMethods.GetRandomName;
using Penguin.CommandLineInterface.Commands.Use;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Penguin.CommandLineInterface.Commands.Create
{
    [Command(Name = "create",
             Usage = "create [command]",
             Description = "Create new WebApp")]
    public class CreateCommand
    {
        private readonly IMediator _mediator;
        private Region _region;
        private CancellationToken _cancellationToken;
        private string _appName;
        private string _resourceGroupName;
        private string _appServicePlanName;

        public CreateCommand(IMediator mediator)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }

        [SubCommand]
        public FlaskApp FlaskApp { get; set; } = null!;

        [SubCommand]
        public DjangoApp DjangoApp { get; set; } = null!;

        [DefaultMethod]
        public async Task CreateNewWebApp([Option(LongName = "name", ShortName = "n", Description = "Name of the App Service")] string name,
            [Option(LongName = "with", ShortName = "w", Description = "Name of the App Service")] ResourceTypes resourceType ,
            CancellationToken cancellationToken)
        {
            _appName = string.IsNullOrWhiteSpace(name) ? await GetRandomName() : name ;
            
            _region = await _mediator.Send(new GetRegionNameCommand(),
                cancellationToken);
            _cancellationToken = cancellationToken;
            await AnsiConsole.Status()
                             .StartAsync("Processing...", async ctx =>
                             {
                                 _resourceGroupName = $"{_appName}-rsg";
                                 _appServicePlanName = $"{_appName}-asp";

                                 AnsiConsole.MarkupLine($"Creating a new app [green]{_appName}[/] in [green]{_region}[/] region..");

                                 // Update the status and spinner
                                 ctx.Status("Deploying new Resource Group");
                                 ctx.Spinner(Spinner.Known.Line);
                                 ctx.SpinnerStyle(Style.Parse("green"));
                                 bool isResourceGroupCreateSuccessful = await CreateResourceGroup();
                                 if (isResourceGroupCreateSuccessful)
                                 {
                                     ctx.Status("Deploying new App Service Plan");
                                     ctx.Spinner(Spinner.Known.Line);
                                     ctx.SpinnerStyle(Style.Parse("green"));
                                     IAppServicePlan appServicePlan = await CreateNewAppServicePlan();

                                     if (appServicePlan != null)
                                     {
                                         ctx.Status("Deploying new App Service");
                                         ctx.Spinner(Spinner.Known.Line);
                                         ctx.SpinnerStyle(Style.Parse("green"));
                                         IWebApp appService = await CreateNewAppService(appServicePlan);

                                         var publishProfile = await appService.GetPublishingProfileAsync();
                                         string userName = publishProfile.GitUsername;
                                         string password = publishProfile.GitPassword;
                                         if (appService != null)
                                         {
                                             string directory = Directory.GetCurrentDirectory();
                                             if (!Directory.Exists(".git"))
                                             {
                                                 Repository.Init(directory);
                                             }

                                             string url = publishProfile.GitUrl;
                                             url = $"https://{userName}:{password}@{url}/{_appName}.git";
                                             url = url.Replace(":443", "");
                                             using var repository = new Repository(directory);

                                             var remote = repository.Network.Remotes["azure"];
                                             if (remote == null)
                                             {
                                                 repository.Network.Remotes.Add("azure", url);
                                             }
                                             else if (remote.Url != url)
                                             {
                                                 repository.Network.Remotes.Remove("azure");
                                                 repository.Network.Remotes.Add("azure", url);
                                             }

                                             if(resourceType != ResourceTypes.none )
                                             {
                                                 ctx.Spinner(Spinner.Known.Line);
                                                 ctx.SpinnerStyle(Style.Parse("green"));
                                                 await DeployResource(ctx, resourceType);
                                                 ctx.Refresh();
                                                 ctx.Status = "Done";
                                             }

                                             AnsiConsoleExtensionMethods.Display($"Added git remote : [green]{publishProfile.GitUrl}[/]");
                                             Console.WriteLine("");
                                             AnsiConsole.MarkupLine($"You can browse to the app using [green]https://{appService.DefaultHostName}[/]");
                                         }
                                     }
                                 }
                             })
                             ;
        }

        private async Task<string> GetRandomName()
        {
            return await _mediator.Send(new GetRandomNameCommand()
            {
                Length = 4
            });
        }

        private async Task<IWebApp> CreateNewAppService(IAppServicePlan appServicePlan)
        {
            return await _mediator.Send(new CreateNewAppServiceWithBlessedImageCommand()
            {
                ResourceGroupName = _resourceGroupName,
                AppServicePlan = appServicePlan,
                AppServiceName = _appName,
                AzureRegion = _region
            },
            _cancellationToken);
        }

        private async Task<IAppServicePlan> CreateNewAppServicePlan()
        {
            return await _mediator.Send(new CreateNewAppServicePlanCommand()
            {
                ResourceGroupName = _resourceGroupName,
                AppServicePlanName = _appServicePlanName,
                AzureRegion = _region,
            },
            _cancellationToken);
        }

        private async Task<bool> CreateResourceGroup()
        {
            return await _mediator.Send(new CreateNewResourceGroupCommand()
            {
                ResourceGroupName = _resourceGroupName,
                AzureRegion = _region,
            },
            _cancellationToken);
        }

        private async Task DeployResource(StatusContext ctx, ResourceTypes resourceType)
        {
            var connectionStrings = new ConnectionStringDictionaryInner
            {
                Properties = new Dictionary<string, ConnStringValueTypePair>()
            };

            switch (resourceType)
            {
                case ResourceTypes.sqlserver:

                    // Update the status and spinner
                    ctx.Status("Create a new SQL server and Database");

                    var sqlServerName = _appName;
                    sqlServerName = sqlServerName.ToLowerInvariant().Trim().Replace("-", "");
                    string databaseName = $"{sqlServerName}db";
                    string userName = sqlServerName;
                    string password = $"{StringExtensionMethods.RandomString(10)}2#";

                    var sqlServer = await _mediator.Send(new CreateNewSqlServerCommand()
                    {
                        ResourceGroupName = _resourceGroupName,
                        AzureRegion = _region,
                        SqlServerName = sqlServerName,
                        DatabaseName = databaseName,
                        UserName = userName,
                        Password = password
                    });

                    if (sqlServer != null)
                    {
                        ctx.Status("Setting up connection strings needed in App Service");
                        string key = "Database";
                        string value = $"Server=tcp:{sqlServerName}.database.windows.net,1433;Initial Catalog={databaseName};Persist Security Info=False;User ID={userName};Password={password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
                        if (!connectionStrings.Properties.ContainsKey(key))
                        {
                            connectionStrings.Properties.Add(key, new ConnStringValueTypePair(value, ConnectionStringType.SQLAzure));
                        }
                        else
                        {
                            connectionStrings.Properties[key] = new ConnStringValueTypePair(value, ConnectionStringType.SQLAzure);
                        }

                        await _mediator.Send(new UpdateConnectionStringsCommand()
                        {
                            ConnectionStrings = connectionStrings,
                            ResourceGroup = _resourceGroupName,
                            AppServiceName = _appName
                        });
                    }

                    break;

                case ResourceTypes.storage:

                    var storage = await _mediator.Send(new CreateNewStorageAccountCommand()
                    {
                        StorageName = $"{_appName}storage",
                        AzureRegion = _region,
                        ResourceGroup = _resourceGroupName,
                    });

                    if (storage != null)
                    {
                        var keys = await storage.GetKeysAsync();
                        if (keys != null)
                        {
                            var firstKey = keys.FirstOrDefault();

                            var storageConnectionStringKey = "Storage";
                            var storageConnectionStringValue = $"DefaultEndpointsProtocol=https;AccountName={storage.Name};AccountKey={firstKey.Value};EndpointSuffix=core.windows.net";
                            if (!connectionStrings.Properties.ContainsKey(storageConnectionStringKey))
                            {
                                connectionStrings.Properties.Add(storageConnectionStringKey, new ConnStringValueTypePair(storageConnectionStringValue, ConnectionStringType.SQLAzure));
                            }
                            else
                            {
                                connectionStrings.Properties[storageConnectionStringKey] = new ConnStringValueTypePair(storageConnectionStringValue, ConnectionStringType.SQLAzure);
                            }

                            await _mediator.Send(new UpdateConnectionStringsCommand()
                            {
                                ConnectionStrings = connectionStrings,
                                ResourceGroup = _resourceGroupName,
                                AppServiceName = _appName
                            });
                        }
                    }

                    break;

                case ResourceTypes.postgresql:
                    break;

                default:
                    break;
            }
        }
    }
}