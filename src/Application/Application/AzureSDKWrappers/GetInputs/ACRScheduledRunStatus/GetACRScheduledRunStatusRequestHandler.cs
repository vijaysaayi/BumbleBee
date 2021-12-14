using BumbleBee.Code.Application.Services.Interfaces;
using MediatR;
using Microsoft.Azure.Management.Fluent;
using System.Threading;
using System.Threading.Tasks;

namespace BumbleBee.Code.Application.AzureSDKWrappers.GetInputs.ACRScheduledRunStatus
{
    public class GetACRScheduledRunStatusRequestHandler : IRequestHandler<GetACRScheduledRunStatusRequest, string>
    {
        private readonly IAzure _azure;
        private readonly IHttpClientService _httpClientService;

        public GetACRScheduledRunStatusRequestHandler(IAzureService azureService, IHttpClientService httpClientService)
        {
            _azure = azureService.Azure;
            _httpClientService = httpClientService;
        }

        public async Task<string> Handle(GetACRScheduledRunStatusRequest request, CancellationToken cancellationToken)
        {
            string url = await _azure.RegistryTaskRuns.GetLogSasUrlAsync(request.ResourceGroupName, request.RegistryName, request.RunId, cancellationToken);

            var response = await _httpClientService.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }
    }
}