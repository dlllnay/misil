
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MuhurluKapi.Llm
{
    public class NpcResponse
    {
        [JsonPropertyName("dialogue")]
        public string Dialogue { get; set; }

        [JsonPropertyName("action")]
        public string Action { get; set; }

        [JsonPropertyName("trust_delta")]
        public int? TrustDelta { get; set; }

        internal static NpcResponse Parse(string text)
        {
            using var document = JsonDocument.Parse(text);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                throw new JsonException("Orrin yanıtı bir JSON nesnesi olmalı.");

            int fields = 0;
            foreach (var property in document.RootElement.EnumerateObject())
            {
                int flag = property.Name switch
                {
                    "dialogue" => 1,
                    "action" => 2,
                    "trust_delta" => 4,
                    _ => 0
                };
                if (flag == 0 || (fields & flag) != 0)
                    throw new JsonException("Orrin yanıtında beklenmeyen veya tekrarlanan alan var.");
                fields |= flag;
            }

            var response = document.RootElement.Deserialize<NpcResponse>();
            if (fields != 7 || response == null || !response.IsValid() ||
                response.Dialogue.Length > 360 ||
                response.Dialogue.Split('\n').Length > 5)
                throw new JsonException("Orrin yanıtının alanları veya diyalog uzunluğu geçersiz.");
            return response;
        }

        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(Dialogue))
                return false;

            if (Action != "none" && Action != "open_door")
                return false;

            if (!TrustDelta.HasValue)
                return false;

            return TrustDelta.Value >= -1 &&
                   TrustDelta.Value <= 1;
        }
    }
}
