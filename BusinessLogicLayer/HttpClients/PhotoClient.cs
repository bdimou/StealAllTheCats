
using BusinessLogicLayer.DTO;
using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.Identity.Client;
using Microsoft.Extensions.Configuration;



namespace BusinessLogicLayer.HttpClients
{
    public class PhotoClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PhotoClient> _logger;
        private readonly IConfiguration _configuration;
        public PhotoClient(HttpClient httpClient,ILogger<PhotoClient> logger,IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }
        public async Task<byte[]> DownloadImageAsync(string imageUrl)
        {
            try
            {
                return await _httpClient.GetByteArrayAsync(imageUrl);
            }
            catch (Exception ex)
            {
                // Optional: log, throw, or return empty array
                Console.WriteLine($"Failed to download image: {ex.Message}");
                return Array.Empty<byte>();
            }
        }
    }
}
