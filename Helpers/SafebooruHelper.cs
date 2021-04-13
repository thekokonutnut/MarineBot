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
    internal class SafebooruImage
    {
        public string Id;
        public string File_url;
    }

    internal class SafebooruTag
    {
        public string Name;
        public string Count;
    }

    internal static class SafebooruHelper
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<int> GetPostCount(string tags)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"https://safebooru.org/index.php?page=dapi&s=post&q=index&limit=0&tags={tags}"),
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

        public static async Task<SafebooruImage> GetRandomImage(string tags)
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
                RequestUri = new Uri($"https://safebooru.org/index.php?page=dapi&s=post&q=index&json=1&pid={pageNum}&tags={tags}"),
                Method = HttpMethod.Get,
            };

            var response = await client.SendAsync(request);
            var respstring = await response.Content.ReadAsStringAsync();

            int status = (int)response.StatusCode;

            if (status != 200)
                throw new Exception($"La API devolvió el código de respuesta: {status} {Enum.GetName(typeof(HttpStatusCode), status)}");

            JArray searchResult = JArray.Parse(respstring);

            var ranCount = random.Next(0, searchResult.Count);

            SafebooruImage result = new SafebooruImage()
            {
                Id = searchResult[ranCount]["id"].ToString(),
                File_url = $"https://safebooru.org/images/{searchResult[ranCount]["directory"]}/{searchResult[ranCount]["image"]}"
            };

            return result;
        }

        public static async Task<List<SafebooruTag>> SearchForTag(string query)
        {
            query = query.Replace(" ", "_");

            var request = new HttpRequestMessage()
            {
                // TODO: find sort parameter
                RequestUri = new Uri($"https://safebooru.org/index.php?page=dapi&s=tag&q=index&order=count&name_pattern=%{query}%"),
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

            if (searchResult["tags"] == null)
                throw new Exception("No se encontraron tags.");

            var result = new List<SafebooruTag>();

            if (searchResult["tags"]["tag"] is JArray)
            {
                IList<JToken> tagsResult = searchResult["tags"]["tag"].ToList();

                // inverse order
                for (int i = tagsResult.Count - 1; i >= 0; i--)
                {
                    var currtag = new SafebooruTag()
                    {
                        Name = tagsResult[i]["@name"].ToString(),
                        Count = tagsResult[i]["@count"].ToString()
                    };

                    result.Add(currtag);

                    if (result.Count == 10) break;
                }
            }
            else
            {
                var currtag = new SafebooruTag()
                {
                    Name = searchResult["tags"]["tag"]["@name"].ToString(),
                    Count = searchResult["tags"]["tag"]["@count"].ToString()
                };

                result.Add(currtag);
            }

            return result;
        }
    }
}
