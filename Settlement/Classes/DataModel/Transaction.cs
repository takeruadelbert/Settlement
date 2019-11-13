using Newtonsoft.Json;
using Settlement.Classes.Other;
using System.Collections.Generic;

namespace Settlement.Classes.DataModel
{
    public class Transaction
    {
        [JsonProperty("created_settlement_dt")]
        public string CreatedDatetimeSettlement { get; set; }

        [JsonProperty("bank")]
        public string Bank { get; set; }

        [JsonProperty("details")]
        public List<DetailTransaction> DetailTransaction;

        public Transaction(string createdDatetimeSettlement, string bank, List<DetailTransaction> details)
        {
            CreatedDatetimeSettlement = createdDatetimeSettlement;
            Bank = bank;
            DetailTransaction = details;
        }
    }
}
