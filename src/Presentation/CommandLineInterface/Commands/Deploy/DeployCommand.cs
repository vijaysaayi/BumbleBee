using BumbleBee.Code.Application.AzureSDKWrappers.Create.NewAppServicePlan;
using BumbleBee.Code.Application.AzureSDKWrappers.Create.NewAzureContainerRegistry;
using BumbleBee.Code.Application.AzureSDKWrappers.Create.NewResourceGroup;
using BumbleBee.Code.Application.AzureSDKWrappers.Create.NewWebAppForContainers;
using BumbleBee.Code.Application.AzureSDKWrappers.Deploy.ScheduleACRBuildpackTask;
using BumbleBee.Code.Application.AzureSDKWrappers.GetInputs.ACRScheduledRunStatus;
using BumbleBee.Code.Application.AzureSDKWrappers.GetInputs.AzureRegion;
using BumbleBee.Code.Application.ExtensionMethods;
using CommandDotNet;
using MediatR;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.ContainerRegistry.Fluent;
using Microsoft.Azure.Management.ContainerRegistry.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Spectre.Console;
using System.Threading;
using System.Threading.Tasks;

namespace BumbleBee.CommandLineInterface.Commands.Deploy
{
    [Command(Name = "deploy",
             Usage = "deploy [command]",
             Description = "Deploy new WebApp")]
    public class DeployCommand
    {
        private readonly string _imageName = "buildpackimage:latest";
        private readonly IMediator _mediator;
        private string _resourceGroupName;
        private string _appServicePlanName;
        private Region _region;
        private string _webappName;
        private string _registryName;

        public DeployCommand(IMediator mediator)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }

        [DefaultMethod]
        public async Task NewAppService(
            [Option(LongName = "name", ShortName = "n", Description = "Name of the App Service")] string webappName,
            [Option(LongName = "respository", ShortName = "r", Description = "Url of repository")] string repositoryUrl,
            [Option(LongName = "buildpack", ShortName = "b", Description = "Buildpack")] string buildpack
        )
        {
            if (string.IsNullOrWhiteSpace(webappName))
            {
                webappName = AnsiConsole.Ask<string>("Enter the [green]name[/] of App Service?");
            }

            _webappName = $"{webappName}{StringExtensionMethods.RandomString(4) }";
            _registryName = $"{_webappName}acr";
            AnsiConsole.WriteLine(_registryName);
            _resourceGroupName = $"{_webappName}-rsg";
            _appServicePlanName = $"{_webappName}-asp";
            _region = await _mediator.Send(new GetRegionNameCommand());

            var isRSGCreateSuccessful = await CreateResourceGroup();
            if (isRSGCreateSuccessful)
            {
                var registry = await CreateNewAzureContainerRegistry();
                AnsiConsole.WriteLine("ACR created");
                if (registry != null)
                {
                    AnsiConsole.WriteLine("Scheduling ACR task");
                    var buildTaskJob = await ScheduleBuildPackTask(registry.LoginServerUrl, repositoryUrl, buildpack);

                    if (buildTaskJob != null)
                    {
                        AnsiConsole.WriteLine("Checking Status");
                        AnsiConsole.MarkupLine($"Task Name : {buildTaskJob.TaskName}");
                        AnsiConsole.MarkupLine($"Status : {buildTaskJob.Status}");
                        AnsiConsole.MarkupLine($"CPU : {buildTaskJob.Cpu}");
                        AnsiConsole.MarkupLine($"Provisioning state : {buildTaskJob.ProvisioningState}");
                        AnsiConsole.MarkupLine($"Last updated time : {buildTaskJob.LastUpdatedTime}");
                        AnsiConsole.MarkupLine($"Run Id : {buildTaskJob.RunId}");

                        var status = buildTaskJob.Status;
                        var previousLog = "";
                        while (status == RunStatus.Queued || status == RunStatus.Running || status == RunStatus.Started)
                        {
                            var currentLog = await _mediator.Send(new GetACRScheduledRunStatusRequest()
                            {
                                ResourceGroupName = _resourceGroupName,
                                RegistryName = _registryName,
                                RunId = buildTaskJob.RunId,
                            });
                            if (!string.IsNullOrEmpty(previousLog))
                                currentLog = currentLog.Replace(previousLog, "");
                            AnsiConsole.WriteLine(currentLog);
                            previousLog = currentLog;

                            Thread.Sleep(3000);

                            buildTaskJob = await buildTaskJob.RefreshAsync();
                            status = buildTaskJob.Status;
                        }

                        AnsiConsole.WriteLine(status.Value);
                        if (status == RunStatus.Failed)
                        {
                            var currentLog = await _mediator.Send(new GetACRScheduledRunStatusRequest()
                            {
                                ResourceGroupName = _resourceGroupName,
                                RegistryName = _registryName,
                                RunId = buildTaskJob.RunId,
                            });

                            AnsiConsole.MarkupLine($"[red]{currentLog}[/]");
                        }

                        if (status == RunStatus.Succeeded)
                        {
                            AnsiConsole.WriteLine("Build succeeded. Deploying App Service");
                            var acrCredentials = await registry.GetCredentialsAsync();
                            var appServicePlan = await CreateNewAppServicePlan();
                            if (appServicePlan != null)
                            {
                                AnsiConsole.WriteLine("Creating WebApp for Containers");
                                IWebApp appService = await CreateWebAppForContainers(registry, acrCredentials, appServicePlan);

                                if (appService != null)
                                {
                                    AnsiConsole.MarkupLine($"You can browse to the app using [green]https://{appService.DefaultHostName}[/]");
                                }
                            }
                        }
                    }
                }
            }

            AnsiConsole.WriteLine("creating new ACR");
        }

