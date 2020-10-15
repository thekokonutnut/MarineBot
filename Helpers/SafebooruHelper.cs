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

    internal static class SafebooruHelper
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<SafebooruImage> GetRandomImage(string tag)
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

            SafebooruImage result = new SafebooruImage()
            {
                Id = postsResult[ranCount]["@id"].ToString(),
                File_url = postsResult[ranCount]["@file_url"].ToString()
            };

            return result;
        }
    }
}
