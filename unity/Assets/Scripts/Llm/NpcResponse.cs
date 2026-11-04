
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
