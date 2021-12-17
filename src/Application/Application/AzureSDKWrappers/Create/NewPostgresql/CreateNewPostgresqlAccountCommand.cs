using BumbleBee.Code.Application.Services.Interfaces;
using MediatR;
using Microsoft.Azure.Management.Fluent;
using System.Threading;
using System.Threading.Tasks;

namespace BumbleBee.Code.Application.AzureSDKWrappers.Create.NewPostgresql
{
    public class CreateNewPostgresqlAccountCommand : IRequest<Unit>
    {
        public string MyProperty { get; set; }
    }

    public class CreateNewPostgresqlAccountCommandHandler : IRequestHandler<CreateNewPostgresqlAccountCommand, Unit>
    {
        private readonly IAzure _azure;

        public CreateNewPostgresqlAccountCommandHandler(IAzureService azureService)
        {
            _azure = azureService.Azure;
        }

        public async Task<Unit> Handle(CreateNewPostgresqlAccountCommand request, CancellationToken cancellationToken)
        {
            return await Task.FromResult(Unit.Value);
        }
    }
}