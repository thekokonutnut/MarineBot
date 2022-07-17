using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Lavalink;
using MarineBot.Attributes;
using MarineBot.Helpers;

namespace MarineBot.Commands
{
    [Group("Music"),Aliases("m")]
    [Description("Music-related commands.")]
    [ShortCommandsGroup]
    [RequireGuild]
    internal class MusicCommands : BaseCommandModule
    {
        [GroupCommand(), Hidden()]
        public async Task MainCommand(CommandContext ctx)
        {
            var cmds = ctx.CommandsNext;
            var context = cmds.CreateContext(ctx.Message, ctx.Prefix, cmds.FindCommand("help", out _), ctx.Command.QualifiedName);
            await cmds.ExecuteCommandAsync(context);
        }
        
        [Command("join"), Description("Join the bot to a voice channel.")]
        public async Task JoinCommand(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)  
            {
                await MessageHelper.SendErrorEmbed(ctx, "You are not in a voice channel.");
                return;
            }
            
            var channel = ctx.Member.VoiceState.Channel;

            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await MessageHelper.SendErrorEmbed(ctx, "The Lavalink connection is not established");
                return;
            }

            var node = lava.ConnectedNodes.Values.First();

            if (channel.Type != ChannelType.Voice)
            {
                await MessageHelper.SendErrorEmbed(ctx, "Not a valid voice channel.");
                return;
            }

            await node.ConnectAsync(channel);
            await MessageHelper.SendSuccessEmbed(ctx, $"Joined {channel.Name}!");
        }

        [Command("leave"), Description("Removes the bot from a voice channel.")]
        public async Task LeaveCommand(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)  
            {
                await MessageHelper.SendErrorEmbed(ctx, "You are not in a voice channel.");
                return;
            }
            
            var channel = ctx.Member.VoiceState.Channel;

            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await MessageHelper.SendErrorEmbed(ctx, "The Lavalink connection is not established");
                return;
            }

            var node = lava.ConnectedNodes.Values.First();

            if (channel.Type != ChannelType.Voice)
            {
                await MessageHelper.SendErrorEmbed(ctx, "Not a valid voice channel.");
                return;
            }

            var conn = node.GetGuildConnection(channel.Guild);

            if (conn == null)
            {
                await MessageHelper.SendErrorEmbed(ctx, "Lavalink is not connected.");
                return;
            }

            await conn.DisconnectAsync();
            await MessageHelper.SendSuccessEmbed(ctx, $"Left {channel.Name}!");
        }

        [Command("play"), Description("Plays something in a voice channel.")]
        public async Task PlayCommand(CommandContext ctx, [Description("Query to search"), RemainingText] string search)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await MessageHelper.SendErrorEmbed(ctx, "You are not in a voice channel.");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await MessageHelper.SendErrorEmbed(ctx, "Lavalink is not connected.");
                return;
            }

            await ctx.TriggerTypingAsync();

            var loadResult = await node.Rest.GetTracksAsync(search);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed 
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await MessageHelper.SendErrorEmbed(ctx, $"Track search failed for {search}.");
                return;
            }

            var track = loadResult.Tracks.First();

            await conn.PlayAsync(track);

            await MessageHelper.SendSuccessEmbed(ctx, $"Now playing {track.Title}!");
        }

        [Command("stop"), Description("Stop the music in a voice channel.")]
        public async Task StopCommand(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await MessageHelper.SendErrorEmbed(ctx, "You are not in a voice channel.");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await MessageHelper.SendErrorEmbed(ctx, "Lavalink is not connected.");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await MessageHelper.SendErrorEmbed(ctx, "There are no tracks loaded.");
                return;
            }

            await conn.StopAsync();
            await MessageHelper.SendInfoEmbed(ctx, "Stopped playing music.");
        }
    }
}