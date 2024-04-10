using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MarineBot.Helpers
{
    internal class SaucenaoResult
    {
        public string Similarity;
        public string Thumbnail;
        public int IndexID;
        public string IndexName;
        public List<string> ExternalUrls;
        public Dictionary<string, string> ExtraInfo;
    }

    internal class SauceHelper
    {
        private static readonly HttpClient client = new HttpClient();
        private string API_KEY;

        public SauceHelper(string apikey)
        {
            API_KEY = apikey;
        }

        public async Task<List<SaucenaoResult>> SaucenaoSearch(string image_url)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"https://saucenao.com/search.php?db=999&api_key={API_KEY}&output_type=2&testmode=1&numres=6&url={Uri.EscapeDataString(image_url)}"),
                Method = HttpMethod.Get,
            };

            var response = await client.SendAsync(request);
            var respstring = await response.Content.ReadAsStringAsync();

            int status = (int)response.StatusCode;

            if (status != 200)
                throw new Exception($"API returned the response code: {status} {Enum.GetName(typeof(HttpStatusCode), status)}");

            JObject sauceObject = JObject.Parse(respstring);
            
            var saucecode = (int)sauceObject["header"]["status"];

            if (saucecode != 0)
                throw new Exception($"Saucenao returned the response code: {saucecode}");

            var sauceresults = (int)sauceObject["header"]["results_returned"];
            if (sauceresults <= 0)
                throw new Exception($"Could not find sauce for that image.");

            IList<JToken> sauceitems = sauceObject["results"].ToList();
            var resultitems = new List<SaucenaoResult>();

            for (int i = 0; i < sauceitems.Count; i++)
            {
                var currsauce = new SaucenaoResult()
                {
                    Similarity = sauceitems[i]["header"]["similarity"].ToString(),
                    Thumbnail = sauceitems[i]["header"]["thumbnail"].ToString(),
                    IndexID = (int)sauceitems[i]["header"]["index_id"],
                    IndexName = sauceitems[i]["header"]["index_name"].ToString(),
                    ExternalUrls = new List<string>(),
                    ExtraInfo = new Dictionary<string, string>()
                };
                
                var data = sauceitems[i]["data"] as JObject;

                foreach (var prop in data.Properties())
                {
                    if (prop.Name == "ext_urls")
                    {
                        currsauce.ExternalUrls = sauceitems[i]["data"]["ext_urls"].Select(x => x.ToString()).ToList();
                    }
                    else
                    {
                        currsauce.ExtraInfo.Add(prop.Name, prop.Value.ToString());
                    }
                }

                resultitems.Add(currsauce);
            }

            Console.WriteLine("finished saucing");
            return resultitems;
        }
    }
}
