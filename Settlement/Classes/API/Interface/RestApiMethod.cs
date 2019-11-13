using Settlement.Classes.API.Response;

namespace Settlement.Classes.API.Interface
{
    interface RestApiMethod
    {
        DataResponse post(string ipAddressServer, string url, string path_file, bool isSingleObjectReturned);
    }
}
