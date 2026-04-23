using System.Collections.Generic;
using System.Text.Json.Serialization;
namespace translation.Models
{
    public class ChatRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }
        [JsonPropertyName("messages")]
        public List<Message> Messages { get; set; }
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }
    }
    public class Message
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
    public class ChatResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice> Choices { get; set; }
    }
    public class Choice
    {
        [JsonPropertyName("message")]
        public Message Message { get; set; }
    }
}
