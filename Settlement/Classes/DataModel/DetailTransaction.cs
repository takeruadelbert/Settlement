using Newtonsoft.Json;

namespace Settlement.Classes.Other
{
    public class DetailTransaction
    {
        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("transaction_dt")]
        public string TransactionDatetime { get; set; }

        [JsonProperty("ipv4")]
        public string IpAddress { get; set; }

        [JsonProperty("operator")]
        public string Operator { get; set; }

        [JsonProperty("ID_reader")]
        public string IDReader { get; set; }

        public DetailTransaction(int amount, string transactionDt, string ipv4, string operatorName, string idReader)
        {
            Amount = amount;
            TransactionDatetime = transactionDt;
            IpAddress = ipv4;
            Operator = operatorName;
            IDReader = idReader;
        }
    }
}
