using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Lavalink4NET;
using Lavalink4NET.DSharpPlus;
using Lavalink4NET.Extensions;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Rest.Entities.Tracks;
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
        private IAudioService _audioService;

        public MusicCommands(IServiceProvider serviceProvider)
        {
            _audioService = serviceProvider.GetService<IAudioService>();
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

            if (channel.Type != ChannelType.Voice)
            {
                await MessageHelper.SendErrorEmbed(ctx, "Not a valid voice channel.");
                return;
            }

            var hasplayer = _audioService.Players.HasPlayer(ctx.Guild.Id);

            if (hasplayer)
            {
                await MessageHelper.SendErrorEmbed(ctx, "Bot is already in a voice channel, use music leave first!");
                return;
            }

            var player = await _audioService.Players.JoinAsync(ctx.Guild.Id, channel.Id, PlayerFactory.Queued);

            if (player != null)
            {
                await MessageHelper.SendSuccessEmbed(ctx, $"Joined {channel.Name}!");
            }
            else
            {
                await MessageHelper.SendErrorEmbed(ctx, "Could not create the audio player!");
            }
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

            /*var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await MessageHelper.SendErrorEmbed(ctx, "The Lavalink connection is not established");
                return;
            }*/

            if (channel.Type != ChannelType.Voice)
            {
                await MessageHelper.SendErrorEmbed(ctx, "Not a valid voice channel.");
                return;
            }

            var player = await _audioService.Players.GetPlayerAsync(ctx.Guild.Id);

            if (player == null)
            {
                await MessageHelper.SendErrorEmbed(ctx, "Bot is not in a voice channel.");
                return;
            }

            await player.DisconnectAsync();
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

            //var lava = ctx.Client.GetLavalink();
            //var node = lava.ConnectedNodes.Values.First();
            //var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            var player = await _audioService.Players.GetPlayerAsync<QueuedLavalinkPlayer>(ctx.Guild.Id);

            if (player == null)
            {
                await MessageHelper.SendErrorEmbed(ctx, "Lavalink is not connected.");
                return;
            }

            await ctx.TriggerTypingAsync();

            var track = await _audioService.Tracks
                .LoadTrackAsync(search, TrackSearchMode.YouTube);

            if (track is null)
            {
                await MessageHelper.SendErrorEmbed(ctx, $"Track search failed for {search}.");
                return;
            }

            var position = await player.PlayAsync(track);

            if (position is 0)
            {
                await MessageHelper.SendSuccessEmbed(ctx, $"Now playing: {track.Title}!");
            }
            else
            {
                await MessageHelper.SendSuccessEmbed(ctx, $"Added track to queue: {track.Title}!\nPosition: {position}");
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

            //var lava = ctx.Client.GetLavalink();
            //var node = lava.ConnectedNodes.Values.First();
            //var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            var player = await _audioService.Players.GetPlayerAsync<QueuedLavalinkPlayer>(ctx.Guild.Id);

            if (player == null)
            {
                await MessageHelper.SendErrorEmbed(ctx, "Lavalink is not connected.");
                return;
            }

            if (player.CurrentItem is null)
            {
                await MessageHelper.SendErrorEmbed(ctx, "There are no tracks loaded.");
                return;
            }

            await player.StopAsync();
            await MessageHelper.SendInfoEmbed(ctx, "Stopped playing music.");
        }

        [Command("pause"), Description("Pause the music in a voice channel.")]
        public async Task PauseCommand(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await MessageHelper.SendErrorEmbed(ctx, "You are not in a voice channel.");
                return;
            }

            //var lava = ctx.Client.GetLavalink();
            //var node = lava.ConnectedNodes.Values.First();
            //var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            var player = await _audioService.Players.GetPlayerAsync<QueuedLavalinkPlayer>(ctx.Guild.Id);

            if (player == null)
            {
                await MessageHelper.SendErrorEmbed(ctx, "Lavalink is not connected.");
                return;
            }

            if (player.CurrentItem is null)
            {
                await MessageHelper.SendErrorEmbed(ctx, "There are no tracks loaded.");
                return;
            }

            if (player.IsPaused)
            {
                await MessageHelper.SendWarningEmbed(ctx, "Audio is already paused.");
                return;
            }

            await player.PauseAsync();
            await MessageHelper.SendInfoEmbed(ctx, "Paused music.");
        }

        [Command("resume"), Description("Resume the music in a voice channel.")]
        public async Task ResumeCommand(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await MessageHelper.SendErrorEmbed(ctx, "You are not in a voice channel.");
                return;
            }

            //var lava = ctx.Client.GetLavalink();
            //var node = lava.ConnectedNodes.Values.First();
            //var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            var player = await _audioService.Players.GetPlayerAsync<QueuedLavalinkPlayer>(ctx.Guild.Id);

            if (player == null)
            {
                await MessageHelper.SendErrorEmbed(ctx, "Lavalink is not connected.");
                return;
            }

            if (player.CurrentItem is null)
            {
                await MessageHelper.SendErrorEmbed(ctx, "There are no tracks loaded.");
                return;
            }

            if (!player.IsPaused)
            {
                await MessageHelper.SendWarningEmbed(ctx, "Audio is not paused.");
                return;
            }

            await player.ResumeAsync();
            await MessageHelper.SendInfoEmbed(ctx, "Resumed playing music.");
        }

        [Command("skip"), Description("Skip current song in queue.")]
        public async Task SkipCommand(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await MessageHelper.SendErrorEmbed(ctx, "You are not in a voice channel.");
                return;
            }

            var player = await _audioService.Players.GetPlayerAsync<QueuedLavalinkPlayer>(ctx.Guild.Id);

            if (player == null)
            {
                await MessageHelper.SendErrorEmbed(ctx, "Lavalink is not connected.");
                return;
            }

            if (player.CurrentItem is null)
            {
                await MessageHelper.SendErrorEmbed(ctx, "There are no tracks loaded.");
                return;
            }

            await player.SkipAsync();

            var track = player.CurrentItem;

            if (track is not null)
            {
                await MessageHelper.SendInfoEmbed(ctx, $"Skipped. Now playing: {track.Track!.Uri}");
            }
            else
            {
                await MessageHelper.SendInfoEmbed(ctx, "Skipped. Stopped playing because the queue is now empty.");
            }
        }

        [Command("trackinfo"), Description("Display current track information.")]
        public async Task TrackInfoCommand(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await MessageHelper.SendErrorEmbed(ctx, "You are not in a voice channel.");
                return;
            }

            var player = await _audioService.Players.GetPlayerAsync<QueuedLavalinkPlayer>(ctx.Guild.Id);

            if (player == null)
            {
                await MessageHelper.SendErrorEmbed(ctx, "Lavalink is not connected.");
                return;
            }

            if (player.CurrentItem is null)
            {
                await MessageHelper.SendErrorEmbed(ctx, "There are no tracks loaded.");
                return;
            }

            var track = player.CurrentItem;

            await MessageHelper.SendInfoEmbed(ctx, $"**Track title:** {track.Track!.Title}\n**Author:** {track.Track!.Author}\n**Duration:** {track.Track!.Duration.ToString("h'h 'm'm 's's'")}");
        }

        [Command("queue"), Description("Shows the queue.")]
        public async Task QueueCommand(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await MessageHelper.SendErrorEmbed(ctx, "You are not in a voice channel.");
                return;
            }

            var player = await _audioService.Players.GetPlayerAsync<QueuedLavalinkPlayer>(ctx.Guild.Id);

            if (player == null)
            {
                await MessageHelper.SendErrorEmbed(ctx, "Lavalink is not connected.");
                return;
            }

            if (player.CurrentItem is null)
            {
                await MessageHelper.SendErrorEmbed(ctx, "There are no tracks loaded.");
                return;
            }

            var sb = new StringBuilder();
            for (int i = 0; i < player.Queue.Count; i++)
            {
                sb.AppendLine($"**{i+1}**: {player.Queue[i].Track!.Title}");
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