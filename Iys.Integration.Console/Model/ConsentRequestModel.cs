using System;

using Newtonsoft.Json;

namespace Iys.Integration.Console.Model {
    public class ConsentRequestModel {
        public ConsentRequestModel() {
        }

        public ConsentRequestModel(string type, string recipient, DateTime consentDate) {
            Source = "HS_WEB";
            RecipientType = "BIREYSEL";
            Status = "ONAY";
            Type = type;
            Recipient = type == "EPOSTA" ? recipient : "+90" + recipient;

            consentDate = consentDate.AddHours(3);
            if (consentDate < new DateTime(2015, 5, 1)) {
                consentDate = new DateTime(2015, 5, 1);
            }

            ConsentDate = consentDate.ToString("yyyy-MM-dd HH:mm:ss");
        }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("recipientType")]
        public string RecipientType { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("consentDate")]
        public string ConsentDate { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("recipient")]
        public string Recipient { get; set; }
    }
}
