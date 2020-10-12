using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace MarineBot.Helpers
{
    internal class FurryHelper
    {
        private HttpClient _client;

        public FurryHelper(string cookieA, string cookieB)
        {
            _client = new HttpClient();

            _client.BaseAddress = new Uri("https://www.furaffinity.net/");
            _client.DefaultRequestHeaders.Host = "www.furaffinity.net";
            _client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.121 Safari/537.36");
            _client.DefaultRequestHeaders.Add("Cookie", $"a={cookieA}; b={cookieB};");
        }


    }
}
