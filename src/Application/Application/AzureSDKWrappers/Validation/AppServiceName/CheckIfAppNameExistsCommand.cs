using MediatR;

namespace Penguin.Application.AzureSDKWrappers.Validation
{
    public class CheckIfAppNameExistsCommand : IRequest<CheckIfAppNameExistsResponse>
    {
        public string WebAppName { get; set; }
    }
}