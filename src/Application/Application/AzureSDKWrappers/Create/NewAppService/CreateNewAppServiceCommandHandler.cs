using BumbleBee.Code.Application.ExtensionMethods;
using BumbleBee.Code.Application.Services;
using BumbleBee.Code.Application.Services.Interfaces;
using MediatR;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System.Threading;
using System.Threading.Tasks;

namespace BumbleBee.Code.Application.AzureSDKWrappers.Create.NewAppService
{
    public class CreateNewAppServiceCommandHandler : IRequestHandler<CreateNewAppServiceCommand, IWebApp>
    {
        private readonly IAzure _azure;
        private readonly ILogger<CreateNewAppServiceCommandHandler> _logger;
        private readonly ProgressIndicator _progressIndicator;
        private CreateNewAppServiceCommand _request;

        public CreateNewAppServiceCommandHandler(IAzureService azureService, ILogger<CreateNewAppServiceCommandHandler> logger)
        {
            _azure = azureService.Azure;
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            //_progressIndicator = new ProgressIndicator(500);
        }

        public async Task<IWebApp> Handle(CreateNewAppServiceCommand request, CancellationToken cancellationToken)
        {
            var appService = await _azure.WebApps.Define(request.AppServiceName)
                .WithExistingLinuxPlan(request.AppServicePlan)
                .WithExistingResourceGroup(request.ResourceGroupName)
                .WithBuiltInImage(RuntimeStack.Python_3_7)
                .CreateAsync();
            AnsiConsoleExtensionMethods.Display($"Successfully deployed App Service");            
            return appService;
        }
    }
}