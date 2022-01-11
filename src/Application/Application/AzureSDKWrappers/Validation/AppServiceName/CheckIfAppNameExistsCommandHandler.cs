using Penguin.Code.Application.Services.Interfaces;
using MediatR;
using Microsoft.Azure.Management.AppService.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Penguin.Application.AzureSDKWrappers.Validation
{
    public class CheckIfAppNameExistsCommandHandler : IRequestHandler<CheckIfAppNameExistsCommand, CheckIfAppNameExistsResponse>
    {
        private readonly IAzure _azure;
        private readonly ILogger<CheckIfAppNameExistsCommandHandler> _logger;

        public CheckIfAppNameExistsCommandHandler(IAzureService azureService, ILogger<CheckIfAppNameExistsCommandHandler> logger)
        {
            _azure = azureService.Azure;
            _logger = logger;
        }

        public async Task<CheckIfAppNameExistsResponse> Handle(CheckIfAppNameExistsCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Checking if app with the name '{request.WebAppName}' is available");
            var result = await _azure.AppServices.Inner.CheckNameAvailabilityWithHttpMessagesAsync(
                name: request.WebAppName,
                type: CheckNameResourceTypes.MicrosoftWebSites,
                isFqdn: false,
                cancellationToken: cancellationToken);

            if (result.Response.IsSuccessStatusCode)
            {
                var content = await result.Response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(content))
                {
                    return JsonConvert.DeserializeObject<CheckIfAppNameExistsResponse>(content);
                }

                _logger.LogError(content);
            }

            throw new Exception("Unable to check if Name is available");
        }
    }
}