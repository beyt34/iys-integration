using Newtonsoft.Json;

namespace Iys.Integration.Console.Model {
    public class TokenRequestModel {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("grant_type")]
        public string Type { get; set; }
    }
}
