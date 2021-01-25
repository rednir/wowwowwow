using System;
using System.Text;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using Discord;
using Discord.WebSocket;

namespace wowwowwow
{
    public class Program
    {

        public DiscordSocketClient _client;

        public const long botAccountID = 802277381129764865;

        public const string deleteReactionText = "\uD83D\uDDD1\uFE0F";

        public static bool isBotPaused = false;

        private CommandManager commandManager = new CommandManager();
        private VerboseManager verboseManager = new VerboseManager();
        private VerboseManager.EmbedMessage embedMessage = new VerboseManager.EmbedMessage();

        public const LogSeverity logLevel = LogSeverity.Debug;

        public static ISocketMessageChannel lastChannel;


        public static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }


        private async Task MainAsync()
        {
            var _config = new DiscordSocketConfig { MessageCacheSize = 100 };
            var token = File.ReadAllText("token.txt");
            _client = new DiscordSocketClient();

            _client.Log += commandManager.Log;
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await commandManager.LoadKeywords();
            _client.MessageReceived += MessageRecieved;
            _client.ReactionAdded += ReactionAdded;

            await Task.Delay(-1);
        }


        private async Task ReactionAdded(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            //verboseManager.sendEmbedMessage(embedMessage.Info(reaction.Emote.Name.ToString()));
            var message = await cachedMessage.GetOrDownloadAsync();
            if (message.Author.Id == botAccountID && reaction.UserId != botAccountID && reaction.Emote.Name == deleteReactionText)
            {
                await message.DeleteAsync();
            }
        }


        private async Task MessageRecieved(SocketMessage recievedMessage)
        {
            if (isBotPaused || recievedMessage.Author.Id == botAccountID)
            {
                return;
            }
            lastChannel = recievedMessage.Channel;
            if (recievedMessage.Content.StartsWith(CommandManager.commandIdentifier))
            {
                await commandManager.Execute(recievedMessage.Content);
                return;
            }

            // only carry on if message is not command
            var foundKeywords = CheckStringForKeyword(recievedMessage.Content);
            try
            {
                if (CommandManager.keywords[foundKeywords].StartsWith("http"))
                {
                    await verboseManager.sendEmbedMessage(embedMessage.KeywordResponse(CommandManager.keywords[foundKeywords], true));
                    return;
                }
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine(recievedMessage);
                return;
            }
            Console.WriteLine(foundKeywords);
            await verboseManager.sendEmbedMessage(embedMessage.KeywordResponse(CommandManager.keywords[foundKeywords]));
        }




        // todo: rewrite
        private dynamic CheckStringForKeyword(string s)
        {
            List<string> listOfKeywords = new List<string>();
            string stringToSearch = s.ToLower().Trim('!', '.', '\"', '?', '\'', '#', ',', ':', '*', '-');

            foreach (var k in CommandManager.keywords.Keys)
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
