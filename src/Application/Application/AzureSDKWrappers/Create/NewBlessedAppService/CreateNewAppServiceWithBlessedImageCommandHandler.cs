using BumbleBee.Code.Application.ExtensionMethods;
using BumbleBee.Code.Application.Services;
using BumbleBee.Code.Application.Services.Interfaces;
using MediatR;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace BumbleBee.Code.Application.AzureSDKWrappers.Create.NewBlessedAppService
{
    public class CreateNewAppServiceWithBlessedImageCommandHandler : IRequestHandler<CreateNewAppServiceWithBlessedImageCommand, IWebApp>
    {
        private readonly IAzure _azure;
        private readonly ILogger<CreateNewAppServiceWithBlessedImageCommandHandler> _logger;
        private readonly ProgressIndicator _progressIndicator;
        private CreateNewAppServiceWithBlessedImageCommand _request;

        public CreateNewAppServiceWithBlessedImageCommandHandler(IAzureService azureService, ILogger<CreateNewAppServiceWithBlessedImageCommandHandler> logger)
        {
            _azure = azureService.Azure;
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        public async Task<IWebApp> Handle(CreateNewAppServiceWithBlessedImageCommand request, CancellationToken cancellationToken)
        {
            var appService = await _azure.WebApps.Define(request.AppServiceName)
                .WithExistingLinuxPlan(request.AppServicePlan)
                .WithExistingResourceGroup(request.ResourceGroupName)
                .WithBuiltInImage(UpdatedRuntimeStack.Python_3_8)
                .CreateAsync();

            AnsiConsoleExtensionMethods.Display($"Successfully deployed App Service");

            return appService;
        }
    }
}