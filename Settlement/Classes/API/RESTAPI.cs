using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Settlement.Classes.API
{
    class RESTAPI
    {
        public DataResponse API_Post_UploadFile(string ip_address, string API_URL, string path_file)
        {
            try
            {
                var client = new RestClient(ip_address);
                var request = new RestRequest(API_URL, Method.POST);
                request.AddFile("file", path_file);

                var response = client.Execute(request);
                var result = JsonConvert.DeserializeObject<DataResponse>(response.Content);
                return result;
            } catch(WebException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }            
        }
    }
}
