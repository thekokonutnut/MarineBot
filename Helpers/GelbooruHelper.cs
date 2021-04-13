using Newtonsoft.Json;
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

        public static async Task<int> GetPostCount(string tags)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"https://gelbooru.com/index.php?page=dapi&s=post&q=index&limit=0&tags={tags}"),
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
            return totalItems;
        }

        public static async Task<GelbooruImage> GetRandomImage(string tags)
        {
            int postCount = await GetPostCount(tags);
            int pageNum = 0;

            if (postCount <= 0)
                throw new Exception("No se encontraron imágenes con esa tag");

            var random = new Random();

            if (postCount > 100)
            {
                int maxPage = (int)Math.Ceiling((double)(postCount / 100));
                if (maxPage > 200) maxPage = 200;

                pageNum = random.Next(0, maxPage);
            }

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"https://gelbooru.com/index.php?page=dapi&s=post&q=index&json=1&pid={pageNum}&tags={tags}"),
                Method = HttpMethod.Get,
            };

            var response = await client.SendAsync(request);
            var respstring = await response.Content.ReadAsStringAsync();

            int status = (int)response.StatusCode;

            if (status != 200)
                throw new Exception($"La API devolvió el código de respuesta: {status} {Enum.GetName(typeof(HttpStatusCode), status)}");

            JArray searchResult = JArray.Parse(respstring);

            var ranCount = random.Next(0, searchResult.Count);

            GelbooruImage result = new GelbooruImage()
            {
                Id = searchResult[ranCount]["id"].ToString(),
                File_url = searchResult[ranCount]["file_url"].ToString()
            };

            return result;
        }

        public static async Task<List<GelbooruTag>> SearchForTag(string query)
        {
            query = query.Replace(" ", "_");

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"https://gelbooru.com/index.php?page=dapi&s=tag&q=index&limit=10&order=des&orderby=count&json=1&name_pattern=%{query}%"),
                Method = HttpMethod.Get,
            };

            var response = await client.SendAsync(request);
            var respstring = await response.Content.ReadAsStringAsync();

            int status = (int)response.StatusCode;

            if (status != 200)
                throw new Exception($"La API devolvió el código de respuesta: {status} {Enum.GetName(typeof(HttpStatusCode), status)}");

            JArray searchResult = JArray.Parse(respstring);

            var tagCount = searchResult.Count;

            if (tagCount <= 0)
                throw new Exception("No se encontraron tags.");

            var result = new List<GelbooruTag>();

            for (int i = 0; i < tagCount; i++)
            {
                var currtag = new GelbooruTag()
                {
                    Name = searchResult[i]["tag"].ToString(),
                    Count = searchResult[i]["count"].ToString()
                };

                result.Add(currtag);
            }

            return result;
        }
    }
}
