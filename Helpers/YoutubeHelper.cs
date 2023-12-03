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
    internal class YoutubeVideo
    {
        public string ID;
        public string Title;
        public string DownloadLink;
        public int Duration;
        public int Filesize;

        public YoutubeVideo(string id, string title, string dwnlink, int duration, int filesize)
        {
            ID = id;
            Title = title;
            DownloadLink = dwnlink;
            Duration = duration;
            Filesize = filesize;
        }
    }
    internal class YoutubeHelper
    {
        private string API_ENDPOINT;

        public YoutubeHelper(string api_endpoint)
        {
            API_ENDPOINT = api_endpoint;
        }

        private static readonly HttpClient client = new HttpClient();

        public async Task<YoutubeVideo> ProcessVideoURL(string url, string format)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"{API_ENDPOINT}/mp3.php?format={format}&url={url}"),
                Method = HttpMethod.Get,
            };

            var response = await client.SendAsync(request);
            var respstring = await response.Content.ReadAsStringAsync();

            int status = (int)response.StatusCode;

            if (status != 200)
                throw new Exception($"API returned the response code: {status} {Enum.GetName(typeof(HttpStatusCode), status)}");

            JObject apiReturn = JObject.Parse(respstring);

            if ((bool)apiReturn["error"])
            {
                string errorMessage = apiReturn["msg"].ToString();
                throw new Exception($"API returned the error message: {errorMessage}");
            }

            var item = new YoutubeVideo(apiReturn["id"].ToString(), apiReturn["title"].ToString(), apiReturn["download"].ToString(), (int)apiReturn["duration"], (int)apiReturn["filesize"]);
            return item;
        }
    }
}