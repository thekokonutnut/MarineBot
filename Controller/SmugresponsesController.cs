using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using MarineBot.Database;
using MarineBot.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MarineBot.Controller
{
    internal class SmugresponsesController
    {
        Dictionary<ulong, bool> channelTracked;
        Dictionary<ulong, bool> guildTracked;
        private SmugresponsesTable _responsesTable;

        public SmugresponsesController(DatabaseController controller)
        {
            channelTracked = new Dictionary<ulong, bool>();
            guildTracked = new Dictionary<ulong, bool>();
            _responsesTable = controller.GetTable<SmugresponsesTable>();
        }

        public bool IsEnabledChannelResponses(ulong channelId)
        {
            if (channelTracked.TryGetValue(channelId, out var isEnabled))
            {
                return isEnabled;
            }

            return false;
        }

        public void SetEnabledChannelResponses(ulong channelId, bool value)
        {
            channelTracked[channelId] = value;
        }

        public void ToggleEnabledChannelResponses(ulong channelId)
        {
            if (channelTracked.TryGetValue(channelId, out var isEnabled))
                channelTracked[channelId] = !isEnabled;
            else
                channelTracked[channelId] = true;
        }

        public bool IsEnabledGuildResponses(ulong guildId)
        {
            if (guildTracked.TryGetValue(guildId, out var isEnabled))
            {
                return isEnabled;
            }

            return false;
        }

        public void SetEnabledGuildResponses(ulong guildId, bool value)
        {
            guildTracked[guildId] = value;
        }

        public void ToggleEnabledGuildResponses(ulong guildId)
        {
            if (guildTracked.TryGetValue(guildId, out var isEnabled))
                guildTracked[guildId] = !isEnabled;
            else
                guildTracked[guildId] = true;
        }

        public async Task<SmugresponseEntity> HandleChannelResponse(DiscordClient cl, DiscordMessage msg)
        {
            var responses = _responsesTable.GetEntries();
            var answers = new List<SmugresponseEntity>();
            foreach (var item in responses)
            {
                // TODO: set case sensitive as an option

                // whole message
                if (item.Type == 1 && msg.Content.ToLower() == item.Query.ToLower())
                {
                    answers.Add(item);
                }
                // partial message
                else if (item.Type == 2 && msg.Content.ToLower().Contains(item.Query.ToLower()))
                {
                    answers.Add(item);
                }
                // regex
                else if (item.Type == 3)
                {
                    try
                    {
                        var match = Regex.Match(msg.Content, item.Query, RegexOptions.IgnoreCase);

                        if (match.Success)
                        {
                            string text;
                            text = item.Answer;

                            for (var i = 1; i < match.Groups.Count; i++)
                            {
                                text = text.Replace("$" + i, match.Groups[i].ToString());
                            }

                            item.ModAnswer = text;
                            answers.Add(item);
                        }
                    }
                    catch (ArgumentException ex)
                    {
                        cl.Logger.Log(LogLevel.Error, $"Error matching regular expression for response ID {item.ID}: {ex.Message}");
                    }
                }
            }

            if (answers.Count != 0)
            {
                int randomIndex = new Random().Next(0, answers.Count);
                var selected = answers[randomIndex];

                if (selected.Answer == "random")
                {
                    var resp_list = responses.ToArray();
                    randomIndex = new Random().Next(0, resp_list.Length);
                    selected = resp_list[randomIndex];
                    if (selected.Type == 3)
                    {
                        string text;
                        text = selected.Answer;

                        for (var i = 1; i < 10; i++)
                        {
                            text = text.Replace("$" + i, msg.Content);
                        }

                        selected.ModAnswer = text;
                        await cl.SendMessageAsync(msg.Channel, selected.ModAnswer);
                    }
                    else
                        await cl.SendMessageAsync(msg.Channel, selected.Answer);
                    return selected;
                }

                if (selected.Type == 3)
                    await cl.SendMessageAsync(msg.Channel, selected.ModAnswer);
                else
                    await cl.SendMessageAsync(msg.Channel, selected.Answer);
                return selected;
            }
            return null;
        }
    }
}
