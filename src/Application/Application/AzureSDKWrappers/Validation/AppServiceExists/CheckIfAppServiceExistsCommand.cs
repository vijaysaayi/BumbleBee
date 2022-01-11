using MediatR;
using Microsoft.Azure.Management.AppService.Fluent;

namespace Penguin.Code.Application.AzureSDKWrappers.Validation.AppServiceExists
{
    public class CheckIfAppServiceExistsCommand : IRequest<IWebApp>
    {
        public string AppServiceName { get; set; }

        public string ResourceGroup { get; set; }
    }
}