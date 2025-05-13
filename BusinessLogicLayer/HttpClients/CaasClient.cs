using BusinessLogicLayer.DTO;
using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.Identity.Client;
using Microsoft.Extensions.Configuration;

namespace BusinessLogicLayer.HttpClients
{
    public interface ICaasClient
    {
        Task<List<CaasResponse>?> FetchKitties();
        Task<byte[]?> DownloadImageAsync(string url);
    }

    public class CaasClient : ICaasClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CaasClient> _logger;
        private readonly IConfiguration _configuration;
        public CaasClient(HttpClient httpClient, ILogger<CaasClient> logger,IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }
        public async Task<List<CaasResponse>?> FetchKitties()
        {
            // CaaS API call  
            HttpResponseMessage response = await _httpClient.GetAsync($"images/search?limit=25&has_breeds=1&api_key={_configuration.GetSection("TheCatAPI")["APIKey"]}");

            // Check if the response is successful  
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException($"Bad request while fetching data: {response.ReasonPhrase}", null, System.Net.HttpStatusCode.BadRequest);
                }
            }

            var responseString = await response.Content.ReadAsStringAsync();

            // check if responseString is null or empty  
            if (string.IsNullOrEmpty(responseString))
            {
                throw new HttpRequestException("Response string is null or empty", null, System.Net.HttpStatusCode.InternalServerError);
            }

            // Deserialize the response using JsonSerializer, remembering that we're dealing with a list of objects  
            var caasResponse = JsonSerializer.Deserialize<List<CaasResponse>>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return caasResponse;
        }

        public async Task<byte[]?> DownloadImageAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;
            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}
