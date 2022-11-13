using Genius;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TelegramMusicBot
{
    class GeniusManager
    {
        // https://genius.com/api-clients
        const string token = "n4dfBRnUws8adqwBucNI0f-Wl0Wqyu0kUy5ktOTuS_sQkcvyPQKRJc1PRjPIUUtA";
        public static GeniusClient client { get; set; } = new GeniusClient(token);
        public static async Task<List<Genius.Models.SearchHit>> SearchHits(string lyricsText)
        {
            var search = await client.SearchClient.Search(lyricsText);

            return search.Response.Hits;
        }
    }
}
