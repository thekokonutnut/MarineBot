using MarineBot.Helpers;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Threading.Tasks;
using DSharpPlus;
using CodingSeb.ExpressionEvaluator;
using MarineBot.Attributes;

namespace MarineBot.Commands
{
    [Group("Utils"),Aliases("u")]
    [Description("Commands of varied utility.")]
    [ShortCommandsGroup]
    [RequireGuild]
    internal class UtilsCommands : BaseCommandModule
    {
        [GroupCommand(), Hidden()]
        public async Task MainCommand(CommandContext ctx)
        {
            var cmds = ctx.CommandsNext;
            var context = cmds.CreateContext(ctx.Message, ctx.Prefix, cmds.FindCommand("help", out _), ctx.Command.QualifiedName);
            await cmds.ExecuteCommandAsync(context);
        }

        [Command("roll"), Description("Roll a random number.")]
        [Example("utils roll 30", "u roll 100 60")]
        public async Task RollCommand(CommandContext ctx, [Description("Maximum number.")] int max, [Description("Minimum number.")] int min = 0)
        {
            try
            {
                int rand = NumbersHelper.GetRandom(min, max);
                await MessageHelper.SendSuccessEmbed(ctx, $"`Result:` {rand}");
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
            }
        }

        [Command("choose"), Description("Choose one of the options provided.")]
        [Example("utils choose Yes No", "u choose \"Puede ser\" \"No creo\" \"Xd!\"")]
        public async Task ChooseCommand(CommandContext ctx, [Description("Options."), RemainingText()] params string[] options)
        {
            if (options == null) throw new ArgumentException();
            try
            {
                if (options.Length < 2) {
                    await MessageHelper.SendWarningEmbed(ctx, "I need at least two options.");
                    return;
                }

                if (NumbersHelper.GetRandom(0, 100) > 95)
                {
                    await MessageHelper.SendWarningEmbed(ctx, "`I can't decide...`");
                    return;
                }

                int chosen = NumbersHelper.GetRandom(0, options.Length - 1);
                await MessageHelper.SendSuccessEmbed(ctx, $"`Result:` {options[chosen]}");
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
            }
        }

        [Command("question"), Description("Clear your doubts.")]
        [Example("utils question Is snessy gay?", "u question Are you sure?")]
        public async Task DudaCommand(CommandContext ctx, [Description("Question"), RemainingText()] string question)
        {
            if (question == null) throw new ArgumentException();
            try
            {
                await MessageHelper.SendSuccessEmbed(ctx, $"`{question}:` {QuotesHelper.GetDudaAnswer()}");
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
            }
        }

        [Command("purge"), Description("Deletes the specified number of messages.")]
        [Example("utils purge 10")]
        [RequireUserPermissions(Permissions.ManageMessages), RequireBotPermissions(Permissions.ManageMessages)]    
        public async Task PurgeCommand(CommandContext ctx, [Description("Number of messages")] uint amount)
        {
            try
            {
                var messages = await ctx.Channel.GetMessagesAsync((int)amount + 1);

                await ctx.Channel.DeleteMessagesAsync(messages);
                var m = MessageHelper.SendSuccessEmbed(ctx, $"Messages deleted successfully.");
                await Task.Delay(2000);
                await m.Result.DeleteAsync();
            }
            catch (Exception e)
            {
                await MessageHelper.SendErrorEmbed(ctx, e.Message);
            }
        }
    }
}