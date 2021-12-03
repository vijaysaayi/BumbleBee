using MediatR;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace BumbleBee.Code.Application.AzureSDKWrappers.Create.NewResourceGroup
{
    public class CreateNewResourceGroupCommand : IRequest<bool>
    {
        public string ResourceGroupName { get; set; }

        public Region AzureRegion { get; set; }
    }
}