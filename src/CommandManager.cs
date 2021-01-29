using System;
using System.Threading;
using System.Threading.Tasks;
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


        private UserCommands.Main mainCommands = new UserCommands.Main();
        private UserCommands.Voice voiceCommands = new UserCommands.Voice();
        private UserCommands.Keyword keywordCommands = new UserCommands.Keyword();
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
                    await mainCommands.Pause(Convert.ToDouble(currentCommand.split[2]));
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


    }
}

