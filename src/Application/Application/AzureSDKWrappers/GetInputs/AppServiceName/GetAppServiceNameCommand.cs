using MediatR;

namespace BumbleBee.Code.Application.AzureSDKWrappers.GetInputs.AppServiceName
{
    public class GetAppServiceNameCommand : IRequest<string>
    {
        public string AppServiceNameProvided { get; set; }

        public GetAppServiceNameCommand()
        {
        }

        public GetAppServiceNameCommand(string name)
        {
            AppServiceNameProvided = name;
        }
    }
}