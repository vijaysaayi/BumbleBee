using Penguin.Code.Application.ExtensionMethods;
using Penguin.Code.Application.Services.Interfaces;
using MediatR;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Penguin.Code.Application.AzureSDKWrappers.Create.NewResourceGroup
{
    public class CreateNewResourceGroupCommandHandler : IRequestHandler<CreateNewResourceGroupCommand, bool>
    {
        private readonly ILogger<CreateNewResourceGroupCommandHandler> _logger;
        private readonly IAzure _azure;

        public CreateNewResourceGroupCommandHandler(IAzureService azureService, ILogger<CreateNewResourceGroupCommandHandler> logger)
        {
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            _azure = azureService.Azure;
        }

        public async Task<bool> Handle(CreateNewResourceGroupCommand request, CancellationToken cancellationToken)
        {
            string resourceGroupName = request.ResourceGroupName;
            await _azure.ResourceGroups
                        .Define(resourceGroupName)
                        .WithRegion(request.AzureRegion)
                        .CreateAsync();

            AnsiConsoleExtensionMethods.Display($"Successfully created Resource Group");
            return true;
        }
    }
}