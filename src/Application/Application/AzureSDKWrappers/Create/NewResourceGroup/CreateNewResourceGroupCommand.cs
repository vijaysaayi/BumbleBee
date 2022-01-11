using MediatR;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace Penguin.Code.Application.AzureSDKWrappers.Create.NewResourceGroup
{
    public class CreateNewResourceGroupCommand : IRequest<bool>
    {
        public string ResourceGroupName { get; set; }

        public Region AzureRegion { get; set; }
    }
}