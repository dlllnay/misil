
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

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
        private readonly HttpClient http;

        public sealed class OllamaHttpException : HttpRequestException
        {
            public int HttpStatusCode { get; }

            public OllamaHttpException(int statusCode, string detail)
                : base($"Ollama HTTP {statusCode}: {detail}")
            {
                HttpStatusCode = statusCode;
            }
        }

        public class ChatMessage
        {
            public string role { get; set; } = "";
            public string content { get; set; } = "";
        }

        public OllamaClient(
            string modelName = "orrin:latest",
            string apiUrl = "http://127.0.0.1:11434/api/chat",
            HttpClient httpClient = null)
        {
            this.modelName = modelName;
            this.apiUrl = apiUrl;
            http = httpClient ?? Http;
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
                // Per-request settings also apply when the installed model was
                // created from an older Modelfile. No model rebuild is needed.
                options = new { temperature = 0.2, num_predict = 192 },
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
                        },
                        trust_delta = new
                        {
                            type = "integer",
                            minimum = -1,
                            maximum = 1
                        }
                    },
                    required = new[]
                    {
                        "dialogue",
                        "action",
                        "trust_delta"
                    },
                    additionalProperties = false
                }
            };

            string json = JsonSerializer.Serialize(requestData);

            using var content = new StringContent(
                json, Encoding.UTF8, "application/json");

            using var response = await http.PostAsync(
                apiUrl, content, cancellationToken);

            string result = await response.Content.ReadAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();

            if (!response.IsSuccessStatusCode)
                throw new OllamaHttpException((int)response.StatusCode, result);

            using var document = JsonDocument.Parse(result);

            if (document.RootElement.ValueKind != JsonValueKind.Object ||
                !document.RootElement.TryGetProperty("message", out var message) ||
                message.ValueKind != JsonValueKind.Object ||
                !message.TryGetProperty("content", out var reply) ||
                reply.ValueKind != JsonValueKind.String ||
                string.IsNullOrWhiteSpace(reply.GetString()))
                throw new JsonException("Ollama yanıtında geçerli message.content bulunamadı.");

            return reply.GetString();
        }

        public async Task<string> GenerateResponseAsync(
            string prompt,
            CancellationToken cancellationToken = default)
        {
            return await GenerateChatResponseAsync(
                new List<ChatMessage>
                {
                    new ChatMessage { role = "user", content = prompt }
                }, cancellationToken);
        }

        public async Task<NpcResponse> GenerateNpcChatResponseAsync(
            List<ChatMessage> messages,
            CancellationToken cancellationToken = default)
        {
            var requestMessages = new List<ChatMessage>(messages);
            for (int attempt = 0; attempt < 2; attempt++)
            {
                string responseText = await GenerateChatResponseAsync(
                    requestMessages, cancellationToken);
                NpcResponse npcResponse = JsonSerializer.Deserialize<NpcResponse>(responseText);
                if (npcResponse == null || !npcResponse.IsValid())
                    throw new JsonException("NPC yanıtı geçersiz: dialogue, action veya trust_delta hatalı.");

                if (!UsesAssistantVoice(npcResponse.Dialogue))
                    return npcResponse;

                // Reject this entire draft before any trust or door change.
                // The replacement must also come from the real local model.
                requestMessages.Insert(Math.Max(0, requestMessages.Count - 1), new ChatMessage
                {
                    role = "system",
                    content = "Correction: your previous draft used customer-service language. " +
                        "Answer the traveller's original message again as Orrin, the stern ancient gatekeeper. " +
                        "Use one short Turkish sentence about the gate, the seal, or the traveller's intent. " +
                        "Do not offer assistance or introduce yourself. Keep the same JSON schema and game-state rules."
                });
            }

            throw new JsonException("Orrin yanıtı karakter denetimini geçemedi; asistan üslubu reddedildi.");
        }

        private static bool UsesAssistantVoice(string dialogue)
        {
            string text = dialogue.ToLowerInvariant().Replace('ı', 'i')
                .Replace('ş', 's').Replace('ğ', 'g').Replace('ü', 'u')
                .Replace('ö', 'o').Replace('ç', 'c');
            return text.Contains("yardimci ol") || text.Contains("yardim edebil") ||
                text.Contains("sorularinizi yanit") || text.Contains("merhaba, ben orrin") ||
                text.Contains("yapay zeka") || text.Contains("asistan") ||
                text.Contains("how can i") || text.Contains("help you");
        }

        public async Task<NpcResponse> GenerateNpcResponseAsync(
            string prompt,
            CancellationToken cancellationToken = default)
        {
            string jsonPrompt = $@"
Sen Orrin adında gizemli bir kapı bekçisisin.

Oyuncunun mesajına yalnızca geçerli JSON ile cevap ver.
JSON dışında hiçbir açıklama, markdown veya kod bloğu yazma.

JSON tam olarak şu üç alanı içermeli:

- dialogue: Oyuncuya doğrudan hitap eden en fazla iki kısa Türkçe cümle.
- dialogue alanında yalnızca Orrin'in kendi sözleri yer alsın.
- Üçüncü şahıs anlatımı kullanma.
- Oyuncunun hareketlerini veya sözlerini uydurma.
- action: Yalnızca ""none"" veya ""open_door"" olabilir.
- Oyuncu kapının açılmasını açıkça isterse ""open_door"" seç.
- Diğer bütün mesajlarda ""none"" seç.
- trust_delta: Yalnızca -1, 0 veya 1 olabilir.
- Oyuncu saygılı, samimi ve ikna ediciyse trust_delta 1 seç.
- Oyuncu tehditkâr veya hakaret ediciyse trust_delta -1 seç.
- Diğer durumlarda trust_delta 0 seç.
- ""open_door"" seçersen kapıyı açmayı kabul ettiğini söyle.
- ""none"" seçersen Orrin karakterine uygun kısa bir cevap ver.

Örnek JSON:
{{""dialogue"":""Sözlerinde samimiyet görüyorum, yolcu."",""action"":""none"",""trust_delta"":1}}

Oyuncunun mesajı:
{prompt}
";

            string responseText = await GenerateResponseAsync(
                jsonPrompt, cancellationToken);

            Debug.Log("Ollama ham JSON: " + responseText);

            NpcResponse npcResponse =
                JsonSerializer.Deserialize<NpcResponse>(responseText);

            if (npcResponse == null || !npcResponse.IsValid())
            {
                throw new JsonException(
                    "NPC yanıtı geçersiz: dialogue, action veya trust_delta hatalı.");
            }

            return npcResponse;
        }
    }
}
