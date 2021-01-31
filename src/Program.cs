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


        private DataManager dataManager = new DataManager();
        private Dictionary<IGuild, CommandManager> instancesOfCommandManager = new Dictionary<IGuild, CommandManager>();
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

        private async Task MessageRecieved(SocketMessage recievedMessage)
        {
            IGuild guildOfMessage = (recievedMessage.Channel as SocketGuildChannel).Guild;
            if (!instancesOfCommandManager.ContainsKey(guildOfMessage))
            {
                // create a new instance for a new guild
                instancesOfCommandManager.Add(guildOfMessage, new CommandManager());
            }
            await instancesOfCommandManager[guildOfMessage].MessageRecieved(recievedMessage);
        }

        private async Task ReactionAdded(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            IGuild guildOfReaction = (channel as SocketGuildChannel).Guild;
            if (!instancesOfCommandManager.ContainsKey(guildOfReaction))
            {
                // create a new instance for a new guild
                instancesOfCommandManager.Add(guildOfReaction, new CommandManager());
            }
            await instancesOfCommandManager[guildOfReaction].ReactionAdded(cachedMessage, channel, reaction);
        }


        private async Task Ready()
        {
            await _client.SetGameAsync("!wow help", null, ActivityType.Playing);
        }


    }
}
