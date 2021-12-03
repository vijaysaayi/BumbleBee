using MediatR;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace BumbleBee.Code.Application.AzureSDKWrappers.GetInputs.AzureRegion
{
    public class GetRegionNameCommand : IRequest<Region>
    {
    }
}