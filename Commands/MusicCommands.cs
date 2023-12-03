using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using MarineBot.Attributes;
using MarineBot.Controller;
using MarineBot.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace MarineBot.Commands
{
    [Group("Music"),Aliases("m")]
    [Description("Music-related commands.")]
    [ShortCommandsGroup]
    [RequireGuild]
    internal class MusicCommands : BaseCommandModule
    {
        private MusicQueueController _queuecontrol;

        public MusicCommands(IServiceProvider serviceProvider)
        {
            _queuecontrol = serviceProvider.GetService<MusicQueueController>();
        }
        
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

            var conn = node.GetGuildConnection(channel.Guild);

            if (conn != null)
            {
                await MessageHelper.SendErrorEmbed(ctx, "Bot is already in a voice channel, use music leave first!");
                return;
            }

            conn = await node.ConnectAsync(channel);
            await MessageHelper.SendSuccessEmbed(ctx, $"Joined {channel.Name}!");

            _queuecontrol.StartQueueSession(channel.Id, conn);
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

            _queuecontrol.DestroyQueueSession(conn.Channel.Id);

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

            if (conn.CurrentState.CurrentTrack == null)
            {
                await conn.PlayAsync(track);
                await MessageHelper.SendSuccessEmbed(ctx, $"Now playing: {track.Title}!");
            }
            else
            {
                int pos = _queuecontrol.AddToQueueSession(conn.Channel.Id, new QueueTrack(track, ctx.Member, ctx.Channel));
                await MessageHelper.SendSuccessEmbed(ctx, $"Added track to queue: {track.Title}!\nPosition: {pos+1}");
            }
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

            _queuecontrol.ClearQueueSession(conn.Channel.Id);
            await conn.StopAsync();
            await MessageHelper.SendInfoEmbed(ctx, "Stopped playing music.");
        }

        [Command("skip"), Description("Skip current song in queue.")]
        public async Task SkipCommand(CommandContext ctx)
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
        }

        [Command("trackinfo"), Description("Display current track information.")]
        public async Task TrackInfoCommand(CommandContext ctx)
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

            var track = conn.CurrentState.CurrentTrack;

            await MessageHelper.SendInfoEmbed(ctx, $"**Track title:** {track.Title}\n**Author:** {track.Author}\n**Duration:** {track.Length.ToString("h'h 'm'm 's's'")}");
        }

        [Command("queue"), Description("Shows the queue.")]
        public async Task QueueCommand(CommandContext ctx)
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

            var sess = _queuecontrol.GetQueueSessionList(conn.Channel.Id);
            if (sess == null || sess.Count == 0)
            {
                await MessageHelper.SendErrorEmbed(ctx, "There is no queue on this session.");
                return;
            }

            var sb = new StringBuilder();
            for (int i = 0; i < sess.Count; i++)
            {
                sb.AppendLine($"**{i+1}**: {sess[i].track.Title} **[{sess[i].addedBy.Username}]**");
            }

            var embed = new DiscordEmbedBuilder()
                .WithColor(0xeaeaea)
                .WithDescription(sb.ToString())
                .WithTitle(":musical_note: Track queue")
                .WithThumbnail(FacesHelper.GetIdleFace());

            await ctx.RespondAsync(embed: embed);
        }
    }
}