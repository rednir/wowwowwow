using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.IO;
using Discord;
using Discord.WebSocket;

namespace wowwowwow
{
    public class VerboseManager
    {

        private EmbedMessage embedMessage = new EmbedMessage();

        public async Task sendEmbedMessage(EmbedMessage message)
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle(message.title);
            embed.WithDescription(message.description);
            embed.WithColor(message.color);
            await Program.lastChannel.SendMessageAsync($"", false, embed.Build());
        }

        public class EmbedMessage
        {

            public string title { get; set; }
            public string description { get; set; }
            public Color color { get; set; }

            public EmbedMessage Error(string toBeDescription)
            {
                return new EmbedMessage() {title = "Error", description = toBeDescription, color = Color.Red};
            }
            public EmbedMessage Warning(string toBeDescription)
            {
                return new EmbedMessage() {title = "Warning", description = toBeDescription, color = Color.Orange};
            }
            public EmbedMessage Info(string toBeDescription)
            {
                return new EmbedMessage() {title = "Info", description = toBeDescription, color = Color.Blue};
            }
            public EmbedMessage Log(string toBeDescription)
            {
                return new EmbedMessage() {title = "", description = toBeDescription, color = Color.DarkGrey};
            }
            
            
        }

    }

}