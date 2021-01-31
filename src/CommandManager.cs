using System;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text.Json;
using System.IO;
using Discord;
using Discord.Commands;
using Discord.WebSocket;


namespace wowwowwow
{
    public class CommandManager
    {

        private class Command
        {
            public SocketMessage message { get; set; }
            //public IGuild guild { get; set; }
            public string[] split { get; set; }
            public List<string> parameters { get; set; }
        }

        private class Counting
        {
            public SocketChannel currentCountingChannel;
            public SocketUser lastUser = Program._client.GetUser(Program.botAccountID);
            public int lastNumber = 1;
        }
        private Counting counting;
        public static bool isBotPaused = false;

        private UserCommands.Main mainCommands = new UserCommands.Main();
        private UserCommands.Voice voiceCommands = new UserCommands.Voice();
        private UserCommands.Keyword keywordCommands = new UserCommands.Keyword();
        private UserCommands.Misc miscCommands = new UserCommands.Misc();
        private UserCommands.Config configCommands = new UserCommands.Config();


        private Command currentCommand;
        private VerboseManager verboseManager = new VerboseManager();
        private VerboseManager.EmbedMessage embedMessage = new VerboseManager.EmbedMessage();
        public const string commandIdentifier = "!wow";
        public const string pointerToHelpText = "\nTo view a list of commands, use `!wow help`";

        public async Task Execute(SocketMessage command)
        {
            // remove extra whitespace
            Regex regex = new Regex("\\s+");

            currentCommand = new Command()
            {
                message = command,
                //guild = (command.Channel as SocketGuildChannel).Guild,
                split = regex.Replace(command.Content, " ").Split(" "),
                parameters = new List<string>()
            };

            if (currentCommand.split.Length <= 1)
            {
                await mainCommands.Help();
                return;
            }

            // add all text in quotes to list
            regex = new Regex("\"(.*?)\"", RegexOptions.Singleline);
            var matches = regex.Matches(command.Content);
            foreach (Match match in matches)
            {
                currentCommand.parameters.Add(match.Groups[1].ToString().Trim('\''));
            }

            try
            {
                switch (currentCommand.split[1])
                {
                    case "vc":
                        _ = ExecuteVoice();
                        return;

                    case "keyword":
                        await ExecuteKeyword();
                        return;

                    case "misc":
                        await ExecuteMisc();
                        return;

                    case "config":
                        await ExecuteConfig();
                        return;

                    default:
                        await ExecuteMain();
                        return;
                }
            }
            catch (Exception ex)
            {
                await verboseManager.SendEmbedMessage(embedMessage.Error($"\nCould not execute the command, the following error was returned:```{ex}```"));
            }

        }


        private async Task ExecuteMain()
        {
            switch (currentCommand.split[1])
            {
                case "help": case "commands": case "list":
                    await mainCommands.Help();
                    return;

                case "reload": case "refresh":
                    await mainCommands.Reload();
                    return;

                case "echo":
                    await mainCommands.Echo(currentCommand.message.Content);
                    return;

                case "pause":
                    double pauseTime = await mainCommands.Pause(Convert.ToDouble(currentCommand.split[2]));
                    await Task.Delay(Convert.ToInt32(pauseTime));
                    return;

                default:
                    await verboseManager.SendEmbedMessage(embedMessage.Error($"No such command.{pointerToHelpText}"));
                    return;
            }
        }


