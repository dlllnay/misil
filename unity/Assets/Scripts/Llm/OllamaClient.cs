
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
            string modelName = "orrin:latest",
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
                stream = false,
                format = "json"
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

        public async Task<NpcResponse> GenerateNpcResponseAsync(
         string prompt,
         CancellationToken cancellationToken = default)
        {
            string jsonPrompt = $@"
             Sen Orrin adında gizemli bir kapı bekçisisin.

             Oyuncunun mesajına yalnızca geçerli JSON ile cevap ver.
             JSON dışında hiçbir açıklama, markdown veya kod bloğu yazma.

             JSON tam olarak şu iki alanı içermeli:
             - dialogue: Oyuncuya söyleyeceğin en fazla iki kısa Türkçe cümle.
             - action: Şimdilik her zaman ""none"".

             Örnek JSON biçimi:
             {{""dialogue"":""Kapının sırları henüz açığa çıkmadı."",""action"":""none""}}

             Oyuncunun mesajı:
             {prompt}
             ";

            string responseText = await GenerateResponseAsync(
                jsonPrompt, cancellationToken);

            NpcResponse npcResponse =
                JsonSerializer.Deserialize<NpcResponse>(responseText);

            if (npcResponse == null)
            {
                throw new JsonException("NPC yanıtı boş.");
            }

            return npcResponse;
        }

    }
}
