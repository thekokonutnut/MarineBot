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
    internal class GelbooruImage
    {
        public string Id;
        public string File_url;
    }

    internal class GelbooruTag
    {
        public string Name;
        public string Count;
    }

    internal static class GelbooruHelper
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<GelbooruImage> GetRandomImage(string tag)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"https://gelbooru.com/index.php?page=dapi&s=post&q=index&tags={tag}"),
                Method = HttpMethod.Get,
            };

            var response = await client.SendAsync(request);
            var respstring = await response.Content.ReadAsStringAsync();

            int status = (int)response.StatusCode;

            if (status != 200)
                throw new Exception($"La API devolvió el código de respuesta: {status} {Enum.GetName(typeof(HttpStatusCode), status)}");

            XmlDocument xmlResult = new XmlDocument();
            xmlResult.LoadXml(respstring);

            JObject searchResult = JObject.Parse(JsonConvert.SerializeXmlNode(xmlResult));

            int totalItems = (int)searchResult["posts"]["@count"];
            if (totalItems <= 0)
                throw new Exception("No se encontraron imágenes con esa tag");

            IList<JToken> postsResult = searchResult["posts"]["post"].ToList();

            var random = new Random();
            var ranCount = random.Next(0, postsResult.Count);

            GelbooruImage result = new GelbooruImage()
            {
                Id = postsResult[ranCount]["@id"].ToString(),
                File_url = postsResult[ranCount]["@file_url"].ToString()
            };

            return result;
        }

        public static async Task<List<GelbooruTag>> SearchForTag(string query)
        {
            query = query.Replace(" ", "_");

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"https://gelbooru.com/index.php?page=dapi&s=tag&q=index&limit=10&order=des&orderby=count&name_pattern=%{query}%"),
                Method = HttpMethod.Get,
            };

            var response = await client.SendAsync(request);
            var respstring = await response.Content.ReadAsStringAsync();

            int status = (int)response.StatusCode;

            if (status != 200)
                throw new Exception($"La API devolvió el código de respuesta: {status} {Enum.GetName(typeof(HttpStatusCode), status)}");

            XmlDocument xmlResult = new XmlDocument();
            xmlResult.LoadXml(respstring);

            JObject searchResult = JObject.Parse(JsonConvert.SerializeXmlNode(xmlResult));

            IList<JToken> tagsResult = searchResult["tags"]["tag"].ToList();

            if (tagsResult.Count <= 0)
                throw new Exception("No se encontraron tags");

            List<GelbooruTag> result = new List<GelbooruTag>();

            for (int i = 0; i < tagsResult.Count; i++)
            {
                var currtag = new GelbooruTag()
                {
                    Name = tagsResult[i]["@name"].ToString(),
                    Count = tagsResult[i]["@count"].ToString()
                };

                result.Add(currtag);
            }

            return result;
        }
    }
}
