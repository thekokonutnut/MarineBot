using DSharpPlus;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Net;
using DSharpPlus.Lavalink;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using MarineBot.Helpers;
using System.IO;
using DSharpPlus.Interactivity.Enums;
using Microsoft.Extensions.DependencyInjection;
using MarineBot.Threads;
using MarineBot.Controller;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext.Exceptions;
using MarineBot.Entities;
using System.Linq;
using Microsoft.Extensions.Logging;
using DSharpPlus.Interactivity.Extensions;
using MarineBot.Converters;
using MarineBot.Attributes;
using MarineBot.Database;

namespace MarineBot
{
    public class Bot : IDisposable
    {
        private DiscordClient           _client;
        private InteractivityExtension  _interactivity;
        private CommandsNextExtension   _cnext;
        private Config                  _config;
        private CancellationTokenSource _cts;
        private DatabaseController      _dbcontroller;
        private CommandsInputController _cmdinput;
        private WebappController        _webappControl;

        private PresenceThread          _presenceThread;
        private ReminderThread          _reminderthread;
        private PollThread              _pollthread;

        public bool HandledExit;

        public Bot()
        {
            Console.WriteLine("[System] Initializing bot...");

            if (!File.Exists("config.json"))
            {
                new Config().SaveToFile("config.json");
                Console.WriteLine("[System] Config file not found. Creating new one.\nPlease fill in the config.json that was generated.\nPress any key to exit..");
                Console.ReadKey();
                Environment.Exit(0);
            }

            this._config = Config.LoadFromFile("config.json");
            FacesHelper.ReloadConfig();
            AuthHelper.ReloadConfig();
            QuotesHelper.ReloadConfig();

            _client = new DiscordClient(new DiscordConfiguration()
            {
                AutoReconnect           = true,
                MinimumLogLevel         = LogLevel.Debug,
                Token                   = _config.Token,
                TokenType               = TokenType.Bot
            });

            _interactivity = _client.UseInteractivity(new InteractivityConfiguration()
            {
                PaginationBehaviour = PaginationBehaviour.Ignore,
                Timeout             = TimeSpan.FromSeconds(30)
            });

            _cts            = new CancellationTokenSource();
            _cmdinput       = new CommandsInputController();
            _dbcontroller   = new DatabaseController(_config.databaseConfig);

            if (!_dbcontroller.TestConnection())
            {
                Console.WriteLine("Press any key to exit..");
                Console.ReadKey();
                Environment.Exit(0);
            }

            var serviceProvider = new ServiceCollection()
                .AddSingleton(_interactivity)
                .AddSingleton(_cts)
                .AddSingleton(_dbcontroller)
                .AddSingleton(_cmdinput)
                .AddSingleton(_config)
                .AddSingleton(_client)
                .AddSingleton(this)
                .BuildServiceProvider();

            _cnext = _client.UseCommandsNext(new CommandsNextConfiguration()
            {
                CaseSensitive           = false,
                EnableDefaultHelp       = true,
                EnableDms               = true,
                EnableMentionPrefix     = true,
                StringPrefixes          = new string[] {_config.Prefix},
                IgnoreExtraArguments    = false,
                Services                = serviceProvider
            });

            _cnext.CommandExecuted  += Commands_CommandExecuted;
            _cnext.CommandErrored   += Commands_CommandErrored;

            _cnext.RegisterConverter(new DictConverter());
            _cnext.RegisterUserFriendlyTypeName<Dictionary<string, string>>("key-value pairs");

            _cnext.SetHelpFormatter<HelpFormatter>();
            _cnext.RegisterCommands<Commands.ReminderCommands>();
            _cnext.RegisterCommands<Commands.PollCommands>();
            _cnext.RegisterCommands<Commands.UtilsCommands>();
            _cnext.RegisterCommands<Commands.ImageCommands>();
            _cnext.RegisterCommands<Commands.ActionCommands>();
            _cnext.RegisterCommands<Commands.AdminCommands>();
            _cnext.RegisterCommands<Commands.MusicCommands>();

            _reminderthread = new ReminderThread(serviceProvider);
            _pollthread     = new PollThread(serviceProvider);
            _presenceThread = new PresenceThread(serviceProvider);
            _webappControl  = new WebappController(serviceProvider);

            _client.Ready           += OnReadyAsync;
            _client.GuildAvailable  += OnGuildAvailable;
            _client.ClientErrored   += OnClientErrored;
        }

        private Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            _client.Logger.Log(LogLevel.Information, $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'");

            if (e.Command.QualifiedName != "help")
                _ = LogTable.LogCommandInfo(e.Context);

            return Task.CompletedTask;
        }

