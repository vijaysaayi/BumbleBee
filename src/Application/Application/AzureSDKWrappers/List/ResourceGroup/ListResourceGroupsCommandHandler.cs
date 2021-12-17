using BumbleBee.Code.Application.Services.Interfaces;
using MediatR;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BumbleBee.Code.Application.AzureSDKWrappers.List.ResourceGroup
{
    public class ListResourceGroupsCommandHandler : IRequestHandler<ListResourceGroupsCommand, List<string>>
    {
        private readonly IAzure _azure;

        public ListResourceGroupsCommandHandler(IAzureService azureService)
        {
            _azure = azureService.Azure;
        }

        public async Task<List<string>> Handle(ListResourceGroupsCommand request, CancellationToken cancellationToken)
        {
            var rsg = new List<string>();

            IPagedCollection<IResourceGroup> resourceGroups = await _azure.ResourceGroups.ListAsync(loadAllPages: true, cancellationToken);
            foreach (var resourceGroup in resourceGroups)
            {
                rsg.Add(resourceGroup.Name);
            }

            return rsg;
        }
    }
}