using Newtonsoft.Json;
using RestSharp;
using Settlement.Classes.API.Interface;
using Settlement.Classes.API.Response;
using System;
using System.Net;

namespace Settlement.Classes.API
{
    class RestApi : RestApiMethod
    {
        public DataResponse post(string ip_address, string API_URL, string path_file, bool isSingleObjectReturned = true)
        {
            try
            {
                var client = new RestClient(ip_address);
                var request = new RestRequest(API_URL, Method.POST);
                request.AddFile("file", path_file);

                var response = client.Execute(request);
                DataResponse result;
                if (isSingleObjectReturned)
                {
                    result = JsonConvert.DeserializeObject<DataResponseObject>(response.Content);
                }
                else
                {
                    result = JsonConvert.DeserializeObject<DataResponseArray>(response.Content);
                }

                return result;
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
