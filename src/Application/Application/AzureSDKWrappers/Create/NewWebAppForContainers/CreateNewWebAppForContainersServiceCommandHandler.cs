using BumbleBee.Code.Application.ExtensionMethods;
using BumbleBee.Code.Application.Services.Interfaces;
using MediatR;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.ContainerRegistry.Fluent;
using Microsoft.Azure.Management.Fluent;
using System.Threading;
using System.Threading.Tasks;

namespace BumbleBee.Code.Application.AzureSDKWrappers.Create.NewWebAppForContainers
{
    public class CreateNewWebAppForContainersServiceCommandHandler : IRequestHandler<CreateNewWebAppForContainersServiceCommand, IWebApp>
    {
        private readonly IAzure _azure;

        public CreateNewWebAppForContainersServiceCommandHandler(IAzureService azureService)
        {
            _azure = azureService.Azure;
        }

        public async Task<IWebApp> Handle(CreateNewWebAppForContainersServiceCommand request, CancellationToken cancellationToken)
        {
            var acrCredentials = request.AcrCredentials;
            var appService = await _azure.WebApps.Define(request.AppServiceName)
                .WithExistingLinuxPlan(request.AppServicePlan)
                .WithExistingResourceGroup(request.ResourceGroupName)
                .WithPrivateRegistryImage(request.ImageAndTagName, request.ServerUrl)
                .WithCredentials(acrCredentials.Username, acrCredentials.AccessKeys[AccessKeyType.Primary])
                .WithAppSetting("WEBSITES_PORT", "8000")
                .CreateAsync();
            AnsiConsoleExtensionMethods.Display($"Successfully deployed App Service");
            return appService;
        }
    }
}