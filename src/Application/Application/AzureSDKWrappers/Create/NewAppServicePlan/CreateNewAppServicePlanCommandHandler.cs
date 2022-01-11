using Penguin.Code.Application.ExtensionMethods;
using Penguin.Code.Application.Services.Interfaces;
using MediatR;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Penguin.Code.Application.AzureSDKWrappers.Create.NewAppServicePlan
{
    public class CreateNewAppServicePlanCommandHandler : IRequestHandler<CreateNewAppServicePlanCommand, IAppServicePlan>
    {
        private readonly IAzure _azure;
        private readonly ILogger<CreateNewAppServicePlanCommandHandler> _logger;

        public CreateNewAppServicePlanCommandHandler(IAzureService azureService, ILogger<CreateNewAppServicePlanCommandHandler> logger)
        {
            _azure = azureService.Azure;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IAppServicePlan> Handle(CreateNewAppServicePlanCommand request, CancellationToken cancellationToken)
        {
            var appservicePlanInner = await _azure.AppServices.AppServicePlans.Manager.AppServicePlans
                .Inner.GetAsync(request.ResourceGroupName, request.AppServicePlanName);
            IAppServicePlan appServicePlan;

            if (appservicePlanInner == null)
            {
                try
                {
                    appServicePlan = await CreateNewAppServicePlanAsync(request);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.HelpLink);
                    throw;
                }
            }
            else
            {
                AnsiConsoleExtensionMethods.Display("Reusing the existing App Service Plan", disableCheckBox:true);
                var appServicePlanId = appservicePlanInner.Id;
                appServicePlan = await _azure.AppServices.AppServicePlans.GetByIdAsync(appServicePlanId);
            }
            return appServicePlan;
        }

        private async Task<IAppServicePlan> CreateNewAppServicePlanAsync(CreateNewAppServicePlanCommand request)
        {
            var appServicePlan = await _azure.AppServices.AppServicePlans
                        .Define(request.AppServicePlanName)
                        .WithRegion(request.AzureRegion)
                        .WithExistingResourceGroup(request.ResourceGroupName)
                        .WithPricingTier(pricingTier: Microsoft.Azure.Management.AppService.Fluent.PricingTier.BasicB1)
                        .WithOperatingSystem(Microsoft.Azure.Management.AppService.Fluent.OperatingSystem.Linux)
                        .CreateAsync();

            AnsiConsoleExtensionMethods.Display($"Successfully deployed App Service Plan ");            
            return appServicePlan;
        }
    }
}