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
        private UserTable _userTable;

        private List<AuthUser> AuthUsers;

        public WebappServer(IPAddress address, int port, string servername, IServiceProvider serviceProvider) : base(address, port)
        {
            ServerName = servername;
            SiteHandler = new HttpSiteHandler(servername);
            _provider = serviceProvider;

            _client = serviceProvider.GetService<DiscordClient>();
            _dbControl = serviceProvider.GetService<DatabaseController>();

            _activityTable = _dbControl.GetTable<ActivityTable>();
            _userTable = _dbControl.GetTable<UserTable>();

            AuthUsers = new List<AuthUser>();

            SiteHandler.RegisterSite(FindActivitySite,      "/activity/id");
            SiteHandler.RegisterSite(ListActivitiesSite,    "/activity/list");
            SiteHandler.RegisterSite(AddActivitySite,       "/activity/add");
            SiteHandler.RegisterSite(DeleteActivitySite,    "/activity/delete");
            SiteHandler.RegisterSite(EditActivitySite,      "/activity/edit");
            SiteHandler.RegisterSite(AuthSite,              "/auth");
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

        private async Task AuthRegister(HttpSession session, string oauthCode)
        {
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

            var token = new AuthToken(response);
            OAuth2UserInfo info_response;

            try
            {
                info_response = await OAuth2Helper.RequestUserInfo(token.AccessToken);
            }
            catch (Exception e)
            {
                session.SendJSONError(e.Message);
                return;
            }

            var user = new AuthUser(info_response);

            var usersDB = await _userTable.GetUsersDB();
            var existingUser = usersDB.FirstOrDefault(u => u.DiscordID == user.DiscordID);

            AuthUserInfo info;

            if (existingUser is not null)
            {
                AuthUsers.Add(existingUser);

                info = new AuthUserInfo(existingUser);
                session.SendJSONObject(new { Error = false, Message = "Already registered.", Session = existingUser.Token.SessionCode, ID = existingUser.ID, Info = info });
                return;
            }

            var token_id = await _userTable.AddTokenDB(token);
            var user_id = await _userTable.AddUserDB(user, token_id);

            usersDB = await _userTable.GetUsersDB();
            var userDB = usersDB.FirstOrDefault(user => user.ID == user_id);

            AuthUsers.Add(userDB);

            info = new AuthUserInfo(existingUser);
            session.SendJSONObject(new { Error = false, Message = "Succesfully registered.", Session = token.SessionCode, UserID = user_id, Info = info });
        }

        private async Task AuthSite(HttpSession session, RequestContext rtx)
        {
            string authHeader = rtx.Headers.Get("Authentication");
            string oauthCode = rtx.Parameters.Get("code");

            if (authHeader == null)
            {
                await AuthRegister(session, oauthCode);
                return;
            }

            var users = await _userTable.GetUsersDB();
            var existingUser = users.FirstOrDefault(user => user.Token.SessionCode == authHeader);

            if (existingUser is null)
            {
                await AuthRegister(session, oauthCode);
                return;
            }

            var userInfo = new AuthUserInfo(existingUser);

            if (AuthUsers.Any(a => a.DiscordID == existingUser.DiscordID))
            {
                session.SendJSONObject(new { Error = false, Message = "Already logged in.", Session = existingUser.Token.SessionCode, ID = existingUser.ID, Info = userInfo });
                return;
            }

            // TODO: check expired token

            AuthUsers.Add(existingUser);

            session.SendJSONObject(new { Error = false, Message = "Succesfully authed.", Session = existingUser.Token.SessionCode, ID = existingUser.ID, Info = userInfo });
        }

        private async Task FindActivitySite(HttpSession session, RequestContext rtx)
        {
            string authHeader = rtx.Headers.Get("Authentication");

            if (authHeader == null)
            {
                session.SendJSONError("Missing authentication header.", 401);
                return;
            }

            var currentUser = AuthUsers.FirstOrDefault(user => user.Token.SessionCode == authHeader);

            if (currentUser is null)
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

            var currentUser = AuthUsers.FirstOrDefault(user => user.Token.SessionCode == authHeader);

            if (currentUser is null)
            {
                session.SendJSONError("Forbidden.", 403);
                return;
            }

            var activities = await _activityTable.GetActivitiesDB();
            IEnumerable<ActivityEntry> myActivities; 
            
            if (AuthHelper.BotAdministrator(currentUser.DiscordID.ToString()))
                myActivities = activities;
            else
                myActivities = activities.Where(g => g.UserID == currentUser.ID);

            var usersDB = await _userTable.GetUsersDB();

            foreach (var act in myActivities)
            {
                act.AddedBy = usersDB.FirstOrDefault(user => user.ID == act.UserID).DiscordID;
            }

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

            var currentUser = AuthUsers.FirstOrDefault(user => user.Token.SessionCode == authHeader);

            if (currentUser is null)
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
            var duplicated = activities.Any(g => g.Status == _activityText && (int)g.Type == activityType);

            if (duplicated)
            {
                session.SendJSONError("Entry already exists.", 403);
                return;
            }

            var entry = new ActivityEntry()
            {
                UserID = currentUser.ID,
                Type = (ActivityType)activityType,
                Status = _activityText
            };

            int id = await _activityTable.AddActivity(entry);
            await _activityTable.LoadTable();

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

            var currentUser = AuthUsers.FirstOrDefault(user => user.Token.SessionCode == authHeader);

            if (currentUser is null)
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

            var bodyParams = HttpUtility.ParseQueryString(rtx.Request.Body);

            var activities = await _activityTable.GetActivitiesDB();
            var found = activities.FirstOrDefault(g => g.ID == id);

            if (found == null)
            {
                session.SendJSONError("Entry not found.", 404);
                return;
            }

            if (!AuthHelper.BotAdministrator(currentUser.DiscordID.ToString()))
                if (found.UserID != currentUser.ID)
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
                UserID = currentUser.ID,
                Type = (ActivityType)activityType,
                Status = _activityText
            };

            await _activityTable.UpdateActivity(id, entry);
            await _activityTable.LoadTable();

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

            var currentUser = AuthUsers.FirstOrDefault(user => user.Token.SessionCode == authHeader);

            if (currentUser is null)
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

            if (!AuthHelper.BotAdministrator(currentUser.DiscordID.ToString()))
            if (found.UserID != currentUser.ID)
            {
                session.SendJSONError("Forbidden.", 403);
                return;
            }

            await _activityTable.RemoveActivity(id, currentUser.ID);
            await _activityTable.LoadTable();

            session.SendJSONObject(new { Error = false, ID = id });
        }
    }
}
