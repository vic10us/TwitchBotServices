using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TwitchBot.Service.Models;

namespace TwitchBot.Service.Services
{
    public class WLEDService
    {
        private readonly HttpClient _httpClient;

        public WLEDService(
            IOptions<WLEDConfig> config, 
            IHttpClientFactory clientFactory)
        {
            _httpClient = clientFactory.CreateClient("WLEDService");
        }

        public async Task<WLEDRootObject> GetRootConfig()
        {
            var streamTask = _httpClient.GetStreamAsync("/json");
            var resp = await System.Text.Json.JsonSerializer.DeserializeAsync<WLEDRootObject>(await streamTask);
            return resp;
        }

        public async Task SetFx(int index)
        {
            await _httpClient.GetAsync($"/win&FX={index}");
        }
    }
}
