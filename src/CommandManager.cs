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
        private UserCommands userCommands = new UserCommands();
        private VerboseManager verboseManager = new VerboseManager();
        private VerboseManager.EmbedMessage embedMessage = new VerboseManager.EmbedMessage();
        public const string commandIdentifier = "!wow";

        public string helpText = string.Join(Environment.NewLine,
        new string[]
        {
            "All user commands:\n",
            "!wow reload",
            "!wow list",
            "!wow add \"<keyword>\" \"<value>\"",
            "!wow remove \"<keyword>\"",
            "!wow edit \"<keyword>\" \"<value>\"",
            "!wow pause <minutes>"
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
            string[] commandSplit = command.Split(" ");
            if (commandSplit.Length <= 1)
            {
                await userCommands.Help();
                return;
            }

            //command = Regex.Replace(command, @"\t|\n|\r", "\n");

            // add all text in quotes to a list
            Regex regex = new Regex("\"(.*?)\"", RegexOptions.Singleline);
            var matches = regex.Matches(command);
            List<String> parameters = new List<string>();
            foreach (Match match in matches)
            {
                parameters.Add(match.Groups[1].ToString().Trim('\''));
            }

            try
            {
                switch (commandSplit[1])
                {
                    case "help":
                        await userCommands.Help(true);
                        break;

                    case "reload":
                        await userCommands.Reload();
                        break;

                    case "echo":
                        await userCommands.Echo(command);
                        break;

                    case "add":
                        await userCommands.Add(parameters);
                        break;

                    case "remove":
                        await userCommands.Remove(parameters);
                        break;

                    case "edit":
                        await userCommands.Edit(parameters);
                        break;
                    
                    case "list":
                        await userCommands.List();
                        break;
                    
                    case "pause":
                        await userCommands.Pause(Convert.ToInt32(commandSplit[2]));
                        break;

                    default:
                        await userCommands.Help();
                        break;
                }
                
            }
            catch (ArgumentOutOfRangeException ex)
            {
                await verboseManager.sendEmbedMessage(embedMessage.Error($"```{ex.Message}```\nEnsure that keywords and values are quoted like \"this\""));
            }
        }



    }
}

