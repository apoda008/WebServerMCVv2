using System.Text.Json;
using WebServerMVCv2.Services.TCP;

namespace WebServerMVCv2.Services.Cache
{

    public sealed class TitleItem
    {
        public string? Title { get; set; } // case-insensitive with the options below
    }

    public sealed class IdItem
    {
        public int Id { get; set; } // case-insensitive with the options below
    }   

    public class IdTitleDictionaryCache
    {
        private static readonly object CacheLockObject = new object();
        private static Dictionary<int, string>? _idTitleDictionaryCache = null;

        public static Dictionary<int, string> GetorLoadCache() //Func<IDictionary<int, string>> loadFunction
        {
            if (_idTitleDictionaryCache != null) return _idTitleDictionaryCache;

            lock (CacheLockObject)
            {
                if (_idTitleDictionaryCache == null)
                {
                    _idTitleDictionaryCache = LoadFromDatabase();
                }
                return _idTitleDictionaryCache;
            }

        }
        private static Dictionary<int, string> LoadFromDatabase() 
        {
            //logic to load data from database
            
            Dictionary<int, string> idTitleDictionary = new Dictionary<int, string>();

            RemoteTcpClient tcpTitles = new RemoteTcpClient("SELECT%TITLE%FROM%MOVIES");
            RemoteTcpClient tcpIds = new RemoteTcpClient("SELECT%ID%FROM%MOVIES"); 

            tcpTitles.ConnectAsync().Wait();

            string? titlesJson = tcpTitles.rawStringResponse;

            if(string.IsNullOrEmpty(titlesJson))
            {
                return idTitleDictionary; //return empty dictionary if no data
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            List<TitleItem> titles = JsonSerializer.Deserialize<List<TitleItem>>(titlesJson, options);
            if (titles == null)
            {
                return idTitleDictionary; //return empty dictionary if deserialization fails
            }

            //Now Ids
            tcpIds.ConnectAsync().Wait();

            string? idsJson = tcpIds.rawStringResponse;

            if (string.IsNullOrEmpty(idsJson))
            {
                return idTitleDictionary; //return empty dictionary if no data
            }

            List<IdItem> ids = JsonSerializer.Deserialize<List<IdItem>>(idsJson, options);
            
            if (ids == null)
            {
                return idTitleDictionary; //return empty dictionary if deserialization fails
            }
            //build dictionary
            for (int i = 0; i < titles.Count && i < ids.Count; i++)
            {
                if (titles[i].Title != null)
                {
                    idTitleDictionary[ids[i].Id] = titles[i].Title;
                }
            }

            return idTitleDictionary;
        }

        public static void InvalidateCache()
        {
            lock (CacheLockObject)
            {
                _idTitleDictionaryCache = null;
            }
        }
    }
}

