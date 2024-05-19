using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using MarineBot.Entities;
using Newtonsoft.Json.Linq;

namespace MarineBot.Helpers
{
    internal static class MALHelper
    {
        private static readonly HttpClient client = new HttpClient();

        private static string API_ENDPOINT = "https://api.jikan.moe/v4";

        public static async Task<List<MALAnimeEntry>> GetAnimeEntriesAsync(string animeTitle, int limit = 4)
        {
            string url = $"{API_ENDPOINT}/anime?q={animeTitle}&limit={limit}";

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };

            var response = await client.SendAsync(request);
            var respstring = await response.Content.ReadAsStringAsync();

            int status = (int)response.StatusCode;

            if (status != 200)
                throw new Exception($"API returned the response code: {status} {Enum.GetName(typeof(HttpStatusCode), status)}");

            JObject searchResult = JObject.Parse(respstring);

            var searchCount = (int)searchResult["pagination"]["items"]["count"];
            if (searchCount <= 0)
                throw new Exception($"Could not find any series matching that query.");

            IList<JToken> searchItems = searchResult["data"].ToList();

            List<MALAnimeEntry> animeEntries = new List<MALAnimeEntry>();
            foreach (var item in searchItems)
            {
                var animeEntry = item.ToObject<MALAnimeEntry>();
                animeEntries.Add(animeEntry);
            }

            return animeEntries;
        }

        public static async Task<MALAnimeEntry> GetAnimeEntryById(int animeId)
        {
            string url = $"{API_ENDPOINT}/anime/{animeId}";

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };

            var response = await client.SendAsync(request);
            var respstring = await response.Content.ReadAsStringAsync();

            int status = (int)response.StatusCode;

            if (status != 200)
                throw new Exception($"API returned the response code: {status} {Enum.GetName(typeof(HttpStatusCode), status)}");

            JObject animeResult = JObject.Parse(respstring);

            var animeEntry = animeResult["data"].ToObject<MALAnimeEntry>();
            return animeEntry;
        }

        public static async Task<MALAnimeEntry> GetRandomAnimeEntry()
        {
            string url = $"{API_ENDPOINT}/random/anime";

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
            };

            var response = await client.SendAsync(request);
            var respstring = await response.Content.ReadAsStringAsync();

            int status = (int)response.StatusCode;

            if (status != 200)
                throw new Exception($"API returned the response code: {status} {Enum.GetName(typeof(HttpStatusCode), status)}");

            JObject animeResult = JObject.Parse(respstring);

            var animeEntry = animeResult["data"].ToObject<MALAnimeEntry>();
            return animeEntry;
        }
    }
}