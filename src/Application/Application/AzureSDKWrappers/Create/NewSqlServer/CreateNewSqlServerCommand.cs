using MediatR;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.Sql.Fluent;

namespace Penguin.Code.Application.AzureSDKWrappers.Create.NewSqlServer
{
    public class CreateNewSqlServerCommand : IRequest<ISqlServer>
    {
        public string SqlServerName { get; set; }

        public Region AzureRegion { get; set; }

        public string ResourceGroupName { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string DatabaseName { get; set; }
    }
}