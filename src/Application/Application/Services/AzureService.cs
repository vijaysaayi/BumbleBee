using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Penguin.Code.Application.ExtensionMethods;
using Penguin.Code.Application.Services.Interfaces;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading;

namespace Penguin.Code.Application.Services
{
    public class AzureService : IAzureService
    {
        private readonly ILogger<AzureService> _logger;

        public ArmClient AzureArmClient { get; private set; }
        public Subscription Subscription { get; private set; }

        public IAzure Azure { get; private set; }

        public AccessToken AccessToken { get; private set; }

        public AzureService(ILogger<AzureService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));            
            if (!EstablishConnection())
            {
                throw new Exception("Error connecting to Azure");
            }

            Console.OutputEncoding = Encoding.UTF8;
        }

        public bool EstablishConnection()
        {
            try
            {
                Console.WriteLine();
                var defaultCredential = new DefaultAzureCredential(includeInteractiveCredentials: true);
                AnsiConsoleExtensionMethods.Display("Sign in was successful. Attempting to get an access token to perform management operations");
                AccessToken = defaultCredential.GetToken(new TokenRequestContext(
                    new[] { "https://management.azure.com/.default" })
                );
                AnsiConsoleExtensionMethods.Display("Successfully obtained a token");
                var defaultTokenCredentials = new Microsoft.Rest.TokenCredentials(AccessToken.Token);
                var azureCredentials = new Microsoft.Azure.Management.ResourceManager.Fluent.Authentication.AzureCredentials(defaultTokenCredentials, defaultTokenCredentials, null, AzureEnvironment.AzureGlobalCloud);
                Azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(azureCredentials).WithDefaultSubscription();

                AzureArmClient = new ArmClient(new DefaultAzureCredential(includeInteractiveCredentials: true));
                Subscription = AzureArmClient.GetDefaultSubscription();

                var subscriptions = AzureArmClient.GetSubscriptions();

                AnsiConsoleExtensionMethods.Display("Successfully established connectivity with Azure");
                
                AnsiConsoleExtensionMethods.Display($"Resources would be deployed in default subscription {Subscription.Data.SubscriptionGuid} ({Azure.SubscriptionId})");
                AnsiConsoleExtensionMethods.Display("", disableCheckBox: true);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }
    }
}