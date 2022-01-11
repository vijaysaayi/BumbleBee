using MediatR;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace Penguin.Code.Application.AzureSDKWrappers.GetInputs.AzureRegion
{
    public class GetRegionNameCommand : IRequest<Region>
    {
        public Region DefaultRegion { get; set; }

        public string ResourceName { get; set; }

        public GetRegionNameCommand()
        {
            ResourceName = "app";
        }
    }
}