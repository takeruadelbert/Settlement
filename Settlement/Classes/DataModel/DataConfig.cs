using Newtonsoft.Json;

namespace Settlement.Classes.Other
{
    class DataConfig
    {
        [JsonProperty("server_epayment")]
        public static string ServerEPayment { get; set; }

        [JsonProperty("server_epass")]
        public static string ServerEPass { get; set; }

        [JsonProperty("database_name")]
        public static string DatabaseName { get; set; }

        [JsonProperty("database_username")]
        public static string DatabaseUsername { get; set; }

        [JsonProperty("database_password")]
        public static string DatabasePassword { get; set; }

        [JsonProperty("settlement_destination_path_in_linux")]
        public static string SettlementDestinationLinux { get; set; }

        [JsonProperty("settlement_destination_path_in_windows")]
        public static string SettlementDestinationWindows { get; set; }

        [JsonProperty("TID_Settlement")]
        public static string TIDSettlement { get; set; }

        [JsonProperty("MID_Settlement")]
        public static string MIDSettlement { get; set; }

        [JsonProperty("bank_name")]
        public static string BankName { get; set; }

        [JsonProperty("api_url_server_epass")]
        public static string ApiUrl { get; set; }
    }
}
