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
        // TODO: better error handling with throw
        private EmbedMessage embedMessage = new EmbedMessage();

        public async Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            if (msg.Severity <= Program.logLevel)
            {
                await SendEmbedMessage(embedMessage.Log($"{msg.Message}", msg.Severity, msg.Source));
            }
        }

        public async Task SendEmbedMessage(EmbedMessage message)
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle(message.title);
            embed.WithColor(message.color);


            if (message.isImage)
            {
                // add image and remove url text (first word)
                embed.WithImageUrl(message.description.Split(" ")[0]);
                if (message.description.Contains(" "))
                {
                    message.description = message.description.Substring(message.description.IndexOf(" ") + 1);
                }
                else
                {
                    message.description = "";
                }
                embed.WithDescription(message.description);
            }
            else
            {
                embed.WithDescription(message.description);
            }
            if (message.logSeverity != new LogSeverity())
            {
                embed.WithTitle($"{message.logSource} {message.logSeverity}");
            }


            if (message.isThereDeleteOption && DataManager.config["reactToDelete"])
            {
                embed.WithFooter($"ᴵˢ ᵗʰᶦˢ ᵐᵉˢˢᵃᵍᵉ ᵃⁿⁿᵒʸᶦⁿᵍˀ ᴿᵉᵃᶜᵗ ᵗᵒ ᵈᵉˡᵉᵗᵉ ᶦᵗᵎ");
                var messageWithDeleteOption = await Program.lastChannel.SendMessageAsync("", false, embed.Build());
                await messageWithDeleteOption.AddReactionAsync(new Emoji(Program.deleteReactionText));
                return;
            }
            if (message.timeUntilDelete > 0)
            {
                embed.WithFooter($"This message will be deleted in {message.timeUntilDelete / 1000} seconds", "https://icons.iconarchive.com/icons/martz90/circle-addon2/256/warning-icon.png");
                var messageToDelete = await Program.lastChannel.SendMessageAsync("", false, embed.Build());
                _ = Task.Delay(message.timeUntilDelete).ContinueWith((t) => messageToDelete.DeleteAsync());
                return;
            }


            Console.WriteLine(new LogMessage(LogSeverity.Info, "wowwowwow", $"sendEmbedMessage ({message.title}: {message.description})").ToString());
            await Program.lastChannel.SendMessageAsync("", false, embed.Build());
        }

        public class EmbedMessage
        {

            public string title { get; set; }
            public string description { get; set; }
            public Color color { get; set; }
            public bool isThereDeleteOption { get; set; }
            public int timeUntilDelete { get; set; }
            public bool isImage { get; set; }
            public LogSeverity logSeverity { get; set; } = new LogSeverity();

            public string logSource { get; set; }

            public EmbedMessage Error(string toBeDescription)
            {
                return new EmbedMessage() { title = "Error", description = toBeDescription, color = Color.Red };
            }
            public EmbedMessage Warning(string toBeDescription)
            {
                return new EmbedMessage() { title = "Warning", description = toBeDescription, color = Color.Orange };
            }
            public EmbedMessage Info(string toBeDescription)
            {
                return new EmbedMessage() { title = "Info", description = toBeDescription, color = Color.Blue };
            }
            public EmbedMessage Log(string toBeDescription, LogSeverity toBeLogSeverity, string toBeLogSource)
            {
                return new EmbedMessage() { title = "", description = toBeDescription, color = Color.Default, timeUntilDelete = 15000, logSeverity = toBeLogSeverity, logSource = toBeLogSource };
            }
            public EmbedMessage KeywordResponse(string toBeDescription, bool toBeImage = false)
            {
                return new EmbedMessage() { title = "", description = toBeDescription, color = Color.LightGrey, isImage = toBeImage, isThereDeleteOption = true };
            }


        }

    }

}