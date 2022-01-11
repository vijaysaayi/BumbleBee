using MediatR;
using Microsoft.Azure.Management.ContainerRegistry.Fluent;

namespace Penguin.Code.Application.AzureSDKWrappers.Deploy.ScheduleACRBuildpackTask
{
    public class ScheduleACRBuildpackTaskCommand : IRequest<IRegistryTaskRun>
    {
        public string RegistryName { get; set; }

        public string ResourceGroupName { get; set; }

        public string ImageName { get; set; }

        public string BuilderName { get; set; }

        public string SourceLocation { get; set; }

        public string RegistryUrl { get; set; }
    }
}