using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DeviceLogLoader.Network
{
    internal interface IHttpClient
    {
        Task<(HttpStatusCode?, HttpContent)> Get(string fileName, int? timeout = null);

        Task<(HttpStatusCode?, HttpContent)> Get(Uri url, int? timeout = null);
    }
}