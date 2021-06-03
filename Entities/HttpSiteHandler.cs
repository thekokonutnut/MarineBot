using NetCoreServer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MarineBot.Entities
{
    class RequestContext
    {
        public HttpRequest Request;
        public string ServerName;
        public string Path;
        public NameValueCollection Parameters;
        public NameValueCollection Headers;
    }

    public class SiteHandlerException : Exception
    {
        public int StatusCode { get; set; }

        public SiteHandlerException()
        {
        }

        public SiteHandlerException(string message)
            : base(message)
        {
            StatusCode = 500;
        }

        public SiteHandlerException(string message, int status)
            : base(message)
        {
            StatusCode = status;
        }
    }

    class HttpSiteHandler
    {
        public string ServerName;
        Dictionary<string, SiteDelegate> Sites = new Dictionary<string, SiteDelegate>();

        public delegate Task SiteDelegate(HttpSession session, RequestContext request);

        public HttpSiteHandler(string servername)
        {
            ServerName = servername;
        }

        public SiteDelegate FindSite(HttpRequest request)
        {
            var uriObj = new Uri(ServerName + request.Url);
            var requestCommand = uriObj.AbsolutePath;

            SiteDelegate cmd = null;

            Sites.TryGetValue(requestCommand, out cmd);

            return cmd;
        }

        public bool HandleSite(HttpSession session, HttpRequest request)
        {
            var site = FindSite(request);

            if (site == null)
            {
                throw new SiteHandlerException("Site not found", 404);
            }

            var uri = new Uri(ServerName + request.Url);
            var parameters = HttpUtility.ParseQueryString(uri.Query);

            var nvCollection = new NameValueCollection((int)request.Headers);
            for (int i = 0; i < request.Headers; i++)
            {
                var head = request.Header(i);
                nvCollection.Add(head.Item1, head.Item2);
            }

            RequestContext rtx = new RequestContext()
            {
                Request = request,
                ServerName = ServerName,
                Path = uri.AbsolutePath,
                Parameters = parameters,
                Headers = nvCollection
            };

            site(session, rtx);
            return true;
        }

        public void RegisterSite(SiteDelegate callback, string endpoint)
        {
            RegisterSite(callback, new string[] { endpoint });
        }

        public void RegisterSite(SiteDelegate callback, string[] endpoints)
        {
            foreach (var str in endpoints)
            {
                if (!Sites.ContainsKey(str))
                    Sites.Add(str, callback);
            }
        }
    }

    public static class HttpSessionExtension
    {
        public static void SendJSONError(this HttpSession sess, string message, int statuscode = 500)
        {
            sess.SendResponseAsync(sess.Response.MakeErrorResponse(statuscode, JsonConvert.SerializeObject(new { Error = true, Message = message })));
        }

        public static void SendJSONObject(this HttpSession sess, object obj)
        {
            sess.SendResponseAsync(sess.Response.MakeGetResponse(JsonConvert.SerializeObject(obj)));
        }
    }

    class CockHttpSession : HttpSession
    {
        public event EventHandler<RequestEventArgs> ReceivedRequest;

        public CockHttpSession(HttpServer server) : base(server)
        {
        }

        protected override void OnReceivedRequest(HttpRequest request)
        {
            var handler = ReceivedRequest;
            if (handler != null)
                handler(this, new RequestEventArgs() { Request = request });
        }

        protected override void OnReceivedRequestError(HttpRequest request, string error)
        {
            Console.WriteLine($"Request error: {error}");
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"HTTP session caught an error: {error}");
        }
    }

    public class RequestEventArgs
    {
        public HttpRequest Request { get; init; }
    }
}
