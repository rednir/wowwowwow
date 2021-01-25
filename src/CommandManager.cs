using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.IO;
using Discord;
using Discord.WebSocket;
using wowwowwow.UserCommands;

namespace wowwowwow
{
    public class CommandManager
    {

        private class Command
        {
            public string fullCommand { get; set; }
            public string[] splitCommand { get; set; }
            public List<string> parameters { get; set; }
        }

        private Command currentCommand;
        private UserCommands.Main mainCommands = new UserCommands.Main();
        private UserCommands.Keyword keywordCommands = new UserCommands.Keyword();
        private VerboseManager verboseManager = new VerboseManager();
        private VerboseManager.EmbedMessage embedMessage = new VerboseManager.EmbedMessage();
        public const string commandIdentifier = "!wow";


        public string helpText = string.Join(Environment.NewLine,
        new string[]
        {
            "Main commands:",
            " - `!wow help`",
            " - `!wow reload`",
            " - `!wow blacklist <user tag>` todo",
            " - `!wow echo`",
            " - `!wow pause <minutes>`",

            "\nKeyword commands:",
            " - `!wow keyword list`",
            " - `!wow keyword add \"<keyword>\" \"<optional: image>\" \"<value>\"`",
            " - `!wow keyword remove \"<keyword>\"`",
            " - `!wow keyword edit \"<keyword>\" \"<optional: image>\" \"<value>\"`",

            "\nOther commands:",
            " - `!`"
        });

        public static Dictionary<string, string> keywords = new Dictionary<string, string>();

        public async Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            if (msg.Severity <= Program.logLevel)
            {
                await verboseManager.sendEmbedMessage(embedMessage.Log($"{msg.Message}", msg.Severity, msg.Source));
            }
        }

        public async Task LoadKeywords()
        {
            await Task.Run(() => keywords = JsonSerializer.Deserialize<Dictionary<String, String>>(File.ReadAllText("keywords.json")));
        }

        public async Task SaveKeywords()
        {
            await File.WriteAllTextAsync("keywords.json", JsonSerializer.Serialize(keywords));
        }

        public async Task Execute(string command)
        {
            currentCommand = new Command()
            {
                fullCommand = command,
                splitCommand = command.Split(" "),
                parameters = new List<string>()
            };

            if (currentCommand.splitCommand.Length <= 1)
            {
                await mainCommands.Help();
                return;
            }

            // add all text in quotes to list
            Regex regex = new Regex("\"(.*?)\"", RegexOptions.Singleline);
            var matches = regex.Matches(command);
            foreach (Match match in matches)
            {
                currentCommand.parameters.Add(match.Groups[1].ToString().Trim('\''));
            }

            try
            {
                switch (currentCommand.splitCommand[1])
                {
                    case "keyword":
                        await ExecuteKeyword();
                        break;

                    default:
                        await ExecuteMain();
                        break;
                }
            }
            catch (Exception ex)
            {
                await verboseManager.sendEmbedMessage(embedMessage.Error($"\nCould not execute the command, the following error was returned:```{ex.Message}```"));
                await mainCommands.Help();
            }


        }


        private async Task ExecuteMain()
        {
            switch (currentCommand.splitCommand[1])
            {
                case "help":
                    await mainCommands.Help();
                    break;

                case "reload":
                    await mainCommands.Reload();
                    break;

                case "echo":
                    await mainCommands.Echo(currentCommand.fullCommand);
                    break;

                case "pause":
                    await mainCommands.Pause(Convert.ToDouble(currentCommand.splitCommand[2]));
                    break;

                default:
                    await verboseManager.sendEmbedMessage(embedMessage.Error("No such command"));
                    await mainCommands.Help();
                    break;
            }
        }


        private async Task ExecuteKeyword()
        {
            try
            {
                switch (currentCommand.splitCommand[2])
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
                        await verboseManager.sendEmbedMessage(embedMessage.Error($"\nNo such command"));
                        await mainCommands.Help();
                        break;

                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                await verboseManager.sendEmbedMessage(embedMessage.Error($"Could not execute the command, the following error was returned:```{ex.Message}```Ensure that keywords and values are quoted like \"this\""));
            }
            catch (IndexOutOfRangeException)
            {
                await verboseManager.sendEmbedMessage(embedMessage.Error($"\nA command was specified with a missing option"));
                await mainCommands.Help();
            }

        }



    }
}

