using MarineBot.Helpers;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using MarineBot.Entities;
using MarineBot.Controller;

namespace MarineBot.Commands
{
    [Group("Reminder"), Aliases("r")]
    [Description("Comandos de recordatorios.")]
    internal class ReminderCommands : BaseCommandModule
    {
        private Database.ReminderDatabase _database;
        private InteractivityExtension _interactivity;
        private CommandsInputController _cmdinput;
        public ReminderCommands(IServiceProvider serviceProvider)
        {
            _interactivity = (InteractivityExtension)serviceProvider.GetService(typeof(InteractivityExtension));
            _cmdinput = (CommandsInputController)serviceProvider.GetService(typeof(CommandsInputController));
            var controller = (DatabaseController)serviceProvider.GetService(typeof(DatabaseController));
            _database = new Database.ReminderDatabase(controller);
            _ = _database.TestConnection();
            _ = _database.CreateTableIfNull();
        }

        [Command("create")]
        [Description("Crea un recordatorio.")]
        public async Task CreateReminderCommand(CommandContext ctx, [Description("ID del recordatorio.")] string name)
        {
            if (_cmdinput.IsAvailable(ctx.User.Id))
                _cmdinput.SetUserAt(ctx.User.Id, MethodBase.GetCurrentMethod());
            else
                return;

            if (await _database.ReminderExists(name))
            {
                await MessageHelper.SendWarningEmbed(ctx, "Ese identificador ya está en uso.");
                _cmdinput.ReleaseUserIfMethod(ctx.User.Id, MethodBase.GetCurrentMethod());
                return;
            }

            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0x347aeb),
                Description = "Escribe una descripción:",
                Footer = MessageHelper.BuildFooter("Usa " + ctx.Prefix + "cancel para cancelar la operación."),
                Title = "Creando recordatorio **" + name + "**",
                ThumbnailUrl = FacesHelper.GetIdleFace()
            };
            var message = await ctx.RespondAsync(embed: embed);

            var cancelCmd = ctx.Prefix + "cancel";

