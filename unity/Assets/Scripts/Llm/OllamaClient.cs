
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;


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

        public class ChatMessage
        {
            public string role { get; set; }
            public string content { get; set; }
        }

        public OllamaClient(
            string modelName = "orrin:latest",
            string apiUrl = "http://127.0.0.1:11434/api/chat")
        {
            this.modelName = modelName;
            this.apiUrl = apiUrl;
        }

        public async Task<string> GenerateChatResponseAsync(
            List<ChatMessage> messages,
            CancellationToken cancellationToken = default)
        {
            var requestData = new
            {
                model = modelName,
                messages = messages,
                stream = false,
                format = new
                {
                    type = "object",
                    properties = new
                    {
                        dialogue = new { type = "string" },
                        action = new
                        {
                            type = "string",
                            @enum = new[] { "none", "open_door" }
                        }
                    },
                    required = new[] { "dialogue", "action" },
                    additionalProperties = false
                }
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
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "";
        }

        public async Task<string> GenerateResponseAsync(
            string prompt,
            CancellationToken cancellationToken = default)
        {

            var requestData = new
            {
                model = modelName,

                messages = new[]
                {
                    new
                    {
                      role = "user",
                      content = prompt
                    }
                },

                stream = false,
                format = new
                {
                    type = "object",
                    properties = new
                    {
                        dialogue = new
                        {
                            type = "string"
                        },
                        action = new
                        {
                            type = "string",
                            @enum = new[] { "none", "open_door" }
                        }
                    },
                    required = new[] { "dialogue", "action" },
                    additionalProperties = false
                }
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
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "";

        }

        public async Task<NpcResponse> GenerateNpcChatResponseAsync(
            List<ChatMessage> messages,
            CancellationToken cancellationToken = default)
        {
            string responseText = await GenerateChatResponseAsync(
                messages, cancellationToken);

            NpcResponse npcResponse =
                JsonSerializer.Deserialize<NpcResponse>(responseText);

            if (npcResponse == null || !npcResponse.IsValid())
            {
                throw new JsonException(
                    "NPC yanıtı geçersiz: dialogue veya action hatalı.");
            }

            return npcResponse;
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
             - dialogue: Oyuncuya doğrudan hitap eden en fazla iki kısa Türkçe cümle.
             - dialogue alanında yalnızca Orrin'in kendi sözleri yer alsın; olay anlatma.
             - Üçüncü şahıs anlatımı kullanma, oyuncunun hareketlerini veya sözlerini uydurma.
             - action: Yalnızca ""none"" veya ""open_door"" olabilir.
             - Oyuncu kapının açılmasını açıkça isterse ""open_door"" seç.
             - Diğer bütün mesajlarda ""none"" seç.
             - ""open_door"" seçersen kapıyı açmayı kabul ettiğini açıkça söyle.
             - ""open_door"" seçtiğinde soru sorma, oyuncudan tekrar izin isteme.
             - dialogue alanında yalnızca Orrin'in oyuncuya söylediği sözler bulunsun.
             - ""none"" seçersen Orrin karakterine uygun kısa bir cevap ver.

             Örnek JSON biçimi:
             {{""dialogue"":""Kapının sırları henüz açığa çıkmadı."",""action"":""none""}}

             Oyuncunun mesajı:
             {prompt}
             ";

            string responseText = await GenerateResponseAsync(
                jsonPrompt, cancellationToken);

            NpcResponse npcResponse =
                JsonSerializer.Deserialize<NpcResponse>(responseText);

            if (npcResponse == null || !npcResponse.IsValid())
            {
                throw new JsonException(
                    "NPC yanıtı geçersiz: dialogue veya action hatalı.");
            }

            return npcResponse;
        }

    }
}
