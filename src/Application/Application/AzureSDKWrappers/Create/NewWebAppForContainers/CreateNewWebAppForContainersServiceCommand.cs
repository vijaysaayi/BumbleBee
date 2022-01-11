using MediatR;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.ContainerRegistry.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace Penguin.Code.Application.AzureSDKWrappers.Create.NewWebAppForContainers
{
    public class CreateNewWebAppForContainersServiceCommand : IRequest<IWebApp>
    {
        public string ResourceGroupName { get; set; }

        public IAppServicePlan AppServicePlan { get; set; }

        public string AppServiceName { get; set; }

        public Region AzureRegion { get; set; }

        public string ImageAndTagName { get; set; }

        public string ServerUrl { get; set; }

        public IRegistryCredentials AcrCredentials { get; set; }

        public int Port { get; set; }

        public CreateNewWebAppForContainersServiceCommand()
        {
            Port = 8000;
        }
    }
}