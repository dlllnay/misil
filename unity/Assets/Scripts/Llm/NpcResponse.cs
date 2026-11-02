using System.Text.Json.Serialization;

namespace MuhurluKapi.Llm
{
    public class NpcResponse
    {
        [JsonPropertyName("dialogue")]
        public string Dialogue { get; set; } = "";

        [JsonPropertyName("action")]
        public string Action { get; set; } = "none";
    }
}