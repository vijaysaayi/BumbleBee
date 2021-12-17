using BumbleBee.Code.Application.Services.Interfaces;
using MediatR;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.Fluent;
using System.Threading;
using System.Threading.Tasks;

namespace BumbleBee.Code.Application.AzureSDKWrappers.Validation.AppServiceExists
{
    public class CheckIfAppServiceExistsCommandHandler : IRequestHandler<CheckIfAppServiceExistsCommand, IWebApp>
    {
        private readonly IAzure _azure;

        public CheckIfAppServiceExistsCommandHandler(IAzureService azureService)
        {
            _azure = azureService.Azure;
        }

        public async Task<IWebApp> Handle(CheckIfAppServiceExistsCommand request, CancellationToken cancellationToken)
        {
            string resourceGroup = request.ResourceGroup ?? $"{request.AppServiceName}-rsg";
            var appService = await _azure.AppServices.WebApps.GetByResourceGroupAsync(resourceGroup, request.AppServiceName, cancellationToken);

            return appService;
        }
    }
}