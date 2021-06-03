using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using MarineBot.Controller;
using MarineBot.Database;
using MarineBot.Entities;
using MarineBot.Helpers;

namespace MarineBot.Threads
{
    class ReminderThread
    {
        private CancellationTokenSource _cts;
        private ReminderTable _reminderTable;
        private DiscordClient _client;

        bool Running = false;
        public ReminderThread(IServiceProvider serviceProvider)
        {
            var controller  = (DatabaseController)serviceProvider.GetService(typeof(DatabaseController));
            _reminderTable  = controller.GetTable<ReminderTable>();
            _cts            = (CancellationTokenSource)serviceProvider.GetService(typeof(CancellationTokenSource));
            _client         = (DiscordClient)serviceProvider.GetService(typeof(DiscordClient));
        }

        private async Task TriggerReminder(Reminder reminder)
        {
            var embed = new DiscordEmbedBuilder()
                .WithColor(0x003487)
                .WithTitle("Recordatorio")
                .WithThumbnail(FacesHelper.GetSuccessFace());

            embed.AddField(reminder.Name, reminder.Description);

            var channel = await _client.GetChannelAsync(reminder.Channel);
            await channel.SendMessageAsync("@everyone", embed);

            return;
        }

        public async Task RunAsync()
        {
            if (Running)
                return;

            Running = true;

            Console.WriteLine("[System] Reminder Thread running.");
            
            while (!_cts.IsCancellationRequested)
            {
                var date = DateTime.UtcNow;
                var reminders =  _reminderTable.GetReminders();
                foreach (var reminder in reminders)
                {
                    if (reminder.Hour == date.Hour && reminder.Minute == date.Minute)
                        await TriggerReminder(reminder);
                }

                Thread.Sleep(60000);
            }
        }
    }
}