using MediatR;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace BumbleBee.Code.Application.AzureSDKWrappers.Create.NewBlessedAppService
{
    public class CreateNewAppServiceWithBlessedImageCommand : IRequest<IWebApp>
    {
        public string ResourceGroupName { get; set; }

        public IAppServicePlan AppServicePlan { get; set; }

        public string AppServiceName { get; set; }

        public Region AzureRegion { get; set; }
    }
}