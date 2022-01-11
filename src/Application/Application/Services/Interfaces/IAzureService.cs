using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Microsoft.Azure.Management.Fluent;

namespace Penguin.Code.Application.Services.Interfaces
{
    public interface IAzureService
    {
        IAzure Azure { get; }

        public AccessToken AccessToken { get; }

        public ArmClient AzureArmClient { get; }
        public Subscription Subscription { get; }
    }
}