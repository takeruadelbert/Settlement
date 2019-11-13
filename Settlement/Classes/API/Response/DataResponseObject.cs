using Newtonsoft.Json;

namespace Settlement.Classes.API.Response
{
    class DataResponseObject : DataResponse
    {
        [JsonProperty("data")]
        public JsonObjectAttribute Data { get; set; }
    }
}