        private async Task ExecuteVoice()
        {
            try
            {
                // each guild has its own instance of VoiceCommands
                /*if (!instancesOfVoiceCommands.ContainsKey(currentCommand.guild))
                {
                    instancesOfVoiceCommands.Add(currentCommand.guild, new UserCommands.Voice());
                }
                UserCommands.Voice voiceCommands = instancesOfVoiceCommands[currentCommand.guild];*/
                        
                switch (currentCommand.split[2])
                {

                    case "add": case "play":
                        await voiceCommands.Add(currentCommand.parameters.Count > 0 ? currentCommand.parameters[0] : currentCommand.split[3], currentCommand.message.Author);
                        return;

                    case "leave": case "clear": case "disconnect": case "stop":
                        await voiceCommands.Leave();
                        return;
                    
                    case "queue": case "list":
                        await voiceCommands.Queue();
                        return;

                    case "skip": case "next":
                        await voiceCommands.Skip();
                        return;

                    default:
                        await verboseManager.SendEmbedMessage(embedMessage.Error($"\nNo such command.\n{pointerToHelpText}"));
                        return;

                }
            }
            catch (IndexOutOfRangeException)
            {
                await verboseManager.SendEmbedMessage(embedMessage.Error($"\nA command was specified with a missing option.{pointerToHelpText}"));
            }
            catch (OperationCanceledException)
            {
                // usually if this is thrown (like on Skip()), everything still continues as normal
            }
            catch (Exception ex)
            {
                await verboseManager.SendEmbedMessage(embedMessage.Error($"\nCould not execute the command, the following error was returned:```{ex}```"));
            }

        }


