using Newtonsoft.Json;

namespace Iys.Integration.Console.Model {
    public class TokenResponseModel {
        [JsonProperty("access_token")]
        public string Token { get; set; }
    }
}
