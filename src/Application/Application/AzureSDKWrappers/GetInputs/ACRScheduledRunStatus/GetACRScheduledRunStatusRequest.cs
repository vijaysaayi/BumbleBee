using MediatR;

namespace BumbleBee.Code.Application.AzureSDKWrappers.GetInputs.ACRScheduledRunStatus
{
    public class GetACRScheduledRunStatusRequest : IRequest<string>
    {
        public string ResourceGroupName { get; set; }

        public string RegistryName { get; set; }

        public string RunId { get; set; }
    }
}