            var descMsg = await _interactivity.WaitForMessageAsync(xm => xm.Author.Id == ctx.User.Id, TimeSpan.FromSeconds(30));
            if (descMsg.TimedOut)
            {
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":anger:"));
                _cmdinput.ReleaseUserIfMethod(ctx.User.Id, MethodBase.GetCurrentMethod());
                return;
            }
            if (descMsg.Result.Content.ToLower() == cancelCmd)
            {
                await MessageHelper.SendWarningEmbed(ctx, "Cancelaste la operación.");
                _cmdinput.ReleaseUserIfMethod(ctx.User.Id, MethodBase.GetCurrentMethod());
                return;
            }

            await descMsg.Result.DeleteAsync();

            embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0x347aeb),
                Description = "A que hora? (HH:MM):",
                Footer = MessageHelper.BuildFooter("Usa " + ctx.Prefix + "cancel para cancelar la operación."),
                Title = "Creando recordatorio **" + name + "**",
                ThumbnailUrl = FacesHelper.GetIdleFace()
            };
            embed.AddField("Ahora mismo son las: ", DateTime.UtcNow.ToString("HH:mm"));
            await message.ModifyAsync(null, new Optional<DiscordEmbed>(embed));

            Regex match = new Regex("\\d\\d\\:\\d\\d");
            var timeMsg = await _interactivity.WaitForMessageAsync(xm => xm.Author.Id == ctx.User.Id && (match.IsMatch(xm.Content) || xm.Content.ToLower() == cancelCmd), TimeSpan.FromSeconds(30));
            if (timeMsg.TimedOut)
            {
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":anger:"));
                _cmdinput.ReleaseUserIfMethod(ctx.User.Id, MethodBase.GetCurrentMethod());
                return;
            }
            if (timeMsg.Result.Content.ToLower() == cancelCmd)
            {
                await MessageHelper.SendWarningEmbed(ctx, "Cancelaste la operación.");
                _cmdinput.ReleaseUserIfMethod(ctx.User.Id, MethodBase.GetCurrentMethod());
                return;
            }

            await timeMsg.Result.DeleteAsync();

            embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0x347aeb),
                Description = "En que canal debo anunciar:",
                Footer = MessageHelper.BuildFooter("Usa " + ctx.Prefix + "cancel para cancelar la operación."),
                Title = "Creando recordatorio **" + name + "**",
                ThumbnailUrl = FacesHelper.GetIdleFace()
            };
            await message.ModifyAsync(null, new Optional<DiscordEmbed>(embed));

            var channelMsg = await _interactivity.WaitForMessageAsync(xm => xm.Author.Id == ctx.User.Id && (xm.Content.StartsWith("<#") || xm.Content.ToLower() == cancelCmd), TimeSpan.FromSeconds(30));
            if (channelMsg.TimedOut)
            {
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":anger:"));
                _cmdinput.ReleaseUserIfMethod(ctx.User.Id, MethodBase.GetCurrentMethod());
                return;
            }
            if (channelMsg.Result.Content.ToLower() == cancelCmd)
            {
                await MessageHelper.SendWarningEmbed(ctx, "Cancelaste la operación.");
                _cmdinput.ReleaseUserIfMethod(ctx.User.Id, MethodBase.GetCurrentMethod());
                return;
            }

            var channelMatch = new Regex("\\<#(\\d+)\\>").Match(channelMsg.Result.Content);
            ulong channelId = 0;

            if (channelMatch.Success)
                channelId = Convert.ToUInt64(channelMatch.Groups[1].Value);

            if (channelId == 0 || ctx.Guild.Channels[channelId] == null)
            {
                await MessageHelper.SendWarningEmbed(ctx, "No pude reconocer ese canal.");
                _cmdinput.ReleaseUserIfMethod(ctx.User.Id, MethodBase.GetCurrentMethod());
                return;
            }

            await channelMsg.Result.DeleteAsync();

            await _database.AddReminder(new Reminder(name, descMsg.Result.Content, timeMsg.Result.Content, ctx.Guild.Id, channelId), ctx);

            await message.DeleteAsync();

            await MessageHelper.SendSuccessEmbed(ctx, 
                $"Recordatorio creado con éxito.\nID: **{name}**" +
                $"\nHora: **{timeMsg.Result.Content}**" +
                $"\nCanal: **{ctx.Guild.Channels[channelId].Name}**");
            _cmdinput.ReleaseUserIfMethod(ctx.User.Id, MethodBase.GetCurrentMethod());
        }

        [Command("delete"), Aliases("remove")]
        [Description("Elimina el recordatorio especificado.")]
        public async Task DeleteRemindersCommand(CommandContext ctx, [Description("ID del recordatorio.")] string name)
        {
            await MessageHelper.SendWarningEmbed(ctx, "Method not implemented.");
        }

        [Command("list")]
        [Description("Lista todos los recordatorios activos.")]
        public async Task ListRemindersCommand(CommandContext ctx)
        {
            if (!_cmdinput.IsAvailable(ctx.User.Id))
                return;

            var embedBuilder = new DiscordEmbedBuilder()
                .WithTitle("Reminders")
                .WithColor(0x007FFF)
                .WithThumbnailUrl(FacesHelper.GetIdleFace());

            var remindersList = await _database.GetReminders(ctx);
            var guildList = new List<Reminder>();

            foreach (var reminder in remindersList)
                if (reminder.Guild == ctx.Guild.Id)
                    guildList.Add(reminder);

            if (guildList.Count == 0)
                embedBuilder.WithDescription("No hay ningún recordatorio activo.");
            else
                foreach (var reminder in guildList)
                    embedBuilder.AddField(reminder.Name, "Descripción: " + reminder.Description + "\nHora: **" + reminder.Time + "**");
            
            await ctx.RespondAsync(embed: embedBuilder);
        }
    }
}
