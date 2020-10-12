﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MarineBot.Helpers
{
    internal class LewdHelper
    {

        private static readonly HttpClient client = new HttpClient();

        public static async Task<string> E621GetRandomImage(string tag)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"https://e621.net/posts.json?limit=100&tags={tag}"),
                Method = HttpMethod.Get,
            };

            request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            request.Headers.Add("User-Agent", "MarineBot/1.69");

            var response = await client.SendAsync(request);
            var respstring = await response.Content.ReadAsStringAsync();

            int status = (int)response.StatusCode;

            if (status != 200)
                return $"La API devolvió el código de respuesta: {status} {Enum.GetName(typeof(HttpStatusCode), status)}";

            JObject searchResult = JObject.Parse(respstring);

            IList<JToken> postsResult = searchResult["posts"].ToList();

            int totalItems = postsResult.Count;
            if (totalItems <= 0)
                return $"No se encontraron imágenes con esa tag.";

            var random = new Random();
            var ranCount = random.Next(0, postsResult.Count);

            return postsResult[ranCount]["file"]["url"].ToString();
        }

    }
}
