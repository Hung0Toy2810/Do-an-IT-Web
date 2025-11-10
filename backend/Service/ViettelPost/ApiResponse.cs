using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
namespace Backend.Service.ViettelPost
{
    public class ApiResponse<T>
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("error")]
        public bool Error { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public T? Data { get; set; }
    }
}