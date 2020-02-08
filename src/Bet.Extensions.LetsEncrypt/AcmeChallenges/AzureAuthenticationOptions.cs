namespace Bet.Extensions.LetsEncrypt.AcmeChallenges
{
    public class AzureAuthenticationOptions
    {
        public string AzureCloudInstance { get; set; } = "AzurePublic";

        public string TenantId { get; set; } = string.Empty;

        public string SubscriptionId { get; set; } = string.Empty;
    }
}
