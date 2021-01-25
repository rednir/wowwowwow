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
            public string fullCommand { get; set; }
            public string[] splitCommand { get; set; }
            public List<string> parameters { get; set; }
        }

        private Command currentCommand;
        private UserCommands userCommands = new UserCommands();
        private VerboseManager verboseManager = new VerboseManager();
        private VerboseManager.EmbedMessage embedMessage = new VerboseManager.EmbedMessage();
        public const string commandIdentifier = "!wow";


        public string helpText = string.Join(Environment.NewLine,
        new string[]
        {
            "Main commands:",
            " - `!wow help`",
            " - `!wow reload`",
            " - `!wow pause <minutes>`",

            "\nKeyword commands:",
            " - `!wow keyword list`",
            " - `!wow keyword add \"<keyword>\" \"<value>\"`",
            " - `!wow keyword remove \"<keyword>\"`",
            " - `!wow keyword edit \"<keyword>\" \"<value>\"`",

            "\nOther commands:",
            " - `!`"
        });

        public static Dictionary<string, string> keywords = new Dictionary<string, string>();

        public async Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            if (msg.Severity <= Program.logLevel)
            {
                await verboseManager.sendEmbedMessage(embedMessage.Log($"**[{msg.Source}: {msg.Severity}]** {msg.Message}"));
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
                await userCommands.Help();
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
            catch
            {
                await userCommands.Help();
            }


        }


        private async Task ExecuteMain()
        {
            switch (currentCommand.splitCommand[1])
            {
                case "help":
                    await userCommands.Help(true);
                    break;

                case "reload":
                    await userCommands.Reload();
                    break;

                case "echo":
                    await userCommands.Echo(currentCommand.fullCommand);
                    break;

                case "pause":
                    await userCommands.Pause(Convert.ToDouble(currentCommand.splitCommand[2]));
                    break;

                default:
                    await userCommands.Help();
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
                        await userCommands.Add(currentCommand.parameters);
                        break;

                    case "remove":
                        await userCommands.Remove(currentCommand.parameters);
                        break;

                    case "edit":
                        await userCommands.Edit(currentCommand.parameters);
                        break;

                    case "list":
                        await userCommands.List();
                        break;

                    default:
                        await userCommands.Help();
                        break;

                }
            }
            catch (Exception ex)
            {
                await verboseManager.sendEmbedMessage(embedMessage.Error($"```{ex.Message}```\nEnsure that keywords and values are quoted like \"this\""));
            }
        }



    }
}

