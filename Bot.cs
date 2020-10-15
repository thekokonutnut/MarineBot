using DSharpPlus;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
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

        private PresenceThread          _presenceThread;
        private ReminderThread          _reminderthread;
        private PollThread              _pollthread;

        public Bot()
        {
            Console.WriteLine("[System] Initializing bot...");

            if (!File.Exists("config.json"))
            {
                new Config().SaveToFile("config.json");
                Console.WriteLine(@"[System] Config file not found. Creating new one.
                                    Please fill in the config.json that was generated.
                                    Press any key to exit..");
                Console.ReadKey();
                Environment.Exit(0);
            }

            this._config = Config.LoadFromFile("config.json");

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
            _dbcontroller   = new DatabaseController(_config._databaseConfig);

            if (!_dbcontroller.TestConnection())
            {
                Console.WriteLine("Press any key to exit..");
                Console.ReadKey();
                Environment.Exit(0);
            }

            var serviceProvider = new ServiceCollection()
                .AddSingleton<InteractivityExtension>   (this._interactivity)
                .AddSingleton<CancellationTokenSource>  (this._cts)
                .AddSingleton<DatabaseController>       (this._dbcontroller)
                .AddSingleton<CommandsInputController>  (this._cmdinput)
                .AddSingleton<Config>                   (this._config)
                .AddSingleton<DiscordClient>            (this._client)
                .BuildServiceProvider();

            _cnext = _client.UseCommandsNext(new CommandsNextConfiguration()
            {
                CaseSensitive           = false,
                EnableDefaultHelp       = true,
                EnableDms               = false,
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
            _cnext.RegisterCommands<Commands.ManagementCommands>();
            _cnext.RegisterCommands<Commands.ReminderCommands>();
            _cnext.RegisterCommands<Commands.PollCommands>();
            _cnext.RegisterCommands<Commands.UtilsCommands>();
            _cnext.RegisterCommands<Commands.ImageCommands>();

            _reminderthread = new ReminderThread(serviceProvider);
            _pollthread     = new PollThread(serviceProvider);
            _presenceThread = new PresenceThread(serviceProvider);

            _client.Ready           += OnReadyAsync;
            _client.GuildAvailable  += OnGuildAvailable;
            _client.ClientErrored   += OnClientErrored;
        }

        private Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            _client.Logger.Log(LogLevel.Information, $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'");
            return Task.CompletedTask;
        }

        private async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            _client.Logger.Log(LogLevel.Error, $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}");
            var ex = e.Exception;

            if (ex is ChecksFailedException)
            {
                var checks = (ex as ChecksFailedException).FailedChecks;
                if (checks.Any(c => c is DSharpPlus.CommandsNext.Attributes.RequireNsfwAttribute))
                    await MessageHelper.SendErrorEmbed(e.Context, "No puedes ejecutar este comándo en un canál no NSFW.");
                else
                    await MessageHelper.SendErrorEmbed(e.Context, "No tienes permiso para ejecutar este comándo.");
            }
            else if (ex is ArgumentException && e.Command != null)
            {
                if (e.Command.QualifiedName == null)
                {
                    var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");
                    await MessageHelper.SendErrorEmbed(e.Context, $"{emoji} Error al intentar ejecutar el comándo.");
                }
                else
                {
                    var ctx = e.Context;
                    var cmds = ctx.CommandsNext;
                    var context = cmds.CreateContext(ctx.Message, ctx.Prefix, cmds.FindCommand("help", out _), e.Command.QualifiedName);
                    await cmds.ExecuteCommandAsync(context);
                }
            }
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

            _client.Logger.Log(LogLevel.Information, "Client is ready to process events.");
            return Task.CompletedTask;
        }

        public async Task RunAsync()
        {
            await _dbcontroller.LoadEverything();

            await _client.ConnectAsync();
            await WaitForCancellationAsync();

            await _dbcontroller.SaveEverything();
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
