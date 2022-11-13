using Genius;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TelegramMusicBot
{
    class GeniusManager
    {
        // https://genius.com/api-clients

        public static GeniusClient client { get; set; }
        public static async Task<List<Genius.Models.SearchHit>> SearchHits(string lyricsText)
        {
            var search = await client.SearchClient.Search(lyricsText);

            return search.Response.Hits;
        }
    }
}