using MediatR;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace BumbleBee.Code.Application.AzureSDKWrappers.Create.NewAppService
{
    public class CreateNewAppServiceCommand : IRequest<IWebApp>
    {
        public string ResourceGroupName { get; set; }

        public IAppServicePlan AppServicePlan { get; set; }

        public string AppServiceName { get; set; }

        public Region AzureRegion { get; set; }
    }
}