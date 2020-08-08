using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using MarineBot.Database;
using MarineBot.Entities;
using MarineBot.Helpers;

namespace MarineBot.Threads
{
    public class ReminderThread
    {
        private CancellationTokenSource _cts;
        private ReminderTable _reminderTable;
        private DiscordClient _client;
        public ReminderThread(IServiceProvider serviceProvider)
        {
            var controller  = (DatabaseController)serviceProvider.GetService(typeof(DatabaseController));
            _reminderTable  = controller.GetTable<ReminderTable>();
            _cts            = (CancellationTokenSource)serviceProvider.GetService(typeof(CancellationTokenSource));
            _client         = (DiscordClient)serviceProvider.GetService(typeof(DiscordClient));
        }

        private async Task TriggerReminder(Reminder reminder)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0x003487),
                Title = "Recordatorio",
                ThumbnailUrl = FacesHelper.GetSuccessFace()
            };
            embed.AddField(reminder.Name, reminder.Description);

            var channel = await _client.GetChannelAsync(reminder.Channel);
            await channel.SendMessageAsync("@everyone", false, embed);

            return;
        }

        public async Task RunAsync()
        {
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

                await Task.Delay(60000);
            }
        }
    }
}