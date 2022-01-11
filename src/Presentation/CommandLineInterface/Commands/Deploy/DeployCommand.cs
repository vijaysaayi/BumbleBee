using Penguin.Code.Application.AzureSDKWrappers.Create.NewAppServicePlan;
using Penguin.Code.Application.AzureSDKWrappers.Create.NewAzureContainerRegistry;
using Penguin.Code.Application.AzureSDKWrappers.Create.NewResourceGroup;
using Penguin.Code.Application.AzureSDKWrappers.Create.NewWebAppForContainers;
using Penguin.Code.Application.AzureSDKWrappers.Deploy.ScheduleACRBuildpackTask;
using Penguin.Code.Application.AzureSDKWrappers.GetInputs.ACRScheduledRunStatus;
using Penguin.Code.Application.AzureSDKWrappers.GetInputs.AzureRegion;
using Penguin.Code.Application.ExtensionMethods;
using CommandDotNet;
using MediatR;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.ContainerRegistry.Fluent;
using Microsoft.Azure.Management.ContainerRegistry.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Spectre.Console;
using System.Threading;
using System.Threading.Tasks;
using Penguin.Code.Application.HelperMethods.GetRandomName;

namespace Penguin.CommandLineInterface.Commands.Deploy
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
        private int _port;
        private Region _region;
        private string _webappName;
        private string _registryName;
        private CancellationToken _cancellationToken;

        public DeployCommand(IMediator mediator)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }

        [DefaultMethod]
        public async Task NewAppService(
            [Option(LongName = "name", ShortName = "n", Description = "Name of the App Service")] string webappName,
            [Option(LongName = "respository", ShortName = "r", Description = "Url of repository")] string repositoryUrl,
            [Option(LongName = "builder", ShortName = "b", Description = "Name of the builder")] string builder,
            [Option(LongName = "port", ShortName = "p", Description = "The port on which App Service is listening")] int port,
            CancellationToken cancellationToken
        )
        {
            _cancellationToken = cancellationToken;
            if (string.IsNullOrWhiteSpace(webappName))
            {
                webappName = await GetRandomName();
            }

            if (port == 0)
            {
                port = AnsiConsole.Ask<int>("Enter the [green]port[/] on which the app listening?");
            }

            _webappName = $"{webappName}{StringExtensionMethods.RandomString(4) }";
            _registryName = $"{_webappName}acr";
            AnsiConsole.WriteLine(_registryName);
            _resourceGroupName = $"{_webappName}-rsg";
            _appServicePlanName = $"{_webappName}-asp";
            _port = port;
            _region = await _mediator.Send(new GetRegionNameCommand(), _cancellationToken);

            var isRSGCreateSuccessful = await CreateResourceGroup();
            if (isRSGCreateSuccessful)
            {
                var registry = await CreateNewAzureContainerRegistry();
                AnsiConsole.WriteLine("ACR created");
                if (registry != null)
                {
                    AnsiConsole.WriteLine("Scheduling ACR task");
                    var buildTaskJob = await ScheduleBuildPackTask(registry.LoginServerUrl, repositoryUrl, builder);

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
                AzureRegion = _region,
                Port = _port
            }, _cancellationToken);
        }

        private async Task<IRegistryTaskRun> ScheduleBuildPackTask(string registryUrl, string repositoryUrl, string builder)
        {
            if (string.IsNullOrWhiteSpace(repositoryUrl))
            {
                repositoryUrl = AnsiConsole.Ask<string>("Enter the [green]url[/] of Github repository :");
            }

            if (string.IsNullOrWhiteSpace(builder))
            {
                builder = AnsiConsole.Ask<string>("Enter the [green]name[/] of builder :");
            }

            return await _mediator.Send(new ScheduleACRBuildpackTaskCommand()
            {
                RegistryName = _registryName,
                ResourceGroupName = _resourceGroupName,
                BuilderName = builder,
                ImageName = _imageName,
                SourceLocation = repositoryUrl,
                RegistryUrl = registryUrl
            }, _cancellationToken);
        }

        private async Task<IRegistry> CreateNewAzureContainerRegistry()
        {
            return await _mediator.Send(new CreateNewAzureContainerRegistryCommand()
            {
                AzureContainerRegistryName = _registryName,
                ResourceGroupName = _resourceGroupName,
                Location = _region
            }, _cancellationToken);
        }

        private async Task<IAppServicePlan> CreateNewAppServicePlan()
        {
            return await _mediator.Send(new CreateNewAppServicePlanCommand()
            {
                ResourceGroupName = _resourceGroupName,
                AppServicePlanName = _appServicePlanName,
                AzureRegion = _region,
            }, _cancellationToken);
        }

        private async Task<bool> CreateResourceGroup()
        {
            return await _mediator.Send(new CreateNewResourceGroupCommand()
            {
                ResourceGroupName = _resourceGroupName,
                AzureRegion = _region,
            }, _cancellationToken);
        }

        private async Task<string> GetRandomName()
        {
            return await _mediator.Send(new GetRandomNameCommand()
            {
                Length = 4
            });
        }
    }
}