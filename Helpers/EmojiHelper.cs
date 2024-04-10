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
            var page = NumbersHelper.GetRandom(20, 35);

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
            var page = NumbersHelper.GetRandom(120, 190);    

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

        public static async Task<string> GetRandomGyazo()
        {
            var page = NumbersHelper.GetRandom(30, 80); 

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"http://web.archive.org/cdx/search/cdx?url=i.gyazo.com/&matchType=prefix&limit=1000&output=json&fl=original&page={page}"),
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

        public static async Task<string> GetRandomPinterest()
        {
            var page = NumbersHelper.GetRandom(8000, 13000); 

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"http://web.archive.org/cdx/search/cdx?url=i.pinimg.com&matchType=prefix&limit=1000&output=json&fl=original&page={page}"),
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

        public static async Task<string> GetRandomImgur()
        {
            var page = NumbersHelper.GetRandom(10, 100000); 

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"http://web.archive.org/cdx/search/cdx?url=i.imgur.com&matchType=prefix&limit=1000&output=json&fl=original&page={page}"),
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

        public static async Task<string> GetRandomPostimg()
        {
            var page = NumbersHelper.GetRandom(2, 227); 

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"http://web.archive.org/cdx/search/cdx?url=i.postimg.cc&matchType=prefix&limit=1000&output=json&fl=original&page={page}"),
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
