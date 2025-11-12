using System.Text.Json.Serialization;

namespace WebServerMVCv2.Models
{
    public class VideoModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [JsonPropertyName("title")]
        public string? Title { get; set; } = string.Empty;
        
        [JsonPropertyName("description")]
        public string? Description { get; set; } = string.Empty;
        
        [JsonPropertyName("video_size")]
        public long VideoSize { get; set; }

        public Dictionary<int, long> _idVidLengthDictionary = 
            Services.Cache.IdVidLengthDictionaryCache.GetorLoadCache();
    }
}
