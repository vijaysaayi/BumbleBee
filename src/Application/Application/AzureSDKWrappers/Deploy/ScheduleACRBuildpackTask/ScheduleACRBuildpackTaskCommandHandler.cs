using Penguin.Code.Application.Services.Interfaces;
using MediatR;
using Microsoft.Azure.Management.ContainerRegistry.Fluent;
using Microsoft.Azure.Management.Fluent;
using Spectre.Console;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Penguin.Code.Application.AzureSDKWrappers.Deploy.ScheduleACRBuildpackTask
{
    public class ScheduleACRBuildpackTaskCommandHandler : IRequestHandler<ScheduleACRBuildpackTaskCommand, IRegistryTaskRun>
    {
        private readonly IAzure _azure;
        private readonly IHttpClientService _httpClient;

        public ScheduleACRBuildpackTaskCommandHandler(IAzureService azureService, IHttpClientService httpClient)
        {
            _azure = azureService.Azure;
            _httpClient = httpClient;
        }

        public async Task<IRegistryTaskRun> Handle(ScheduleACRBuildpackTaskCommand request, CancellationToken cancellationToken)
        {
            string content = await GetContent(request.RegistryUrl, request.ImageName, request.BuilderName);
            if (content == null)
                throw new System.Exception("Unable to file buildpacktask.yml file");
            string encodedContent = Base64Encode(content);

            var newBuildTask = _azure.RegistryTaskRuns.ScheduleRun()
                                    .WithExistingRegistry(request.ResourceGroupName, request.RegistryName)
                                        .WithLinux()
                                        .WithEncodedTaskRunRequest()
                                            .DefineEncodedTaskStep()
                                                .WithBase64EncodedTaskContent(encodedContent)
                                                .Attach()
                                            .WithSourceLocation(request.SourceLocation)
                                        .WithArchiveEnabled(true);

            IRegistryTaskRun run = await newBuildTask.ExecuteAsync();
            if (run != null)
            {
                AnsiConsole.MarkupLine($"Registry Name : {run.RegistryName}");
                AnsiConsole.MarkupLine($"Task Name : {run.TaskName}");
                AnsiConsole.MarkupLine($"Status : {run.Status}");
                AnsiConsole.MarkupLine($"CPU : {run.Cpu}");
                AnsiConsole.MarkupLine($"Provisioning state : {run.ProvisioningState}");
                AnsiConsole.MarkupLine($"Last updated time : {run.LastUpdatedTime}");
                AnsiConsole.MarkupLine($"Run Id : {run.RunId}");
            }
            AnsiConsole.WriteLine(run.Status.Value);
            await run.RefreshAsync();
            AnsiConsole.WriteLine(run.Status.Value);
            return run;
        }

        public async Task<string> GetContent(string repositoryUrl, string imageName, string builderName)
        {
            var content = await GetBuildpackTaskYml();
            if (content != null)
            {
                content = content.Replace("{registry_url}", repositoryUrl).Replace("{image_name}", imageName).Replace("{builder_name}", builderName);
            }
            return content;
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public async Task<string> GetBuildpackTaskYml()
        {
            string url = "https://gist.githubusercontent.com/vijaysaayi/1359cff3aa6bbe5db787cc5403aa094a/raw/72197a0264f357a1d528a41f950d3192ca214c6e/buildpacktask.yml";
            string localPath = Path.Combine(Path.GetTempPath(), "buildpacktask.yml");

            if (File.Exists(localPath))
            {
                using var reader = File.OpenText(localPath);
                var content = await reader.ReadToEndAsync();
                return content;
            }

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                using (StreamWriter c = new StreamWriter(localPath, false))
                {
                    await c.WriteAsync(content);
                }

                return content;
            }

            throw new System.Exception("Unable to fetch buildpacktask.yml file");
        }
    }
}