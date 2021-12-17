using MediatR;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Extensions.Logging;
using Sharprompt;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BumbleBee.Code.Application.AzureSDKWrappers.GetInputs.AzureRegion
{
    public class GetRegionNameCommandHandler : IRequestHandler<GetRegionNameCommand, Region>
    {
        private readonly ILogger<GetRegionNameCommandHandler> _logger;

        public GetRegionNameCommandHandler(ILogger<GetRegionNameCommandHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Region> Handle(GetRegionNameCommand request, CancellationToken cancellationToken)
        {
            var availableRegions = Region.Values;
            Console.OutputEncoding = System.Text.Encoding.ASCII;
            var selectedRegion = Prompt.Select($"Please select the region where you want {request.ResourceName} to be deployed ",
                                               items: availableRegions,
                                               pageSize: 10,
                                               defaultValue: request.DefaultRegion);
            //var selectedRegion = AnsiConsole.Prompt(
            //    new SelectionPrompt<Region>()
            //    .Title("Please select the region where you want the App to be deployed ")
            //    .PageSize(10)
            //    .AddChoices(availableRegions)
            //    );
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine();
            return await Task.FromResult(selectedRegion);
        }
    }
}