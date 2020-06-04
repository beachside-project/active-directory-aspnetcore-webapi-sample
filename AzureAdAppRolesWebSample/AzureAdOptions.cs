using System.Text.Json.Serialization;

namespace AzureAdAppRolesWebSample
{
    public class AzureAdOptions
    {
        public string Authority { get; set; }
        public string ClientId { get; set; }

        [JsonIgnore]
        public string Audience => ClientId;
    }
}