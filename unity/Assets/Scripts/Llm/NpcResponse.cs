
using System.Text.Json.Serialization;

namespace MuhurluKapi.Llm
{
    public class NpcResponse
    {
        [JsonPropertyName("dialogue")]
        public string Dialogue { get; set; } = "";

        [JsonPropertyName("action")]
        public string Action { get; set; } = "none";

        [JsonPropertyName("trust_delta")]
        public int TrustDelta { get; set; } = 0;

        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(Dialogue))
                return false;

            if (Action != "none" && Action != "open_door")
                return false;

            return TrustDelta >= -1 && TrustDelta <= 1;
        }
    }
}
