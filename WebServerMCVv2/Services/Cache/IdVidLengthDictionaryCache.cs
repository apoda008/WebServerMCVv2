using System.Text.Json;
using System.Text.Json.Serialization;
using WebServerMVCv2.Services.TCP;

namespace WebServerMVCv2.Services.Cache
{
    
    public sealed class Video_SizeItem
    {
        [JsonPropertyName("video_size")]
        public long VidLen { get; set; }// case-insensitive with the options below
    }

    public sealed class IdVidItem
    {
        public int? Id { get; set; } // case-insensitive with the options below
    }
    public class IdVidLengthDictionaryCache
    {
        private static readonly object CacheLockObject = new object();
        private static Dictionary<int, long>? _idVideoDictionaryCache = null;

        public static Dictionary<int, long> GetorLoadCache() //Func<IDictionary<int, string>> loadFunction
        {
            if (_idVideoDictionaryCache != null) return _idVideoDictionaryCache;

            lock (CacheLockObject)
            {
                if (_idVideoDictionaryCache == null)
                {
                    _idVideoDictionaryCache = LoadFromDatabaseVid();
                }
                return _idVideoDictionaryCache;
            }

        }
        private static Dictionary<int, long> LoadFromDatabaseVid()
        {
            //logic to load data from database

            Dictionary<int, long> idVidDictionary = new Dictionary<int, long>();

            RemoteTcpClient tcpVidLength = new RemoteTcpClient("SELECT%VIDLEN%FROM%MOVIES");
            RemoteTcpClient tcpIds = new RemoteTcpClient("SELECT%ID%FROM%MOVIES");

            //vid lengths
            tcpVidLength.ConnectAsync().Wait();

            string? VidLenJson = tcpVidLength.rawStringResponse;

            if (string.IsNullOrEmpty(VidLenJson))
            {
                return idVidDictionary; //return empty dictionary if no data
            }

            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };

            List<Video_SizeItem> VidLens = JsonSerializer.Deserialize<List<Video_SizeItem>>(VidLenJson, options);
            
            if (VidLens == null)
            {
                return idVidDictionary; //return empty dictionary if deserialization fails
            }

            //Now Ids
            tcpIds.ConnectAsync().Wait();

            string? idsJson = tcpIds.rawStringResponse;

            if (string.IsNullOrEmpty(idsJson))
            {
                return idVidDictionary; //return empty dictionary if no data
            }

            List<IdItem> ids = JsonSerializer.Deserialize<List<IdItem>>(idsJson, options);
            if (ids == null)
            {
                return idVidDictionary; //return empty dictionary if deserialization fails
            }
            //build dictionary
            for (int i = 0; i < VidLens.Count && i < ids.Count; i++)
            {
                
                if (VidLens[i].VidLen >= 0)
                {
                    idVidDictionary[ids[i].Id] = VidLens[i].VidLen;
                }
            }

            return idVidDictionary;
        }

        public static void InvalidateCache()
        {
            lock (CacheLockObject)
            {
                _idVideoDictionaryCache = null;
            }
        }
    }
}
