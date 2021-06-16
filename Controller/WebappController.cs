using DSharpPlus;
using MarineBot.Database;
using MarineBot.Entities;
using MarineBot.Helpers;
using NetCoreServer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using DSharpPlus.Entities;
using System.Web;

namespace MarineBot.Controller
{
    internal class WebappController
    {
        private CancellationTokenSource _cts;
        private WebappServer server;

        public WebappController(IServiceProvider serviceProvider)
        {
            _cts = serviceProvider.GetService<CancellationTokenSource>();

            server = new WebappServer(IPAddress.Any, 17701, "http://marinebot:17701", serviceProvider);
        }

        public async Task StartServerAsync()
        {
            Console.WriteLine("[Webapp Server] Server starting...");
            server.Start();
            Console.WriteLine("[Webapp Server] Done!");

            await WaitForCancellationAsync();

            Console.WriteLine("[Webapp Server] Server stopping...");
            server.Stop();
            Console.WriteLine("[Webapp Server] Done!");
        }

        private async Task WaitForCancellationAsync()
        {
            while (!_cts.IsCancellationRequested)
                await Task.Delay(500);
        }
    }

    internal class WebappSession : CockHttpSession
    {
        public WebappSession(HttpServer server) : base(server)
        {
            Console.WriteLine("[Webapp Session] Initializing...");
        }
    }

    internal class WebappServer : HttpServer
    {
        public string ServerName;
        private HttpSiteHandler SiteHandler;

        private IServiceProvider _provider;

        private DiscordClient _client;
        private DatabaseController _dbControl;

        private ActivityTable _activityTable;

        private Dictionary<string, OAuth2Response> AuthSessions;
        private Dictionary<string, OAuth2UserInfo> UserInfo;

        public WebappServer(IPAddress address, int port, string servername, IServiceProvider serviceProvider) : base(address, port)
        {
            ServerName = servername;
            SiteHandler = new HttpSiteHandler(servername);
            _provider = serviceProvider;

            _client = serviceProvider.GetService<DiscordClient>();
            _dbControl = serviceProvider.GetService<DatabaseController>();

            _activityTable = _dbControl.GetTable<ActivityTable>();

            AuthSessions = new Dictionary<string, OAuth2Response>();

            UserInfo = new Dictionary<string, OAuth2UserInfo>();

            SiteHandler.RegisterSite(FindActivitySite, "/activity/id");
            SiteHandler.RegisterSite(ListActivitiesSite, "/activity/list");
            SiteHandler.RegisterSite(AddActivitySite, "/activity/add");
            SiteHandler.RegisterSite(DeleteActivitySite, "/activity/delete");
            SiteHandler.RegisterSite(EditActivitySite, "/activity/edit");
            SiteHandler.RegisterSite(AuthExpiredSite, "/auth/expire");
            SiteHandler.RegisterSite(AuthInfoSite, "/auth/info");
            SiteHandler.RegisterSite(AuthSite, "/auth");
        }

        protected override TcpSession CreateSession() 
        {
            var sess = new CockHttpSession(this);
            sess.ReceivedRequest += HandleReceivedRequest;
            return sess;
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"[Webapp Server ERROR]: {error}");
        }

        private void HandleReceivedRequest(object sender, RequestEventArgs args)
        {
            var request = args.Request;
            var session = (HttpSession)sender;

            try
            {
                SiteHandler.HandleSite(session, request);
            }
            catch (SiteHandlerException e)
            {
                session.SendJSONError(e.Message, e.StatusCode);
            }
            catch (Exception e)
            {
                session.SendResponse(session.Response.MakeErrorResponse(500));
                Console.WriteLine($"[Webapp Server] Exception: {e.Message}\n\n{e.StackTrace}");
            }
        }

        private Task AuthExpiredSite(HttpSession session, RequestContext rtx)
        {
            string authHeader = rtx.Headers.Get("Authentication");

            if (authHeader == null)
            {
                session.SendJSONError("Missing authentication header.", 401);
                return Task.CompletedTask;
            }

            if (!AuthSessions.ContainsKey(authHeader))
            {
                session.SendJSONError("Forbidden.", 403);
                return Task.CompletedTask;
            }

            var sessObj = AuthSessions[authHeader];

            var expired = DateTime.Now > sessObj.Requested.AddSeconds(sessObj.Lifespan);

            session.SendJSONObject(new { Error = false, Requested = sessObj.Requested.ToString(), sessObj.Lifespan, Expired = expired });
            return Task.CompletedTask;
        }

