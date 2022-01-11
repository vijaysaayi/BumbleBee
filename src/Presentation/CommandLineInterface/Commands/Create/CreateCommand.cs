using Penguin.Code.Application.AzureSDKWrappers.Create.NewAppServicePlan;
using Penguin.Code.Application.AzureSDKWrappers.Create.NewBlessedAppService;
using Penguin.Code.Application.AzureSDKWrappers.Create.NewResourceGroup;
using Penguin.Code.Application.AzureSDKWrappers.GetInputs.AzureRegion;
using Penguin.Code.Application.ExtensionMethods;
using CommandDotNet;
using LibGit2Sharp;
using MediatR;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using RandomNameGeneratorLibrary;
using Spectre.Console;
using System;
using System.IO;
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
        public async Task CreateNewWebApp(CancellationToken cancellationToken)
        {
            _appName = GetRandomName();
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

                                             AnsiConsoleExtensionMethods.Display($"Added git remote : [green]{publishProfile.GitUrl}[/]");
                                             Console.WriteLine("");
                                             AnsiConsole.MarkupLine($"You can browse to the app using [green]https://{appService.DefaultHostName}[/]");
                                         }
                                     }
                                 }
                             })
                             ;
        }

        private string GetRandomName()
        {
            var placeGenerator = new PlaceNameGenerator();
            var name = placeGenerator.GenerateRandomPlaceName();
            name = name.ToLowerInvariant()
                       .Replace(" ", "")
                       .Replace(".", "")
                       .Replace("'", "");

            return $"{name}-{StringExtensionMethods.RandomString(4)}";
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
    }
}