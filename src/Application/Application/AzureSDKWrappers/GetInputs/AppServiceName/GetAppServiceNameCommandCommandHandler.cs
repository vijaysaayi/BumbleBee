using BumbleBee.Application.AzureSDKWrappers.Validation;
using BumbleBee.Code.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BumbleBee.Code.Application.AzureSDKWrappers.GetInputs.AppServiceName
{
    public class GetAppServiceNameCommandCommandHandler : IRequestHandler<GetAppServiceNameCommand, string>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GetAppServiceNameCommandCommandHandler> _logger;
        private readonly ProgressIndicator _progressIndicator;
        private static Random _random = new Random();

        public GetAppServiceNameCommandCommandHandler(IMediator mediator, ILogger<GetAppServiceNameCommandCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            //_progressIndicator = new ProgressIndicator(500);
        }

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        public async Task<string> Handle(GetAppServiceNameCommand request, CancellationToken cancellationToken)
        {
            //_progressIndicator.Start();
            var appNameAvailability = await _mediator.Send(new CheckIfAppNameExistsCommand()
            {
                WebAppName = request.AppServiceNameProvided
            });
            //_progressIndicator.Stop();

            if (appNameAvailability != null)
            {
                if (appNameAvailability.NameAvailable)
                {
                    return request.AppServiceNameProvided;
                }
                else
                {
                    AnsiConsole.MarkupLine($"The name '{request.AppServiceNameProvided}' is already taken. Please specify a unique Name");
                    var answer = AnsiConsole.Confirm("Do you want Bumblebee to add extra characters to make the name your app unique ?");
                    if (!answer)
                    {
                        throw new Exception("Please execute the command again with a unique name");
                    }

                    request.AppServiceNameProvided = $"{request.AppServiceNameProvided}-{RandomString(4)}";
                    AnsiConsole.MarkupLine($"Selecting app name as [green]{request.AppServiceNameProvided}[/]");
                    Console.WriteLine();
                    return request.AppServiceNameProvided;
                }
            }

            throw new Exception("Unable to check if app name is available");
        }
    }
}