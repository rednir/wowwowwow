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
                case "help":
                    await mainCommands.Help();
                    return;

                case "reload":
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
                switch (currentCommand.split[2])
                {
                    case "join":
                        await voiceCommands.Join((currentCommand.message.Author as IVoiceState).VoiceChannel);
                        return;
                    
                    case "leave":
                        await voiceCommands.Leave();
                        return;

                    case "add":
                        // after error handling rework, change VoiceCommands.Join to throw error instead of return false
                        if (await voiceCommands.Join((currentCommand.message.Author as IVoiceState).VoiceChannel))
                        {
                            // use quoted parameters if exists
                            await voiceCommands.Add(currentCommand.parameters.Count > 0 ? currentCommand.parameters[0] : currentCommand.split[3]);
                        }
                        return;

                    case "skip":
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

                    case "remove":
                        await keywordCommands.Remove(currentCommand.parameters);
                        return;

                    case "edit":
                        await keywordCommands.Edit(currentCommand.parameters);
                        return;

                    case "list":
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

