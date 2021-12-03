using MediatR;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace BumbleBee.Code.Application.AzureSDKWrappers.Create.NewAppServicePlan
{
    public class CreateNewAppServicePlanCommand : IRequest<IAppServicePlan>
    {
        public string ResourceGroupName { get; set; }

        public string AppServicePlanName { get; set; }

        public Region AzureRegion { get; set; }
    }
}