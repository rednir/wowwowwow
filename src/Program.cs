﻿using System;
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

        private CommandManager commandManager = new CommandManager();

        public const LogSeverity logLevel = LogSeverity.Debug;

        public static IMessageChannel lastChannel;
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

            await Task.Delay(-1);
        }


        private async Task MessageRecieved(SocketMessage recievedMessage)
        {
            if (recievedMessage.Author.Id == botAccountID) // tincan
            {
                return;
            }
            lastChannel = recievedMessage.Channel;
            if (recievedMessage.Content.StartsWith(CommandManager.commandIdentifier))
            {
                await commandManager.Execute(recievedMessage.Content);
                return;
            }
            var foundKeywords = CheckStringForKeyword(recievedMessage.Content);
            if (foundKeywords != null)
            {
                Console.WriteLine(foundKeywords);
                await lastChannel.SendMessageAsync(CommandManager.keywords[foundKeywords]);
            }

        }


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