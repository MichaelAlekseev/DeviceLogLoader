using Serilog;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceLogLoader.Network
{
    internal class Http : IHttpClient
    {
        private HttpClient _httpClient;

        public Http() => _httpClient = new HttpClient { BaseAddress = new Uri("https://disk.iiko.pro/") };

        public Task<(HttpStatusCode?, HttpContent)> Get(string fileName, int? timeout = null) => Get(new Uri(_httpClient.BaseAddress + fileName), timeout);

        public async Task<(HttpStatusCode?, HttpContent)> Get(Uri url, int? timeout = null)
        {
            using CancellationTokenSource cts = new(timeout ?? 1_000);
            try
            {
                var response = await _httpClient.GetAsync(url, cts.Token).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.OK)
                    return (response.StatusCode, response.Content);
                Log.Warning($"[Http:Get] - Response StatusCode: '{response.StatusCode}'. Url: {url}. Content:\n{await response.Content.ReadAsStringAsync().ConfigureAwait(false)}");
                return (response.StatusCode, response.Content);
            }
            catch when (cts.IsCancellationRequested)
            {
                Log.Warning($"[Http:Get] - Timeout. Url: {url}");
                return (HttpStatusCode.RequestTimeout, null);
            }
            catch (Exception e)
            {
                Log.Error(e, $"[Http:Get] - Url: {url}.");
                return (null, null);
            }
        }
    }
}