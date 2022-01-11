using Penguin.Code.Application.ExtensionMethods;
using Penguin.Code.Application.Services.Interfaces;
using MediatR;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.Sql.Fluent;
using Microsoft.Azure.Management.Sql.Fluent.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Penguin.Code.Application.AzureSDKWrappers.Create.NewSqlServer
{
    public class CreateNewSqlServerCommandHandler : IRequestHandler<CreateNewSqlServerCommand, ISqlServer>
    {
        private readonly IAzure _azure;

        public CreateNewSqlServerCommandHandler(IAzureService azureService)
        {
            _azure = azureService.Azure;
        }

        public async Task<ISqlServer> Handle(CreateNewSqlServerCommand request, CancellationToken cancellationToken)
        {
            var existingSqlServer = await _azure.SqlServers.GetByResourceGroupAsync(request.ResourceGroupName, request.SqlServerName, cancellationToken);
            if (existingSqlServer == null)
            {
                var newSqlServer = await _azure.SqlServers.Define(request.SqlServerName)
                    .WithRegion(request.AzureRegion)
                    .WithExistingResourceGroup(request.ResourceGroupName)
                    .WithAdministratorLogin(request.UserName)
                    .WithAdministratorPassword(request.Password)
                .DefineDatabase(request.DatabaseName)
                .WithEdition(DatabaseEdition.Standard)
                .WithCollation("SQL_Latin1_General_CP1_CI_AS")
                .WithServiceObjective(ServiceObjectiveName.S1)
                .Attach()
                .CreateAsync();
                AnsiConsoleExtensionMethods.Display($"Successfully created new Sql Server ([green]{request.SqlServerName}[/]) and Database ([green]{request.DatabaseName}[/])");
                return newSqlServer;
            }
            AnsiConsoleExtensionMethods.Display($"SQL server {request.SqlServerName} already exists in {request.ResourceGroupName}. Using it");
            return existingSqlServer;
        }
    }
}