using System.Net.Http.Json;
using System.Text.Json;

using NihongoBot.Application.Models;

namespace NihongoBot.Application.Services
{
    public class JlptVocabApiService
    {
        private readonly HttpClient _httpClient;

        public JlptVocabApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<JLPTWord?> GetRandomWordAsync(int level = 1)
        {
            JLPTWord? response = await _httpClient.GetFromJsonAsync<JLPTWord>($"random?level={level}");
			return response;
        }
    }
}
