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
                LogLevel                = LogLevel.Debug,
                Token                   = _config.Token,
                TokenType               = TokenType.Bot,
                UseInternalLogHandler   = true
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

        private async Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            _client.DebugLogger.LogMessage(LogLevel.Error, "MarineBot", $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);
            var ex = e.Exception;

            if (ex is ChecksFailedException)
            {
                var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");
                await MessageHelper.SendErrorEmbed(e.Context, $"{emoji} No tienes permiso para ejecutar ese comándo.");
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

        private Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            _client.DebugLogger.LogMessage(LogLevel.Info, "MarineBot", $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'", DateTime.Now);
            return Task.CompletedTask;
        }

        private Task OnClientErrored(ClientErrorEventArgs e)
        {
            _client.DebugLogger.LogMessage(LogLevel.Error, "MarineBot", $"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);
            return Task.CompletedTask;
        }

        private Task OnReadyAsync(ReadyEventArgs e)
        {
            _ = Task.Factory.StartNew(() => _reminderthread.RunAsync());
            _ = Task.Factory.StartNew(() => _pollthread.RunAsync());
            _ = Task.Factory.StartNew(() => _presenceThread.RunAsync());

            _client.DebugLogger.LogMessage(LogLevel.Info, "MarineBot", "Client is ready to process events.", DateTime.Now);
            return Task.CompletedTask;
        }

        private Task OnGuildAvailable(GuildCreateEventArgs e)
        {
            _client.DebugLogger.LogMessage(LogLevel.Info, "MarineBot", $"Guild available: {e.Guild.Name}", DateTime.Now);
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
