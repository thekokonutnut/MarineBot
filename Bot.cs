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
using MarineBot.Controller;

namespace MarineBot
{
    public class Bot : IDisposable
    {
        private DiscordClient _client;
        private InteractivityExtension _interactivity;
        private CommandsNextExtension _cnext;
        private Config _config;
        private CancellationTokenSource _cts;
        private DatabaseController _dbcontroller;
        private CommandsInputController _cmdinput;
        private ReminderThread _reminderthread;

        public Bot()
        {
            Console.WriteLine("[System] Initializing bot...");

            if (!File.Exists("config.json"))
            {
                new Config().SaveToFile("config.json");
                Console.WriteLine("[System] Config file not found. Creating new one.");
                Console.WriteLine("Please fill in the config.json that was generated.");
                Console.WriteLine("Press any key to exit..");
                Console.ReadKey();
                Environment.Exit(0);
            }

            this._config = Config.LoadFromFile("config.json");

            _client = new DiscordClient(new DiscordConfiguration()
            {
                AutoReconnect = true,
                LogLevel = LogLevel.Debug,
                Token = _config.Token,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true
            });

            _interactivity = _client.UseInteractivity(new InteractivityConfiguration()
            {
                PaginationBehaviour = PaginationBehaviour.Ignore,
                Timeout = TimeSpan.FromSeconds(30)
            });

            _cts = new CancellationTokenSource();

            _dbcontroller = new DatabaseController(_config._databaseConfig);

            _cmdinput = new CommandsInputController();

            var serviceProvider = new ServiceCollection()
                .AddSingleton<InteractivityExtension>(this._interactivity)
                .AddSingleton<CancellationTokenSource>(this._cts)
                .AddSingleton<DatabaseController>(this._dbcontroller)
                .AddSingleton<CommandsInputController>(this._cmdinput)
                .AddSingleton<Config>(this._config)
                .AddSingleton<DiscordClient>(this._client)
                .BuildServiceProvider();

            _cnext = _client.UseCommandsNext(new CommandsNextConfiguration()
            {
                CaseSensitive = false,
                EnableDefaultHelp = true,
                EnableDms = false,
                EnableMentionPrefix = true,
                StringPrefixes = new string[] {_config.Prefix},
                IgnoreExtraArguments = true,
                Services = serviceProvider
            });

            _cnext.SetHelpFormatter<HelpFormatter>();
            _cnext.RegisterCommands<Commands.ManagementCommands>();
            _cnext.RegisterCommands<Commands.ReminderCommands>();
            _cnext.RegisterCommands<Commands.UtilsCommands>();

            _reminderthread = new ReminderThread(serviceProvider);

            _client.Ready += OnReadyAsync;
        }

        public async Task RunAsync()
        {
            await _client.ConnectAsync();
            _ = Task.Factory.StartNew(() => _reminderthread.RunAsync());
            await WaitForCancellationAsync();
        }

        private async Task WaitForCancellationAsync()
        {
            while (!_cts.IsCancellationRequested)
                await Task.Delay(500);
        }

        private async Task OnReadyAsync(ReadyEventArgs e)
        {
            await Task.Yield();
            //_starttimes.SocketStart = DateTime.Now;
            Console.WriteLine("[System] Bot is ready.");
        }

        public void Dispose()
        {
            this._client.Dispose();
            this._interactivity = null;
            this._cnext = null;
            this._config = null;
        }
    }
}
