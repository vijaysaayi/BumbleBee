using BumbleBee.Code.Application.ExtensionMethods;
using BumbleBee.Code.Application.Services.Interfaces;
using MediatR;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.Storage.Fluent;
using System.Threading;
using System.Threading.Tasks;

namespace BumbleBee.Code.Application.AzureSDKWrappers.Create.NewStorageAccount
{
    public class CreateNewStorageAccountCommandHandler : IRequestHandler<CreateNewStorageAccountCommand, IStorageAccount>
    {
        private readonly IAzure _azure;

        public CreateNewStorageAccountCommandHandler(IAzureService azureService)
        {
            _azure = azureService.Azure;
        }

        public async Task<IStorageAccount> Handle(CreateNewStorageAccountCommand request, CancellationToken cancellationToken)
        {
            string storageName = request.StorageName.Trim().ToLower().Replace("-", "");

            var existingStorage = await _azure.StorageAccounts.GetByResourceGroupAsync(request.ResourceGroup, storageName, cancellationToken);
            if (existingStorage == null)
            {
                var newStorageAccount = await _azure.StorageAccounts.Define(storageName).WithRegion(request.AzureRegion)
                                                            .WithExistingResourceGroup(request.ResourceGroup)
                                                            .CreateAsync();
                AnsiConsoleExtensionMethods.Display($"Successfully created new Storage Account ([green]{newStorageAccount.Name}[/])");
                return newStorageAccount;
            }

            AnsiConsoleExtensionMethods.Display($"Storage account {storageName} already exists in {request.ResourceGroup}. Using it");
            return existingStorage;
        }
    }
}