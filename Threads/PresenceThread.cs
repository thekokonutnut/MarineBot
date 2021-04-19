using DSharpPlus;
using DSharpPlus.Entities;
using MarineBot.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MarineBot.Threads
{
    class PresenceThread
    {
        private CancellationTokenSource _cts;
        private DiscordClient _client;
        private Config _config;

        public PresenceThread(IServiceProvider serviceProvider)
        {
            _cts = (CancellationTokenSource)serviceProvider.GetService(typeof(CancellationTokenSource));
            _client = (DiscordClient)serviceProvider.GetService(typeof(DiscordClient));
            _config = (Config)serviceProvider.GetService(typeof(Config));
        }

        public async Task RunAsync()
        {
            Console.WriteLine("[System] Presence Thread running.");

            string[] statusList = _config.statusMessages;

            while (!_cts.IsCancellationRequested)
            {
                var ran = NumbersHelper.GetRandom(0, statusList.Length - 1);
                var currStatus = statusList[ran];
#if DEBUG
                currStatus = "localhost xd";
#endif

                var activity = new DiscordActivity($"{currStatus} ({_config.Prefix}help)", ActivityType.Playing);
                await _client.UpdateStatusAsync(activity);

                await Task.Delay(120000);
            }
        }
    }
}
