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
    public class CommandManager
    {

        private class Command
        {
            public SocketMessage message { get; set; }
            public string[] split { get; set; }
            public List<string> parameters { get; set; }
        }


        private UserCommands.Main mainCommands = new UserCommands.Main();
        private UserCommands.Keyword keywordCommands = new UserCommands.Keyword();
        private UserCommands.Config configCommands = new UserCommands.Config();

        private Command currentCommand;
        private VerboseManager verboseManager = new VerboseManager();
        private VerboseManager.EmbedMessage embedMessage = new VerboseManager.EmbedMessage();
        public const string commandIdentifier = "!wow";
        public const string pointerToHelpText = "\nTo view a list of commands, use `!wow help`";


        public string helpText = string.Join(Environment.NewLine,
        new string[]
        {
            "Main commands:",
            " - `!wow help`",
            " - `!wow reload`",
            " - `!wow echo`",
            " - `!wow pause <minutes>`",

            "\nKeyword commands:",
            " - `!wow keyword list`",
            " - `!wow keyword add \"<keyword>\" \"<optional:image>\" \"<value>\"`",
            " - `!wow keyword remove \"<keyword>\"`",
            " - `!wow keyword edit \"<keyword>\" \"<optional:image>\" \"<value>\"`",

            "\nTODO Configuration commands:",
            " - `!wow config ignore <user> <true/false>`",
            " - `!wow config react_to_delete <true/false>`",
            " - `!wow config quiet_mode <true/false>`",
            " - `!wow config reset`"
        });

        public async Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            if (msg.Severity <= Program.logLevel)
            {
                await verboseManager.SendEmbedMessage(embedMessage.Log($"{msg.Message}", msg.Severity, msg.Source));
            }
        }

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
                    case "keyword":
                        await ExecuteKeyword();
                        break;

                    case "config":
                        await ExecuteConfig();
                        break;

                    default:
                        await ExecuteMain();
                        break;
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
                    break;

                case "reload":
                    await mainCommands.Reload();
                    break;

                case "echo":
                    await mainCommands.Echo(currentCommand.message.Content);
                    break;

                case "pause":
                    await mainCommands.Pause(Convert.ToDouble(currentCommand.split[2]));
                    break;

                default:
                    await verboseManager.SendEmbedMessage(embedMessage.Error($"No such command.{pointerToHelpText}"));
                    break;
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
                        break;

                    case "remove":
                        await keywordCommands.Remove(currentCommand.parameters);
                        break;

                    case "edit":
                        await keywordCommands.Edit(currentCommand.parameters);
                        break;

                    case "list":
                        await keywordCommands.List();
                        break;

                    default:
                        await verboseManager.SendEmbedMessage(embedMessage.Error($"\nNo such command.\n{pointerToHelpText}"));
                        break;

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
                        await configCommands.Ignore(currentCommand.message.MentionedUsers, currentCommand.split[4]);
                        break;

                    case "react_to_delete":
                        await configCommands.ReactToDelete(currentCommand.split[3]);
                        break;

                    case "quiet_mode":
                        await configCommands.QuietMode(currentCommand.split[3]);
                        break;

                    case "reset":
                        await configCommands.Reset();
                        break;

                    default:
                        await verboseManager.SendEmbedMessage(embedMessage.Error($"\nNo such command.{pointerToHelpText}"));
                        break;

                }
            }
            catch (IndexOutOfRangeException)
            {
                await verboseManager.SendEmbedMessage(embedMessage.Error($"\nA command was specified with a missing option.{pointerToHelpText}"));
            }

        }


    }
}