        private async Task ExecuteKeyword()
        {
            try
            {
                switch (currentCommand.split[2])
                {
                    case "add":
                        await keywordCommands.Add(currentCommand.parameters);
                        return;

                    case "remove": case "delete":
                        await keywordCommands.Remove(currentCommand.parameters);
                        return;

                    case "edit": case "change":
                        await keywordCommands.Edit(currentCommand.parameters);
                        return;

                    case "list": case "all":
                        await keywordCommands.List();
                        return;

                    default:
                        await verboseManager.SendEmbedMessage(embedMessage.Error($"\nNo such command.\n{pointerToHelpText}"));
                        return;

                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                await verboseManager.SendEmbedMessage(embedMessage.Error($"Could not execute the command, the following error was returned:```{ex.Message}```Ensure that keywords and values are quoted like \"this\""));
            }
            catch (IndexOutOfRangeException)
            {
                await verboseManager.SendEmbedMessage(embedMessage.Error($"A command was specified with a missing option.{pointerToHelpText}"));
            }

        }

        private async Task ExecuteMisc()
        {
            try 
            {
                switch (currentCommand.split[2])
                {
                    case "count":
                        await miscCommands.Count();
                        counting = new Counting();
                        counting.currentCountingChannel = (currentCommand.message.Channel as SocketChannel);
                        return;

                    case "pfp":
                        await miscCommands.Pfp(currentCommand.message.MentionedUsers, currentCommand.message.Author);
                        return;

                    default:
                        await verboseManager.SendEmbedMessage(embedMessage.Error($"\nNo such command.{pointerToHelpText}"));
                        return;
                }
            }
            catch (IndexOutOfRangeException)
            {
                await verboseManager.SendEmbedMessage(embedMessage.Error($"\nA command was specified with a missing option.{pointerToHelpText}"));
            }
        }

        private async Task ExecuteConfig()
        {
            try
            {
                switch (currentCommand.split[2])
                {
                    case "ignore":
                        await configCommands.Ignore(Enumerable.ElementAt(currentCommand.message.MentionedUsers, 0), currentCommand.split[4]);
                        return;

                    case "react_to_delete":
                        await configCommands.ReactToDelete(currentCommand.split[3]);
                        return;

                    case "quiet_mode":
                        await configCommands.QuietMode(currentCommand.split[3]);
                        return;

                    case "reset":
                        await configCommands.Reset();
                        return;

                    default:
                        await verboseManager.SendEmbedMessage(embedMessage.Error($"\nNo such command.{pointerToHelpText}"));
                        return;

                }
            }
            catch (IndexOutOfRangeException)
            {
                await verboseManager.SendEmbedMessage(embedMessage.Error($"\nA command was specified with a missing option.{pointerToHelpText}"));
            }

        }


        public async Task ReactionAdded(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var message = await cachedMessage.GetOrDownloadAsync();
            if (message.Author.Id == Program.botAccountID && reaction.UserId != Program.botAccountID && reaction.Emote.Name == Program.deleteReactionText)
            {
                Console.WriteLine(new LogMessage(LogSeverity.Info, "wowwowwow", $"ReactionAdded (Delete by: {reaction.User} / {reaction.UserId})").ToString());
                await message.DeleteAsync();
            }
        }

        public async Task MessageRecieved(SocketMessage recievedMessage)
        {
            Console.WriteLine($"[{recievedMessage.Timestamp}] {recievedMessage.Author}: {recievedMessage.Content}");
            if (isBotPaused || DataManager.config["ignore"].Contains(recievedMessage.Author.Id))
            {
                return;
            }

            if (recievedMessage.Content.StartsWith(CommandManager.commandIdentifier))
            {
                VerboseManager.lastChannel = recievedMessage.Channel;
                await CommandRecieved(recievedMessage);
                return;
            }
            else if (counting != null)
            {
                int userNumber;
                if (int.TryParse(recievedMessage.Content, out userNumber) && counting.lastUser != recievedMessage.Author && counting.currentCountingChannel == recievedMessage.Channel)
                {
                    counting.lastUser = recievedMessage.Author;
                    if (userNumber == counting.lastNumber + 1)
                    {
                        await recievedMessage.AddReactionAsync(new Emoji("✅"));
                        counting.lastNumber += 1;
                    }
                    else
                    {
                        await recievedMessage.AddReactionAsync(new Emoji("❎"));
                        await verboseManager.SendEmbedMessage(embedMessage.GenericResponse($"Clearly {recievedMessage.Author.Username} doesn't know how to count...\nType `!wow misc count` to try again.", false, false));
                        counting = null;
                    }
                }

            }

            // only carry on if message is not command
            // todo: make this seperate method
            var foundKeywords = CheckStringForKeyword(recievedMessage.Content);
            if (foundKeywords is null)
            {
                return;
            }

            VerboseManager.lastChannel = recievedMessage.Channel;
            Console.WriteLine($"changed last channel to = {recievedMessage.Channel}");
            if (DataManager.keywords[foundKeywords].StartsWith("http"))
            {
                await verboseManager.SendEmbedMessage(embedMessage.GenericResponse(DataManager.keywords[foundKeywords], true));
                return;
            }

            await verboseManager.SendEmbedMessage(embedMessage.GenericResponse(DataManager.keywords[foundKeywords]));
        }

        private async Task CommandRecieved(SocketMessage command)
        {
            if (command.Content == CommandManager.commandIdentifier)
            {
                await verboseManager.SendEmbedMessage(embedMessage.Info(CommandManager.pointerToHelpText));
                return;
            }
            
            await Execute(command);
        }


        // todo: rewrite
        private dynamic CheckStringForKeyword(string s)
        {

            List<string> listOfKeywords = new List<string>();
            string stringToSearch = s.ToLower().Trim('!', '.', '\"', '?', '\'', '#', ',', ':', '*', '-');
            stringToSearch = new StringBuilder(stringToSearch).Append(" ").Insert(0, " ").ToString();  // this is used to stop errors occuring when checking whether the keyword found is part of another word

            foreach (var k in DataManager.keywords.Keys)
            {
                string keyword = k.ToLower();
                if (stringToSearch.Contains(keyword))
                {
                    // prioritize exact matches
                    if (keyword == stringToSearch)
                    {
                        listOfKeywords.Add(keyword);
                        listOfKeywords.RemoveAll((x) => x != keyword);
                        return listOfKeywords.Max();

                    }
                    else if (stringToSearch.Contains(keyword))
                    {
                        // check if the keyword found is not part of another word (check if whitespace in front and behind)
                        if (char.IsWhiteSpace(stringToSearch[stringToSearch.IndexOf(keyword) + keyword.Length]) && char.IsWhiteSpace(stringToSearch[stringToSearch.IndexOf(keyword) - 1]))
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

