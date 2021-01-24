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
        public const string commandIdentifier = "!wow";

        public string helpText = string.Join(Environment.NewLine,
        new string[]
        {
            "commands:\n",
            "!wow reload",
            "!wow echo",
            "!wow list",
            "!wow add \"<keyword>\" \"<value>\"",
            "!wow remove \"<keyword>\"",
            "!wow edit \"<keyword>\" \"<value>\""
        });

        public static Dictionary<string, string> keywords = new Dictionary<string, string>();

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
                        await userCommands.Help();
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

                    default:
                        await userCommands.Help(true);
                        break;
                }
                
            }
            catch (ArgumentOutOfRangeException ex)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, "Program", $"{ex}\nKeywords and values should be quoted like \"this\""));
            }
        }



    }
}

