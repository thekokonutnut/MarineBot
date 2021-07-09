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
using MarineBot.Threads;
using MarineBot.Database;
using MarineBot.Controller;
using MarineBot.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace MarineBot.Commands
{
    [Group("Reminder"), Aliases("r")]
    [Description("Reminder commands.")]
    [RequireGuild]
    internal class ReminderCommands : BaseCommandModule
    {
        private ReminderTable _reminderTable;
        public InteractivityExtension _interactivity { private get; set; }
        public CommandsInputController _cmdinput { private get; set; }

        public ReminderCommands(IServiceProvider serviceProvider)
        {
            var controller = serviceProvider.GetService<DatabaseController>();
            _reminderTable = controller.GetTable<ReminderTable>();
        }

        [GroupCommand(), Hidden()]
        public async Task MainCommand(CommandContext ctx)
        {
            var cmds = ctx.CommandsNext;
            var context = cmds.CreateContext(ctx.Message, ctx.Prefix, cmds.FindCommand("help", out _), ctx.Command.QualifiedName);
            await cmds.ExecuteCommandAsync(context);
        }

        [Command("create"), Description("Create a reminder.")]
        [Example("reminder create La torta en el horno", "r create Que")]
        public async Task CreateReminderCommand(CommandContext ctx, [Description("Reminder name."), RemainingText()] string name)
        {
            if (name == null) throw new ArgumentException();

            if (_cmdinput.IsAvailable(ctx.User.Id))
                _cmdinput.SetUserAt(ctx.User.Id, MethodBase.GetCurrentMethod());
            else
                return;

            if (_reminderTable.ReminderExists(name))
            {
                await MessageHelper.SendWarningEmbed(ctx, "That identifier is already in use.");
                _cmdinput.ReleaseUserIfMethod(ctx.User.Id, MethodBase.GetCurrentMethod());
                return;
            }

            var embed = new DiscordEmbedBuilder()
                .WithColor(0x347aeb)
                .WithDescription("Write a description:")
                .WithFooter("Use " + ctx.Prefix + "cancel to cancel the operation.")
                .WithTitle("Creating reminder **" + name + "**")
                .WithThumbnail(FacesHelper.GetIdleFace());

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
                await MessageHelper.SendWarningEmbed(ctx, "You canceled the operation.");
                _cmdinput.ReleaseUserIfMethod(ctx.User.Id, MethodBase.GetCurrentMethod());
                return;
            }

            await descMsg.Result.DeleteAsync();

            embed = new DiscordEmbedBuilder()
                .WithColor(0x347aeb)
                .WithDescription("What time? (HH:MM):")
                .WithFooter("Use " + ctx.Prefix + "cancel to cancel the operation.")
                .WithTitle("Creating reminder **" + name + "**")
                .WithThumbnail(FacesHelper.GetIdleFace());

            embed.AddField("Current time is: ", DateTime.UtcNow.ToString("HH:mm"));
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
                await MessageHelper.SendWarningEmbed(ctx, "You canceled the operation.");
                _cmdinput.ReleaseUserIfMethod(ctx.User.Id, MethodBase.GetCurrentMethod());
                return;
            }

            await timeMsg.Result.DeleteAsync();

            embed = new DiscordEmbedBuilder()
                .WithColor(0x347aeb)
                .WithDescription("On which channel should I announce:")
                .WithFooter("Use " + ctx.Prefix + "cancel to cancel the operation.")
                .WithTitle("Creating reminder **" + name + "**")
                .WithThumbnail(FacesHelper.GetIdleFace());

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
                await MessageHelper.SendWarningEmbed(ctx, "You canceled the operation.");
                _cmdinput.ReleaseUserIfMethod(ctx.User.Id, MethodBase.GetCurrentMethod());
                return;
            }

            var channelMatch = new Regex("\\<#(\\d+)\\>").Match(channelMsg.Result.Content);
            ulong channelId = 0;

            if (channelMatch.Success)
                channelId = Convert.ToUInt64(channelMatch.Groups[1].Value);

            if (channelId == 0 || ctx.Guild.Channels[channelId] == null)
            {
                await MessageHelper.SendWarningEmbed(ctx, "I could not identify that channel.");
                _cmdinput.ReleaseUserIfMethod(ctx.User.Id, MethodBase.GetCurrentMethod());
                return;
            }

            await channelMsg.Result.DeleteAsync();

            _reminderTable.AddReminder(new Reminder(name, descMsg.Result.Content, timeMsg.Result.Content, ctx.Guild.Id, channelId));

            await message.DeleteAsync();

            await MessageHelper.SendSuccessEmbed(ctx, 
                $"Reminder successfully created.\nID: **{name}**" +
                $"\nHour: **{timeMsg.Result.Content}**" +
                $"\nChannel: **{ctx.Guild.Channels[channelId].Name}**");
            _cmdinput.ReleaseUserIfMethod(ctx.User.Id, MethodBase.GetCurrentMethod());

            await _reminderTable.SaveChanges();
        }

        [Command("delete"), Aliases("remove"), Description("Deletes the specified reminder.")]
        [Example("reminder delete Que", "r remove The Game")]
        public async Task DeleteRemindersCommand(CommandContext ctx, [Description("Reminder name."), RemainingText()] string name)
        {
            if (name == null) throw new ArgumentException();

            if (!_reminderTable.RemoveReminder(name))
            {
                await MessageHelper.SendWarningEmbed(ctx, $"No reminder found with that name.");
                return;
            }
            await MessageHelper.SendSuccessEmbed(ctx, $"Reminder successfully removed.\nID: **{name}**");
            await _reminderTable.SaveChanges();
        }

        [Command("list"), Description("Lists all active reminders.")]
        public async Task ListRemindersCommand(CommandContext ctx)
        {
            if (!_cmdinput.IsAvailable(ctx.User.Id))
                return;

            var embedBuilder = new DiscordEmbedBuilder()
                .WithTitle("Reminders")
                .WithColor(0x007FFF)
                .WithThumbnail(FacesHelper.GetIdleFace());

            var remindersList = _reminderTable.GetReminders();
            var guildList = new List<Reminder>();

            foreach (var reminder in remindersList)
                if (reminder.Guild == ctx.Guild.Id)
                    guildList.Add(reminder);

            if (guildList.Count == 0)
                embedBuilder.WithDescription("There are no active reminders.");
            else
                foreach (var reminder in guildList)
                    embedBuilder.AddField(reminder.Name, "Description: " + reminder.Description + "\nHour: **" + reminder.Time + "**");
            
            await ctx.RespondAsync(embed: embedBuilder);
        }
    }
}
