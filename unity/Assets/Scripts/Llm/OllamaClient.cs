
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MuhurluKapi.Llm
{
    public class OllamaClient : ILlmClient
    {
        private static readonly HttpClient Http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(120)
        };

        private readonly string modelName;
        private readonly string apiUrl;

        public OllamaClient(
            string modelName = "tinyllama:latest",
            string apiUrl = "http://127.0.0.1:11434/api/generate")
        {
            this.modelName = modelName;
            this.apiUrl = apiUrl;
        }

        public async Task<string> GenerateResponseAsync(
            string prompt,
            CancellationToken cancellationToken = default)
        {
            var requestData = new
            {
                model = modelName,
                prompt = prompt,
                stream = false
            };

            string json = JsonSerializer.Serialize(requestData);

            using var content = new StringContent(
                json, Encoding.UTF8, "application/json");

            using var response = await Http.PostAsync(
                apiUrl, content, cancellationToken);

            response.EnsureSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(result);

            return document.RootElement
                .GetProperty("response")
                .GetString() ?? "";
        }
    }
}
  