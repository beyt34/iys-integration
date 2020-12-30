using Newtonsoft.Json;

namespace Iys.Integration.Console.Model {
    public class ConsentResponseModel {
        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty("creationDate")]
        public string CreationDate { get; set; }
    }
}
