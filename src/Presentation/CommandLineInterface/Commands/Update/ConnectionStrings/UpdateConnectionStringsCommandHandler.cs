using BumbleBee.Code.Application.ExtensionMethods;
using BumbleBee.Code.Application.Services.Interfaces;
using MediatR;
using Microsoft.Azure.Management.AppService.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Rest.Azure;
using System.Threading;
using System.Threading.Tasks;

namespace BumbleBee.CommandLineInterface.Commands.Update.ConnectionStrings
{
    public class UpdateConnectionStringsCommandHandler : IRequestHandler<UpdateConnectionStringsCommand, AzureOperationResponse<ConnectionStringDictionaryInner>>
    {
        private readonly IAzure _azure;

        public UpdateConnectionStringsCommandHandler(IAzureService azureService)
        {
            _azure = azureService.Azure;
        }

        public async Task<AzureOperationResponse<ConnectionStringDictionaryInner>> Handle(UpdateConnectionStringsCommand request, CancellationToken cancellationToken)
        {
            var response = await _azure.WebApps.Inner.UpdateConnectionStringsWithHttpMessagesAsync(request.ResourceGroup, request.AppServiceName, request.ConnectionStrings, null, cancellationToken);
            AnsiConsoleExtensionMethods.Display($"Succesfully updated Connection strings for app {request.AppServiceName}");

            return response;
        }
    }
}