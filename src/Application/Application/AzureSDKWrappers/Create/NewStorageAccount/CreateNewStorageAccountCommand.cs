using MediatR;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.Storage.Fluent;

namespace BumbleBee.Code.Application.AzureSDKWrappers.Create.NewStorageAccount
{
    public class CreateNewStorageAccountCommand : IRequest<IStorageAccount>
    {
        public string StorageName { get; set; }

        public Region AzureRegion { get; set; }

        public string ResourceGroup { get; set; }
    }
}