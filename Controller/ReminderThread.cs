using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using MarineBot.Entities;
using MarineBot.Helpers;

namespace MarineBot.Controller
{
    public class ReminderThread
    {
        private CancellationTokenSource _cts;
        private Database.ReminderDatabase _database;
        private DiscordClient _client;
        public ReminderThread(IServiceProvider serviceProvider)
        {
            var controller = (DatabaseController)serviceProvider.GetService(typeof(DatabaseController));
            _database = new Database.ReminderDatabase(controller);
            _cts = (CancellationTokenSource)serviceProvider.GetService(typeof(CancellationTokenSource));
            _client = (DiscordClient)serviceProvider.GetService(typeof(DiscordClient));
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
                var reminders = await _database.GetReminders();
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