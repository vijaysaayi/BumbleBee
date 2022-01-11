using Penguin.Code.Application.AzureSDKWrappers.Create.NewResourceGroup;
using Penguin.Code.Application.AzureSDKWrappers.Create.NewSqlServer;
using Penguin.Code.Application.AzureSDKWrappers.Create.NewStorageAccount;
using Penguin.Code.Application.AzureSDKWrappers.GetInputs.AzureRegion;
using Penguin.Code.Application.AzureSDKWrappers.List.ResourceGroup;
using Penguin.Code.Application.AzureSDKWrappers.Update.ConnectionStrings;
using Penguin.Code.Application.AzureSDKWrappers.Validation.AppServiceExists;
using Penguin.Code.Application.ExtensionMethods;
using CommandDotNet;
using MediatR;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.AppService.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Sharprompt;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Penguin.CommandLineInterface.Commands.Use
{
    [Command(Name = "use",
             Usage = "use [command]",
             Description = "Add Azure Dependencies")]
    public class UseCommand
    {
        private readonly IMediator _mediator;
        private string _webappName;
        private Region _region;
        private string _resourceGroupName;

        public UseCommand(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [DefaultMethod]
        public async Task Resource(ResourceTypes resourceType,
            [Option(LongName = "name", ShortName = "n", Description = "Name of the App Service")] string webappName)
        {
            if (string.IsNullOrWhiteSpace(webappName))
            {
                webappName = AnsiConsole.Ask<string>("Enter the [green]name[/] of App Service?");
            }
            _webappName = webappName;
            IWebApp appService = null;
            await AnsiConsole.Status()
                 .StartAsync("Check if app exists", async ctx =>
                 {
                     ctx.Status("Fetching the details of the App Service ");
                     ctx.Spinner(Spinner.Known.Line);
                     ctx.SpinnerStyle(Style.Parse("green"));
                     appService = await _mediator.Send(new CheckIfAppServiceExistsCommand()
                     {
                         AppServiceName = _webappName
                     });
                     ctx.Status = "Done";
                 });

            if (appService == null)
            {
                AnsiConsole.MarkupLine("We had troubling finding which resource group the app exists.");
                var resourceGroups = await _mediator.Send(new ListResourceGroupsCommand());
                Console.OutputEncoding = System.Text.Encoding.ASCII;
                var resourceGroup = Prompt.Select("Please select the resource group where your app is residing ",
                                                   items: resourceGroups,
                                                   pageSize: 10);
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                Console.WriteLine();

                appService = await _mediator.Send(new CheckIfAppServiceExistsCommand()
                {
                    AppServiceName = _webappName,
                    ResourceGroup = resourceGroup
                });

                if (appService == null)
                {
                    throw new Exception($"Unable to find the App Service {webappName}");
                }

                _resourceGroupName = resourceGroup;
            }

            _region = await _mediator.Send(new GetRegionNameCommand()
            {
                DefaultRegion = appService.Region,
                ResourceName = resourceType.ToString()
            });
            _resourceGroupName ??= $"{_webappName}-rsg";

            var isRSGCreateSuccessful = await CreateResourceGroup();
            if (isRSGCreateSuccessful)
            {
                await AnsiConsole.Status()
                 .StartAsync("Deploy Resources", async ctx =>
                 {
                     ctx.Spinner(Spinner.Known.Line);
                     ctx.SpinnerStyle(Style.Parse("green"));
                     await DeployResource(ctx, resourceType);
                     ctx.Refresh();
                     ctx.Status = "Done";
                 });
            }

            Console.WriteLine();
            AnsiConsole.MarkupLine($"You can browse to the app using [green]https://{appService.DefaultHostName}[/]");
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

                    var sqlServerName = _webappName;
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
                            AppServiceName = _webappName
                        });
                    }

                    break;

                case ResourceTypes.storage:

                    var storage = await _mediator.Send(new CreateNewStorageAccountCommand()
                    {
                        StorageName = $"{_webappName}storage",
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
                                AppServiceName = _webappName
                            });
                        }
                    }

                    break;

                case ResourceTypes.prostgresql:
                    break;

                default:
                    break;
            }
        }

        private async Task<bool> CreateResourceGroup()
        {
            return await _mediator.Send(new CreateNewResourceGroupCommand()
            {
                ResourceGroupName = _resourceGroupName,
                AzureRegion = _region,
            });
        }
    }
}