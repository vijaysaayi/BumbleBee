using Penguin.Code.Application.Services.Interfaces;
using MediatR;
using Microsoft.Azure.Management.ContainerRegistry.Fluent;
using Microsoft.Azure.Management.Fluent;
using System.Threading;
using System.Threading.Tasks;

namespace Penguin.Code.Application.AzureSDKWrappers.Create.NewAzureContainerRegistry
{
    public class CreateNewAzureContainerRegistryCommandHandler : IRequestHandler<CreateNewAzureContainerRegistryCommand, IRegistry>
    {
        private readonly IAzure _azure;

        public CreateNewAzureContainerRegistryCommandHandler(IAzureService azureService)
        {
            _azure = azureService.Azure;
        }

        public async Task<IRegistry> Handle(CreateNewAzureContainerRegistryCommand request, CancellationToken cancellationToken)
        {
            IRegistry registry = await _azure.ContainerRegistries.Define(request.AzureContainerRegistryName)
                .WithRegion(request.Location)
                .WithExistingResourceGroup(request.ResourceGroupName)
                .WithBasicSku()
                .WithRegistryNameAsAdminUser()
                .CreateAsync();

            return registry;
        }
    }
}