using MediatR;

namespace BumbleBee.Application.AzureSDKWrappers.Validation
{
    public class CheckIfAppNameExistsCommand : IRequest<CheckIfAppNameExistsResponse>
    {
        public string WebAppName { get; set; }
    }
}