        private async Task<IWebApp> CreateWebAppForContainers(IRegistry registry, IRegistryCredentials acrCredentials, IAppServicePlan appServicePlan)
        {
            return await _mediator.Send(new CreateNewWebAppForContainersServiceCommand()
            {
                ResourceGroupName = _resourceGroupName,
                AppServiceName = _webappName,
                AppServicePlan = appServicePlan,
                ServerUrl = registry.LoginServerUrl,
                ImageAndTagName = $"{registry.Name}.azurecr.io/{_imageName}",
                AcrCredentials = acrCredentials,
                AzureRegion = _region
            });
        }

        private async Task<IRegistryTaskRun> ScheduleBuildPackTask(string registryUrl, string repositoryUrl, string buildpack)
        {
            if (string.IsNullOrWhiteSpace(repositoryUrl))
            {
                repositoryUrl = AnsiConsole.Ask<string>("Enter the [green]url[/] of Github repository :");
            }

            if (string.IsNullOrWhiteSpace(buildpack))
            {
                buildpack = AnsiConsole.Ask<string>("Enter the [green]name[/] of buildpack (eg. heroku/buildpacks:18) :");
            }

            return await _mediator.Send(new ScheduleACRBuildpackTaskCommand()
            {
                RegistryName = _registryName,
                ResourceGroupName = _resourceGroupName,
                BuilderName = buildpack,
                ImageName = _imageName,
                SourceLocation = repositoryUrl,
                RegistryUrl = registryUrl
            });
        }

        private async Task<IRegistry> CreateNewAzureContainerRegistry()
        {
            return await _mediator.Send(new CreateNewAzureContainerRegistryCommand()
            {
                AzureContainerRegistryName = _registryName,
                ResourceGroupName = _resourceGroupName,
                Location = _region
            });
        }

        private async Task<IAppServicePlan> CreateNewAppServicePlan()
        {
            return await _mediator.Send(new CreateNewAppServicePlanCommand()
            {
                ResourceGroupName = _resourceGroupName,
                AppServicePlanName = _appServicePlanName,
                AzureRegion = _region,
            });
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