        private async Task AuthInfoSite(HttpSession session, RequestContext rtx)
        {
            string authHeader = rtx.Headers.Get("Authentication");

            if (authHeader == null)
            {
                session.SendJSONError("Missing authentication header.", 401);
                return;
            }

            if (!AuthSessions.ContainsKey(authHeader))
            {
                session.SendJSONError("Forbidden.", 403);
                return;
            }

            var sessObj = AuthSessions[authHeader];

            OAuth2UserInfo response;

            try
            {
                response = await OAuth2Helper.RequestUserInfo(sessObj.Token);
            }
            catch (Exception e)
            {
                session.SendJSONError(e.Message);
                return;
            }

            UserInfo[authHeader] = response;
            session.SendJSONObject(new { Error = false, User = response });
            return;
        }

        private async Task AuthSite(HttpSession session, RequestContext rtx)
        {
            string oauthCode = rtx.Parameters.Get("code");

            if (oauthCode == null)
            {
                session.SendJSONError("Missing parameter: code", 400);
                return;
            }

            OAuth2Response response;

            try
            {
                response = await OAuth2Helper.RequestAccessToken(oauthCode);
            }
            catch (Exception e)
            {
                session.SendJSONError(e.Message);
                return;
            }

            var hash = CryptoHelper.CreateMD5(response.Token);

            AuthSessions[hash] = response;

            session.SendJSONObject(new { Error = false, Session = hash, Token = response.Token });
        }

        private async Task FindActivitySite(HttpSession session, RequestContext rtx)
        {
            string authHeader = rtx.Headers.Get("Authentication");

            if (authHeader == null)
            {
                session.SendJSONError("Missing authentication header.", 401);
                return;
            }

            if (!AuthSessions.ContainsKey(authHeader))
            {
                session.SendJSONError("Forbidden.", 403);
                return;
            }

            string requestId = rtx.Parameters.Get("id");
            if (requestId == null)
            {
                session.SendJSONError("Missing parameter id.", 400);
                return;
            }

            int id = -1;
            if (!int.TryParse(requestId, out id))
            {
                session.SendJSONError("Invalid ID.", 400);
                return;
            }

            var activities = await _activityTable.GetActivitiesDB();
            var found = activities.FirstOrDefault(g => g.ID == id);

            if (found == null)
            {
                session.SendJSONError("Entry not found.", 404);
                return;
            }

            session.SendJSONObject(new { Error = false, Entry = found });
        }

        private async Task ListActivitiesSite(HttpSession session, RequestContext rtx)
        {
            string authHeader = rtx.Headers.Get("Authentication");

            if (authHeader == null)
            {
                session.SendJSONError("Missing authentication header.", 401);
                return;
            }

            if (!AuthSessions.ContainsKey(authHeader))
            {
                session.SendJSONError("Forbidden.", 403);
                return;
            }

            if (!UserInfo.ContainsKey(authHeader))
            {
                session.SendJSONError("User info not retrieved: request /auth/info", 404);
                return;
            }

            var activities = await _activityTable.GetActivitiesDB();
            IEnumerable<ActivityEntry> myActivities; 
            
            if (AuthHelper.BotAdministrator(UserInfo[authHeader].ID.ToString()))
                myActivities = activities;
            else
                myActivities = activities.Where(g => g.AddedBy == UserInfo[authHeader].ID);

            session.SendJSONObject(new { Error = false, List = myActivities });
        }

        private async Task AddActivitySite(HttpSession session, RequestContext rtx)
        {
            string authHeader = rtx.Headers.Get("Authentication");

            if (authHeader == null)
            {
                session.SendJSONError("Missing authentication header.", 401);
                return;
            }

            if (!AuthSessions.ContainsKey(authHeader))
            {
                session.SendJSONError("Forbidden.", 403);
                return;
            }

            if (!UserInfo.ContainsKey(authHeader))
            {
                session.SendJSONError("User info not retrieved: request /auth/info", 404);
                return;
            }

            string contentType = rtx.Headers.Get("Content-Type");

            if (contentType != "application/x-www-form-urlencoded")
            {
                session.SendJSONError("Invalid content type.", 400);
                return;
            }

            var bodyParams = HttpUtility.ParseQueryString(rtx.Request.Body);

            string _activityText = bodyParams.Get("activity");
            string _type = bodyParams.Get("type");

            if (_activityText == null || _type == null)
            {
                session.SendJSONError("Missing parameters: activity, type", 400);
                return;
            }

            int activityType = -1;
            if (!int.TryParse(_type, out activityType))
            {
                session.SendJSONError("Invalid activity type.", 400);
                return;
            }
            if (activityType < 0 || activityType > 5)
            {
                session.SendJSONError("Invalid activity type.", 400);
                return;
            }

            var activities = _activityTable.GetEntries();
            var duplicated = activities.Any(g => g.Activity.Name == _activityText && (int)g.Activity.ActivityType == activityType);

            if (duplicated)
            {
                session.SendJSONError("Entry already exists.", 403);
                return;
            }

            var entry = new ActivityEntry()
            {
                AddedBy = UserInfo[authHeader].ID,
                Activity = new DiscordActivity()
                {
                    ActivityType = (ActivityType)activityType,
                    Name = _activityText
                }
            };

            int id = _activityTable.CreateEntry(entry);
            await _activityTable.SaveChanges();

            session.SendJSONObject(new { Error = false, ID = id });
        }

