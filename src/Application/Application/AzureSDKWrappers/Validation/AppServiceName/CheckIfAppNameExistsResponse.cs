namespace BumbleBee.Application.AzureSDKWrappers.Validation
{
    public class CheckIfAppNameExistsResponse
    {
        public bool NameAvailable { get; set; }

        public string Reason { get; set; }

        public string Message { get; set; }
    }
}