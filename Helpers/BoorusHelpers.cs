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
    #region Safebooru
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
                throw new Exception($"API returned the response code: {status} {Enum.GetName(typeof(HttpStatusCode), status)}");

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
                throw new Exception("No images found with that tag.");

            var random = new Random();

            if (postCount > 100)
            {
                int maxPage = (int)Math.Ceiling(postCount / 100.0);
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
                throw new Exception($"API returned the response code: {status} {Enum.GetName(typeof(HttpStatusCode), status)}");

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
                throw new Exception($"API returned the response code: {status} {Enum.GetName(typeof(HttpStatusCode), status)}");

            XmlDocument xmlResult = new XmlDocument();
            xmlResult.LoadXml(respstring);

            JObject searchResult = JObject.Parse(JsonConvert.SerializeXmlNode(xmlResult));

            if (searchResult["tags"] == null)
                throw new Exception("No tags were found.");

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

                    if (result.Count == 20) break;
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
    #endregion

    #region Gelbooru
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
                throw new Exception($"API returned the response code: {status} {Enum.GetName(typeof(HttpStatusCode), status)}");

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
                throw new Exception("No images found with that tag.");

            var random = new Random();

            if (postCount > 100)
            {
                int maxPage = (int)Math.Ceiling(postCount / 100.0);
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
                throw new Exception($"API returned the response code: {status} {Enum.GetName(typeof(HttpStatusCode), status)}");

            JObject searchResult = JObject.Parse(respstring);

            var ranCount = random.Next(0, searchResult.Count);

            GelbooruImage result = new GelbooruImage()
            {
                Id = searchResult["post"][ranCount]["id"].ToString(),
                File_url = searchResult["post"][ranCount]["file_url"].ToString()
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
                throw new Exception($"API returned the response code: {status} {Enum.GetName(typeof(HttpStatusCode), status)}");

            JObject searchResult = JObject.Parse(respstring);

            int tagCount = Convert.ToInt32(searchResult["@attributes"]["count"]);
            int limit = Convert.ToInt32(searchResult["@attributes"]["limit"]);

            if (tagCount <= 0)
                throw new Exception("No tags were found.");

            var result = new List<GelbooruTag>();

            for (int i = 0; i < tagCount; i++)
            {
                if (i == limit) break;
                
                var currtag = new GelbooruTag()
                {
                    Name = searchResult["tag"][i]["name"].ToString(),
                    Count = searchResult["tag"][i]["count"].ToString()
                };

                result.Add(currtag);
            }

            return result;
        }
    }
    #endregion

    #region E621
    internal class E621Image
    {
        public string Id;
        public string ImageUrl;
    }

    internal class E621Helper
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<E621Image> GetRandomImage(string tag)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"https://e621.net/posts.json?limit=200&tags={tag}"),
                Method = HttpMethod.Get,
            };

            request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            request.Headers.Add("User-Agent", "MarineBot/1.69");

            var response = await client.SendAsync(request);
            var respstring = await response.Content.ReadAsStringAsync();

            int status = (int)response.StatusCode;

            if (status != 200)
                throw new Exception($"API returned the response code: {status} {Enum.GetName(typeof(HttpStatusCode), status)}");

            JObject searchResult = JObject.Parse(respstring);

            IList<JToken> postsResult = searchResult["posts"].ToList();

            int totalItems = postsResult.Count;
            if (totalItems <= 0)
                throw new Exception($"No images found with that tag.");

            var random = new Random();
            var ranCount = random.Next(0, postsResult.Count);

            E621Image result = new E621Image()
            {
                Id = postsResult[ranCount]["id"].ToString(),
                ImageUrl = postsResult[ranCount]["file"]["url"].ToString()
            };

            return result;
        }

    }
    #endregion
}
