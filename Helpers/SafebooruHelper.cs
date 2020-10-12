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
        public string @Id { get; set; }
        public string @File_url { get; set; }
    }

    internal static class SafebooruHelper
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<string> GetRandomImage(string tag)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"https://safebooru.org/index.php?page=dapi&s=post&q=index&tags={tag}"),
                Method = HttpMethod.Get,
            };

            var response = await client.SendAsync(request);
            var respstring = await response.Content.ReadAsStringAsync();

            int status = (int)response.StatusCode;

            if (status != 200)
                return $"La API devolvió el código de respuesta: {status} {Enum.GetName(typeof(HttpStatusCode), status)}";

            XmlDocument xmlResult = new XmlDocument();
            xmlResult.LoadXml(respstring);

            JObject searchResult = JObject.Parse(JsonConvert.SerializeXmlNode(xmlResult));

            int totalItems = (int)searchResult["posts"]["@count"];
            if (totalItems <= 0)
                return $"No se encontraron imágenes con esa tag.";

            IList<JToken> postsResult = searchResult["posts"]["post"].ToList();

            var random = new Random();
            var ranCount = random.Next(0, postsResult.Count);

            return postsResult[ranCount]["@file_url"].ToString();
        }
    }
}
