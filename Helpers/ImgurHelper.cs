using System.Threading.Tasks;
using System.Net.Http;
using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace MarineBot.Helpers
{
    public class ImgurImage
    {
        public string Title { get; set; }
        public string Link { get; set; }
    }

    public class ImgurHelper
    {
        private string CLIENT_ID = "";
        private static readonly HttpClient client = new HttpClient();

        public ImgurHelper(string clientId)
        {
            CLIENT_ID = clientId;
        }

        public async Task<string> GetRandomImage(string tag)
        {
            var request = new HttpRequestMessage() {
                RequestUri = new Uri($"https://api.imgur.com/3/gallery/t/{tag}/"),
                Method = HttpMethod.Get,
            };
            
            request.Headers.Add("Authorization", "Client-ID c8763935d9ee314");

            var response = await client.SendAsync(request);
            var respstring = await response.Content.ReadAsStringAsync();

            int status = (int)response.StatusCode;

            if (status != 200)
                return $"La API devolvió el código de respuesta: {status} {Enum.GetName(typeof(HttpStatusCode), status)}";

            JObject imgurSearch = JObject.Parse(respstring);

            int totalItems = (int)imgurSearch["data"]["total_items"];
            if (totalItems <= 0)
                return $"No se encontraron imágenes con esa tag.";

            IList<JToken> results = imgurSearch["data"]["items"].Children().ToList();
            IList<ImgurImage> searchResults = new List<ImgurImage>();
            foreach (JToken result in results)
            {
                ImgurImage searchResult = result.ToObject<ImgurImage>();
                searchResults.Add(searchResult);
            }

            var random = new Random();
            var ranCount = random.Next(0, searchResults.Count);

            return searchResults[ranCount].Link;
        }
    }
}