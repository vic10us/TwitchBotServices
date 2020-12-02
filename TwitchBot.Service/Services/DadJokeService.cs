using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TwitchBot.Service.Features.DadJokes;
using TwitchBot.Service.Models;

namespace TwitchBot.Service.Services
{
    public class DadJokeService
    {
        private readonly HttpClient _httpClient;

        public DadJokeService(IHttpClientFactory clientFactory)
        {
            _httpClient = clientFactory.CreateClient("DadJokeService");
        }

        public async Task<DadJoke> GetDadJoke()
        {
            var r = new HttpRequestMessage(HttpMethod.Get, "/");
            r.Headers.Add("Accept", "application/json");
            var o = await _httpClient.SendAsync(r);
            o.EnsureSuccessStatusCode();
            var json = await o.Content.ReadAsStringAsync();
            var resp = JsonConvert.DeserializeObject<DadJoke>(json);
            // var o = await _httpClient.GetFromJsonAsync<DadJoke>("/");
            if (resp == null) throw new Exception("Unable to get dad joke :(");
            return resp;
        }
    }
}
