using MediatR;
using Microsoft.Azure.Management.AppService.Fluent.Models;

namespace BumbleBee.Code.Application.AzureSDKWrappers.Update.SourceControl
{
    public class UpdateSourceControlCommand : IRequest<bool>
    {
        public string ResourceGroupName { get; set; }

        public string AppServiceName { get; set; }

        public SiteSourceControlInner SiteSourceControlInner { get; set; }
    }
}