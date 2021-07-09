using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MarineBot.Helpers
{
    internal static class EmojiHelper
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<string> GetRandomOnlineEmoji()
        {
            var page = NumbersHelper.GetRandom(0, 27); //why 27? i don't knowxd         

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"http://web.archive.org/cdx/search/cdx?url=cdn.discordapp.com/emojis/&matchType=prefix&limit=1000&output=json&fl=original&page={page}"),
                Method = HttpMethod.Get,
            };

            var response = await client.SendAsync(request);
            var respstring = await response.Content.ReadAsStringAsync();

            int status = (int)response.StatusCode;

            if (status != 200)
                throw new Exception($"API returned the response code: {status} {Enum.GetName(typeof(HttpStatusCode), status)}");

            JArray searchResult = JArray.Parse(respstring);

            var ranCount = NumbersHelper.GetRandom(1, searchResult.Count - 1);

            return (string)searchResult[ranCount][0];
        }

        public static async Task<string> GetRandomOnlineImage()
        {
            var page = NumbersHelper.GetRandom(0, 32); //why 32? i don't knowxd         

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"http://web.archive.org/cdx/search/cdx?url=cdn.discordapp.com/attachments/&matchType=prefix&limit=1000&output=json&fl=original&page={page}"),
                Method = HttpMethod.Get,
            };

            var response = await client.SendAsync(request);
            var respstring = await response.Content.ReadAsStringAsync();

            int status = (int)response.StatusCode;

            if (status != 200)
                throw new Exception($"API returned the response code: {status} {Enum.GetName(typeof(HttpStatusCode), status)}");

            JArray searchResult = JArray.Parse(respstring);

            var ranCount = NumbersHelper.GetRandom(1, searchResult.Count - 1);

            return (string)searchResult[ranCount][0];
        }
    }
}
