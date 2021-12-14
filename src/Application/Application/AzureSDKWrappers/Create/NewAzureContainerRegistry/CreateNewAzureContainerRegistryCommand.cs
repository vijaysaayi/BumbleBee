using MediatR;
using Microsoft.Azure.Management.ContainerRegistry.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace BumbleBee.Code.Application.AzureSDKWrappers.Create.NewAzureContainerRegistry
{
    public class CreateNewAzureContainerRegistryCommand : IRequest<IRegistry>
    {
        public Region Location { get; set; }

        public string ResourceGroupName { get; set; }

        public string AzureContainerRegistryName { get; set; }
    }
}