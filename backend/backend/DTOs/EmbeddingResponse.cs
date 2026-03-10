using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class EmbeddingResponse
    {
        [JsonPropertyName("embedding")]
        public List<float> Embedding { get; set; }
    }

}
