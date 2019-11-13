using Newtonsoft.Json;

namespace Settlement.Classes.API.Response
{
    abstract class DataResponse
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
