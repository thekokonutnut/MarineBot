using DSharpPlus;
using DSharpPlus.Entities;
using MarineBot.Controller;
using MarineBot.Database;
using MarineBot.Entities;
using MarineBot.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MarineBot.Threads
{
    class PresenceThread
    {
        private CancellationTokenSource _cts;
        private DiscordClient _client;
        private Config _config;
        private ActivityTable _activityTable;

        bool Running = false;

        public PresenceThread(IServiceProvider serviceProvider)
        {
            var controller = serviceProvider.GetService<DatabaseController>();
            _activityTable = controller.GetTable<ActivityTable>();

            _cts = serviceProvider.GetService<CancellationTokenSource>();
            _client = serviceProvider.GetService<DiscordClient>();
            _config = serviceProvider.GetService<Config>();
        }

        public async Task RunAsync()
        {
            if (Running)
                return;

            Running = true;

            var statusList = await _activityTable.GetActivitiesDB(true);
            var usedStatus = new List<int>();

            Console.WriteLine("[System] Presence Thread running.");

            while (!_cts.IsCancellationRequested)
            {
                if (usedStatus.Count >= statusList.Count)
                {
                    usedStatus.Clear();
                    statusList = await _activityTable.GetActivitiesDB(true);
                }

                var index = NumbersHelper.GetRandom(0, statusList.Count - 1);

                while (usedStatus.Contains(index))
                    index = NumbersHelper.GetRandom(0, statusList.Count - 1);

                usedStatus.Add(index);

                var newStatus = statusList[index];
                await ChangeStatus(newStatus);

                var delay = NumbersHelper.GetRandom(30, 40);
                Thread.Sleep(delay * 1000);
            }

            Console.WriteLine("[System] Presence Thread stopped.");
        }

        async Task ChangeStatus(ActivityEntry new_status)
        {
            string final_status = "";
            DiscordActivity activity;
            string text = new_status.Activity.Name;

            if (text.Length > 4)
            {
                int step = Convert.ToInt32(Math.Ceiling((double)text.Length / 4));

                for (int i = 0; i < 4; i++)
                {
                    var start_index = step * i;

                    if (start_index >= text.Length)
                        break;

                    if (start_index + step > text.Length)
                        final_status += text.Substring(start_index, text.Length - start_index);
                    else
                        final_status += text.Substring(start_index, step);

                    activity = new DiscordActivity($"{final_status}", new_status.Activity.ActivityType);
                    await _client.UpdateStatusAsync(activity);

                    Thread.Sleep(300);
                }
            }
            else
            {
                foreach (var c in text)
                {
                    final_status += c;

                    activity = new DiscordActivity($"{final_status}", new_status.Activity.ActivityType);
                    await _client.UpdateStatusAsync(activity);

                    Thread.Sleep(350);
                }
            }

            activity = new DiscordActivity($"{final_status} | {_config.Prefix}help", new_status.Activity.ActivityType);
            await _client.UpdateStatusAsync(activity);
        }
    }
}
