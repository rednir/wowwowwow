using System;
using System.Text;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using Discord;
using Discord.WebSocket;
using Discord.Commands;

namespace wowwowwow
{
    public class Program
    {

        public static DiscordSocketClient _client;

        public const long botAccountID = 802277381129764865;

        public const string deleteReactionText = "\uD83D\uDDD1\uFE0F";

        public static bool isBotPaused = false;

        private DataManager dataManager = new DataManager();
        private CommandManager commandManager = new CommandManager();
        private VerboseManager verboseManager = new VerboseManager();
        private VerboseManager.EmbedMessage embedMessage = new VerboseManager.EmbedMessage();

        public const LogSeverity logLevel = LogSeverity.Error;


        public static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }


        private async Task MainAsync()
        {
            var _config = new DiscordSocketConfig { MessageCacheSize = 100 };
            var token = File.ReadAllText("token.txt");
            _client = new DiscordSocketClient();

            _client.Log += verboseManager.Log;
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await dataManager.LoadData();
            _client.MessageReceived += MessageRecieved;
            _client.ReactionAdded += ReactionAdded;
            _client.Ready += Ready;



            await Task.Delay(-1);
        }


        private async Task Ready()
        {
            await _client.SetGameAsync("!wow help", null, ActivityType.Playing);
        }


        private async Task ReactionAdded(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var message = await cachedMessage.GetOrDownloadAsync();
            if (message.Author.Id == botAccountID && reaction.UserId != botAccountID && reaction.Emote.Name == deleteReactionText)
            {
                Console.WriteLine(new LogMessage(LogSeverity.Info, "wowwowwow", $"ReactionAdded (Delete by: {reaction.User})").ToString());
                await message.DeleteAsync();
            }
        }

        private async Task MessageRecieved(SocketMessage recievedMessage)
        {
            Console.WriteLine($"[{recievedMessage.Timestamp}] {recievedMessage.Author}: {recievedMessage.Content}");
            if (isBotPaused || DataManager.config["ignore"].Contains(recievedMessage.Author.Id))
            {
                return;
            }

            if (recievedMessage.Content.StartsWith(CommandManager.commandIdentifier))
            {
                VerboseManager.lastChannel = recievedMessage.Channel;
                if (recievedMessage.Content == CommandManager.commandIdentifier)
                {
                    await verboseManager.SendEmbedMessage(embedMessage.Info(CommandManager.pointerToHelpText));
                    return;
                }
                await commandManager.Execute(recievedMessage);
                return;
            }

            // only carry on if message is not command
            // todo: make this seperate method
            var foundKeywords = CheckStringForKeyword(recievedMessage.Content);
            if (DataManager.keywords.Count == 0)
            {
                return;
            }
            VerboseManager.lastChannel = recievedMessage.Channel;

            if (DataManager.keywords[foundKeywords].StartsWith("http"))
            {
                await verboseManager.SendEmbedMessage(embedMessage.KeywordResponse(DataManager.keywords[foundKeywords], true));
                return;
            }

            await verboseManager.SendEmbedMessage(embedMessage.KeywordResponse(DataManager.keywords[foundKeywords]));
        }




        // todo: rewrite
        private dynamic CheckStringForKeyword(string s)
        {
            List<string> listOfKeywords = new List<string>();
            string stringToSearch = s.ToLower().Trim('!', '.', '\"', '?', '\'', '#', ',', ':', '*', '-');

            foreach (var k in DataManager.keywords.Keys)
            {
                string keyword = k.ToLower();
                if (stringToSearch.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    // prioritize exact matches
                    if (keyword == stringToSearch)
                    {
                        listOfKeywords.Add(keyword);
                        listOfKeywords.RemoveAll((x) => x != keyword);
                        break;
                    }
                    else if (stringToSearch.Contains(keyword))
                    {
                        // check if the keyword is not part of another word (check if whitespace in front and behind)
                        try
                        {
                            if (stringToSearch[stringToSearch.IndexOf(keyword) + keyword.Length] == ' ' && stringToSearch[stringToSearch.IndexOf(keyword) - 1] == ' ')
                            {
                                listOfKeywords.Add(keyword);
                            }
                        }
                        catch (IndexOutOfRangeException)
                        {
                            listOfKeywords.Add(keyword);
                        }
                    }
                }
            }

            return listOfKeywords.Max();

        }


    }
}
