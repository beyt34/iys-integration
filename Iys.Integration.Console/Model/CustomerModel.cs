using System;

namespace Iys.Integration.Console.Model {
    public class CustomerModel {
        public int Id { get; set; }

        public string Type { get; set; }

        public string Recipient { get; set; }

        public DateTime CreatedDateTime { get; set; }
    }
}
