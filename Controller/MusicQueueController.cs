using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using MarineBot.Helpers;

namespace MarineBot.Controller
{
    class QueueTrack 
    {
        public LavalinkTrack track;
        public DiscordMember addedBy;
        public DiscordChannel broadcast;

        public QueueTrack(LavalinkTrack _track, DiscordMember _member, DiscordChannel _channel)
        {
            track = _track;
            addedBy = _member;
            broadcast = _channel;
        }
    }

    internal class MusicQueueController
    {
        Dictionary<ulong, List<QueueTrack>> connectionQueue;

        public MusicQueueController()
        {
            connectionQueue = new Dictionary<ulong, List<QueueTrack>>();
        }

        public void StartQueueSession(ulong channel, LavalinkGuildConnection connection)
        {
            if (connectionQueue.ContainsKey(channel)) 
                return;
            
            connectionQueue.Add(channel, new List<QueueTrack>());
            connection.PlaybackFinished += HandlePlaybackFinished;
        }

        public bool DestroyQueueSession(ulong channel)
        {
            if (connectionQueue.ContainsKey(channel))
                return connectionQueue.Remove(channel);
            return false;
        }

        public void ClearQueueSession(ulong channel)
        {
            if (connectionQueue.TryGetValue(channel, out var sess))
                sess.Clear();
        }

        public int AddToQueueSession(ulong channel, QueueTrack track)
        {
            if (connectionQueue.TryGetValue(channel, out var sess))
            {
                sess.Add(track);
                return sess.IndexOf(track);
            }
            return -1;
        }

        public List<QueueTrack> GetQueueSessionList(ulong channel)
        {
            if (connectionQueue.TryGetValue(channel, out var sess))
                return sess;
            return null;
        }

        public QueueTrack GetNextInQueueSession(ulong channel)
        {
            if (connectionQueue.TryGetValue(channel, out var sess))
            {
                if (sess.Count == 0)
                    return null;

                var track = sess.First();
                sess.RemoveAt(0);
                return track;
            }
            return null;
        }

        private async Task HandlePlaybackFinished(LavalinkGuildConnection connection, TrackFinishEventArgs args)
        {
            Console.WriteLine($"HandlePlaybackFinished: {connection.Channel.Id}");

            var chan = connection.Channel.Id;

            if (connection == null || !connection.IsConnected || !connectionQueue.ContainsKey(chan))
            {
                connection.PlaybackFinished -= HandlePlaybackFinished;
                return;
            }

            Console.WriteLine("Obtaining next track");
            var nextTrack = GetNextInQueueSession(chan);
            
            if (nextTrack != null)
            {
                Console.WriteLine($"Next track: {nextTrack.track.Title}");
                var discordchan = nextTrack.broadcast;

                var embed = MessageHelper.SuccessEmbed($"Playing next in queue: {nextTrack.track.Title}!\nAdded by {nextTrack.addedBy.Username}");
                await discordchan.SendMessageAsync(embed);

                await connection.PlayAsync(nextTrack.track);
            }
        }
    }
}
