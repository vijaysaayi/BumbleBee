using BumbleBee.Code.Application.Services.Interfaces;
using MediatR;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System.Threading;
using System.Threading.Tasks;

namespace BumbleBee.Code.Application.AzureSDKWrappers.Update.SourceControl
{
    public class UpdateSourceControlCommandHandler : IRequestHandler<UpdateSourceControlCommand, bool>
    {
        private readonly ILogger<UpdateSourceControlCommand> _logger;
        private readonly IAzure _azure;

        public UpdateSourceControlCommandHandler(IAzureService azureService, ILogger<UpdateSourceControlCommand> logger)
        {
            _logger = logger;
            _azure = azureService.Azure;
        }

        public async Task<bool> Handle(UpdateSourceControlCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _azure.WebApps.Inner.CreateOrUpdateSourceControlAsync(request.ResourceGroupName, request.AppServiceName, request.SiteSourceControlInner);
                _logger.LogInformation($"Successfully deployed code for App Service- '{request.AppServiceName}'");
                AnsiConsole.MarkupLine($"Successfully deployed code for App Service- '{request.AppServiceName}'");
                AnsiConsole.MarkupLine($"");
                _logger.LogInformation("");
                return true;
            }
            catch (System.Exception ex)
            {
                _logger.LogDebug(ex.Message);
                _logger.LogError("There was an issue deploying the code");
                AnsiConsole.MarkupLine(Emoji.Known.CrossMark + " There was an issue deploying code from repository " + request.SiteSourceControlInner.RepoUrl);
                AnsiConsole.WriteLine();
                return false;
            }
        }
    }
}