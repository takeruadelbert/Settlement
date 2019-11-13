using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Settlement.Classes.API.Response
{
    class DataResponseArray : DataResponse
    {
        [JsonProperty("data")]
        public JArray Data { get; set; }
    }
}