        private async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            var ex = e.Exception;

            if (ex is CommandNotFoundException)
            {
                var ctx = e.Context;
                var cmds = ctx.CommandsNext;

                // I shouldn't be doing this by hand...
                string msg = ctx.Message.Content.Replace(ctx.Prefix, "");
                string msgCommand = msg.Split(" ").First();
                string msgArgs = string.Join(" ", msg.Split(" ").Skip(1));

                bool found = false;
                Command lastCmd = null;
                foreach (var parent in cmds.RegisteredCommands)
                {
                    var cmd = parent.Value;
                    if (cmd == lastCmd || cmd.Name == "help")
                        continue;

                    if (!cmd.CustomAttributes.Any(att => att is ShortCommandsGroupAttribute))
                        continue;

                    var subcommands = (cmd as CommandGroup).Children;
                    var foundCmd = subcommands.FirstOrDefault(s => s.Name == msgCommand);

                    if (foundCmd != null)
                    {
                        found = true;

                        var context = cmds.CreateContext(ctx.Message, ctx.Prefix, foundCmd, msgArgs);
                        _ = cmds.ExecuteCommandAsync(context);
                        break;
                    }

                    lastCmd = cmd;
                }

                if (found) return;
            }
            else if (ex is ChecksFailedException)
            {
                var checks = (ex as ChecksFailedException).FailedChecks;
                if (checks.Any(c => c is DSharpPlus.CommandsNext.Attributes.RequireNsfwAttribute))
                    await MessageHelper.SendErrorEmbed(e.Context, "You cannot run this command on a non-NSFW channel.");
                else
                    await MessageHelper.SendErrorEmbed(e.Context, "You do not have permission to execute this command.");
            }
            else if (ex is ArgumentException && e.Command != null)
            {
                if (e.Command.QualifiedName == null)
                {
                    var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");
                    await MessageHelper.SendErrorEmbed(e.Context, $"{emoji} Error when attempting to perform the command.");
                }
                else
                {
                    var ctx = e.Context;
                    var cmds = ctx.CommandsNext;
                    var context = cmds.CreateContext(ctx.Message, ctx.Prefix, cmds.FindCommand("help", out _), e.Command.QualifiedName);
                    _ = cmds.ExecuteCommandAsync(context);

                    return;
                }
            }

            _client.Logger.Log(LogLevel.Error, $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {ex.GetType()}: {ex.Message ?? "<no message>"}");
        }
        private Task OnClientErrored(DiscordClient sender, ClientErrorEventArgs e)
        {
            _client.Logger.Log(LogLevel.Error, $"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}");
            return Task.CompletedTask;
        }

        private Task OnGuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
        {
            _client.Logger.Log(LogLevel.Information, $"Guild available: {e.Guild.Name}");
            return Task.CompletedTask;
        }

        private Task OnReadyAsync(DiscordClient sender, ReadyEventArgs e)
        {
            _ = Task.Factory.StartNew(() => _reminderthread.RunAsync());
            _ = Task.Factory.StartNew(() => _pollthread.RunAsync());
            _ = Task.Factory.StartNew(() => _presenceThread.RunAsync());

            _ = Task.Factory.StartNew(() => _webappControl.StartServerAsync());

            _client.Logger.Log(LogLevel.Information, "Client is ready to process events.");
            return Task.CompletedTask;
        }

        public async Task RunAsync()
        {
            await _dbcontroller.LoadEverything();

            var lavalinkEndpoint = new ConnectionEndpoint
            {
                Hostname = "127.0.0.1",
                Port = 2333 // From your server configuration
            };

            var lavalinkConfig = new LavalinkConfiguration
            {
                Password = "kokito69",
                RestEndpoint = lavalinkEndpoint,
                SocketEndpoint = lavalinkEndpoint
            };

            var lavalink = _client.UseLavalink();

            await _client.ConnectAsync();
            await lavalink.ConnectAsync(lavalinkConfig); 
            await WaitForCancellationAsync();

            //await _dbcontroller.SaveEverything();
        }

        public void RequestShutdown()
        {
            HandledExit = true;
            _cts.Cancel();
        }

        public void RequestRestart()
        {
            _cts.Cancel();
        }

        private async Task WaitForCancellationAsync()
        {
            while (!_cts.IsCancellationRequested)
                await Task.Delay(500);
        }

        public void Dispose()
        {
            this._client.Dispose();
            this._interactivity = null;
            this._cnext = null;
            this._config = null;
            this._reminderthread = null;
            this._pollthread = null;
        }
    }
}