        private async Task EditActivitySite(HttpSession session, RequestContext rtx)
        {
            string authHeader = rtx.Headers.Get("Authentication");

            if (authHeader == null)
            {
                session.SendJSONError("Missing authentication header.", 401);
                return;
            }

            if (!AuthSessions.ContainsKey(authHeader))
            {
                session.SendJSONError("Forbidden.", 403);
                return;
            }

            string requestId = rtx.Parameters.Get("id");
            if (requestId == null)
            {
                session.SendJSONError("Missing parameter id.", 400);
                return;
            }

            int id = -1;
            if (!int.TryParse(requestId, out id))
            {
                session.SendJSONError("Invalid ID.", 400);
                return;
            }

            if (!UserInfo.ContainsKey(authHeader))
            {
                session.SendJSONError("User info not retrieved: request /auth/info", 404);
                return;
            }

            var bodyParams = HttpUtility.ParseQueryString(rtx.Request.Body);

            var activities = await _activityTable.GetActivitiesDB();
            var found = activities.FirstOrDefault(g => g.ID == id);

            if (found == null)
            {
                session.SendJSONError("Entry not found.", 404);
                return;
            }

            if (!AuthHelper.BotAdministrator(UserInfo[authHeader].ID.ToString()))
                if (found.AddedBy != UserInfo[authHeader].ID)
                {
                    session.SendJSONError("Forbidden.", 403);
                    return;
                }

            string contentType = rtx.Headers.Get("Content-Type");

            if (contentType != "application/x-www-form-urlencoded")
            {
                session.SendJSONError("Invalid content type.", 400);
                return;
            }

            string _activityText = bodyParams.Get("activity");
            string _type = bodyParams.Get("type");

            if (_activityText == null || _type == null)
            {
                session.SendJSONError("Missing parameters: activity, type", 400);
                return;
            }

            int activityType = -1;
            if (!int.TryParse(_type, out activityType))
            {
                session.SendJSONError("Invalid activity type.", 400);
                return;
            }
            if (activityType < 0 || activityType > 5)
            {
                session.SendJSONError("Invalid activity type.", 400);
                return;
            }

            var entry = new ActivityEntry()
            {
                ID = id,
                AddedBy = UserInfo[authHeader].ID,
                Activity = new DiscordActivity()
                {
                    ActivityType = (ActivityType)activityType,
                    Name = _activityText
                }
            };

            _activityTable.UpdateEntry(id, entry);
            await _activityTable.SaveChanges();

            session.SendJSONObject(new { Error = false, ID = id });
        }

        private async Task DeleteActivitySite(HttpSession session, RequestContext rtx)
        {
            string authHeader = rtx.Headers.Get("Authentication");

            if (authHeader == null)
            {
                session.SendJSONError("Missing authentication header.", 401);
                return;
            }

            if (!AuthSessions.ContainsKey(authHeader))
            {
                session.SendJSONError("Forbidden.", 403);
                return;
            }

            string requestId = rtx.Parameters.Get("id");
            if (requestId == null)
            {
                session.SendJSONError("Missing parameter id.", 400);
                return;
            }

            int id = -1;
            if (!int.TryParse(requestId, out id))
            {
                session.SendJSONError("Invalid ID.", 400);
                return;
            }

            if (!UserInfo.ContainsKey(authHeader))
            {
                session.SendJSONError("User info not retrieved: request /auth/info", 404);
                return;
            }

            var activities = await _activityTable.GetActivitiesDB();
            var found = activities.FirstOrDefault(g => g.ID == id);

            if (found == null)
            {
                session.SendJSONError("Entry not found.", 404);
                return;
            }

            if (!AuthHelper.BotAdministrator(UserInfo[authHeader].ID.ToString()))
            if (found.AddedBy != UserInfo[authHeader].ID)
            {
                session.SendJSONError("Forbidden.", 403);
                return;
            }

            _activityTable.RemoveEntry(id);
            await _activityTable.SaveChanges();

            session.SendJSONObject(new { Error = false, ID = id });
        }
    }
}
