using System.Threading.Tasks;
using System.Net.Http;
using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace MarineBot.Helpers
{
    internal class ImgurImage
    {
        public string Title;
        public string Link;
        public string ImageLink;
        public string Uploader;

        public ImgurImage(string title, string link, string imglink, string uploader)
        {
            Title = title;
            Link = link;
            ImageLink = imglink;
            Uploader = uploader;
        }
    }

    internal class ImgurHelper
    {
        private string CLIENT_ID;
        private static readonly HttpClient client = new HttpClient();

        public ImgurHelper(string clientId)
        {
            CLIENT_ID = clientId;
        }

        public async Task<ImgurImage> GetRandomImage(string tag)
        {
            var request = new HttpRequestMessage() {
                RequestUri = new Uri($"https://api.imgur.com/3/gallery/t/{tag}/"),
                Method = HttpMethod.Get,
            };
            
            request.Headers.Add("Authorization", $"Client-ID {CLIENT_ID}");

            var response = await client.SendAsync(request);
            var respstring = await response.Content.ReadAsStringAsync();

            int status = (int)response.StatusCode;

            if (status != 200)
                throw new Exception($"La API devolvió el código de respuesta: {status} {Enum.GetName(typeof(HttpStatusCode), status)}");

            JObject imgurSearch = JObject.Parse(respstring);

            int totalItems = (int)imgurSearch["data"]["total_items"];
            if (totalItems <= 0)
                throw new Exception($"No se encontraron imágenes con esa tag.");

            IList<JToken> results = imgurSearch["data"]["items"].Children().ToList();
            IList<ImgurImage> searchResults = new List<ImgurImage>();
            foreach (JToken result in results)
            {
                ImgurImage searchResult;
                if ((bool)result["is_album"])
                {
                    searchResult = new ImgurImage(result["title"].ToString(), result["link"].ToString(),
                                                         result["images"][0]["link"].ToString(),
                                                         result["account_url"].ToString());
                } 
                else
                {
                    searchResult = new ImgurImage(result["title"].ToString(), result["link"].ToString(),
                                                         result["link"].ToString(),
                                                         result["account_url"].ToString());
                }
                searchResults.Add(searchResult);
            }

            var random = new Random();
            var ranCount = random.Next(0, searchResults.Count);

            return searchResults[ranCount];
        }
    }
}