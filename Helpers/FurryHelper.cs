using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MarineBot.Helpers
{
    internal static class FurrAf_BrowseOptions_Data
    {
        public static Dictionary<string, int> Tags = new Dictionary<string, int>
        {
            { "All", 1 },
            { "Abstract", 2 },
            { "Animal related (non-anthro)", 3 },
            { "Anime", 4 },
            { "Comics", 5 },
            { "Doodle", 6 },
            { "Fanart", 7 },
            { "Fantasy", 8 },
            { "Human", 9 },
            { "Portraits", 10 },
            { "Scenery", 11 },
            { "Still Life", 12 },
            { "Tutorials", 13 },
            { "Miscellaneous", 14 },
            { "Baby fur", 101 },
            { "Bondage", 102 },
            { "Digimon", 103 },
            { "Fat Furs", 104 },
            { "Fetish Other", 105 },
            { "Fursuit", 106 },
            { "Gore / Macabre Art", 119 },
            { "Hyper", 107 },
            { "Inflation", 108 },
            { "Macro / Micro", 109 },
            { "Muscle", 110 },
            { "My Little Pony / Brony", 111 },
            { "Paw", 112 },
            { "Pokemon", 113 },
            { "Pregnancy", 114 },
            { "Sonic", 115 },
            { "Transformation", 116 },
            { "TF / TG", 120 },
            { "Vore", 117 },
            { "Water Sports", 118 },
            { "General Furry Art", 100 },
            { "Techno", 201 },
            { "Trance", 202 },
            { "House", 203 },
            { "90s", 204 },
            { "80s", 205 },
            { "70s", 206 },
            { "60s", 207 },
            { "Pre-60s", 208 },
            { "Classical", 209 },
            { "Game Music", 210 },
            { "Rock", 211 },
            { "Pop", 212 },
            { "Rap", 213 },
            { "Industrial", 214 },
            { "Other Music", 200 }
        };
        public static Dictionary<string, int> Gender = new Dictionary<string, int>
        {
            { "Any", 0 },
            { "Male", 2 },
            { "Female", 3 },
            { "Herm", 4 },
            { "Intersex", 11 },
            { "Trans (Male)", 8 },
            { "Trans (Female)", 9 },
            { "Non-Binary", 10 },
            { "Multiple characters", 6 },
            { "Other / Not Specified", 7 }
        };
        public static Dictionary<string, int> Rating = new Dictionary<string, int>
        {
            { "General", 0 },
            { "Mature", 1 },
            { "Adult", 2 },
            { "Any", 3 }
        };
    }

    internal class FurrAf_BrowseOptions
    {
        public int Tag = 1;
        public int Gender = 0;
        public int Category = 1;
        public int Rating = 3;
        public int Species = 1;

        public FurrAf_BrowseOptions(string tag = null, string gender = null, string rating = null)
        {
            if (tag != null)    Tag     = Parse(FurrAf_BrowseOptions_Data.Tags, tag);
            if (gender != null) Gender  = Parse(FurrAf_BrowseOptions_Data.Gender, gender);
            if (rating != null) Rating  = Parse(FurrAf_BrowseOptions_Data.Rating, rating);
        }

        public int Parse(Dictionary<string, int> dict, string value)
        {
            var val = dict.Where(e => e.Key.ToLower() == value.ToLower()).FirstOrDefault().Value;
            return val;
        }
    }

    internal class FurrAf_Entry
    {
        public int ID;
        public string Title;
        public string Description;
        public string Author;

        public FurrAf_Entry(int id, string title, string desc, string author)
        {
            ID = id;
            Title = title;
            Description = desc;
            Author = author;
        }
    }
    internal class FurryHelper
    {
        private const string UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.121 Safari/537.36";
        private HttpClient _client;

        public FurryHelper(string cookieA, string cookieB)
        {
            _client = new HttpClient();

            _client.BaseAddress = new Uri("https://www.furaffinity.net/");
            _client.DefaultRequestHeaders.Host = "www.furaffinity.net";
            _client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
            _client.DefaultRequestHeaders.Add("Cookie", $"a={cookieA}; b={cookieB};");
        }

        public async Task<List<FurrAf_Entry>> Browse(int page = 1, FurrAf_BrowseOptions options = null)
        {
            List<FurrAf_Entry> _list = new List<FurrAf_Entry>();

            HttpResponseMessage response;
            if (options != null)
            {
                var postValues = new Dictionary<string, string>
                {
                    { "cat",            options.Category.ToString() },
                    { "atype",          options.Tag.ToString() },
                    { "species",        options.Species.ToString() },
                    { "gender",         options.Gender.ToString() },
                    { "perpage",        "48" },
                    { "page",           page.ToString() },
                    { "rating_general", (options.Rating == 0 || options.Rating == 3) ? "on" : "off" },
                    { "rating_mature",  (options.Rating == 1 || options.Rating == 3) ? "on" : "off" },
                    { "rating_adult",   (options.Rating == 2 || options.Rating == 3) ? "on" : "off" },
                    { "go",             "Apply" }
                };

                var postData = new FormUrlEncodedContent(postValues);
                response = await _client.PostAsync($"/browse/", postData);
            }
            else
            {
                response = await _client.GetAsync($"/browse/{page}/");
            }

            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(responseBody);

            var scripts = doc.DocumentNode.SelectNodes("//script");
            string containerScript = scripts[7].InnerText;
            Match jsonMatch = Regex.Match(containerScript, @"var descriptions \= (.+)", RegexOptions.IgnoreCase);

            if (!jsonMatch.Success)
            {
                Console.WriteLine("Couldn't fetch entries.");
                return _list;
            }
            string entriesJson = jsonMatch.Groups[1].Value;
            JObject entries = JObject.Parse(entriesJson[0..^1]);

            foreach (var entry in entries)
                _list.Add(new FurrAf_Entry(Convert.ToInt32(entry.Key), entry.Value["title"]
                    .ToString(), entry.Value["description"].ToString(), entry.Value["username"].ToString()));

            return _list;
        }

        public async Task<Uri> GetImage(int id)
        {
            HttpResponseMessage response = await _client.GetAsync($"/view/{id}/");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(responseBody);

            var dwnlBtn = doc.DocumentNode.SelectSingleNode("//div[@class='download']/a");
            var imgSource = dwnlBtn.Attributes["href"].Value;

            return new Uri($"https:{imgSource}");
        }

    }
}
