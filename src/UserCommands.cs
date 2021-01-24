using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.IO;
using Discord;
using Discord.WebSocket;

namespace wowwowwow
{
    public class UserCommands
    {
        static private CommandManager commandManager = new CommandManager();

        public async Task Help(bool fromHelpCommand = false)
        {
            if (fromHelpCommand == false)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, "Program", "no such command"));
            }
            await Program.Log(new LogMessage(LogSeverity.Info, "Program", commandManager.helpText));
        }

        public async Task Reload()
        {
            await commandManager.LoadKeywords();
            await Program.Log(new LogMessage(LogSeverity.Info, "Program", "reloaded keywords"));
        }

        public async Task Echo(string command)
        {
            await Program.Log(new LogMessage(LogSeverity.Info, "Program", command));
        }

        public async Task Add(List<string> parameters)
        {
            await Program.Log(new LogMessage(LogSeverity.Debug, "Program", $"{parameters[0]}"));

            CommandManager.keywords.Add(parameters[0], parameters[1]);
            await commandManager.SaveKeywords();
            await Program.Log(new LogMessage(LogSeverity.Info, "Program", "added new keyword"));
        }

        public async Task Remove(List<string> parameters)
        {
            if (!CommandManager.keywords.ContainsKey(parameters[0]))
            {
                await Program.Log(new LogMessage(LogSeverity.Error, "Program", "no such keyword"));
                return;
            }
            CommandManager.keywords.Remove(parameters[0]);
            await commandManager.SaveKeywords();
            await Program.Log(new LogMessage(LogSeverity.Info, "Program", "removed keyword"));
        }

        public async Task Edit(List<string> parameters)
        {
            if (!CommandManager.keywords.ContainsKey(parameters[0]))
            {
                await Program.Log(new LogMessage(LogSeverity.Error, "Program", "no such keyword"));
                return;
            }
            CommandManager.keywords.Remove(parameters[0]);
            CommandManager.keywords.Add(parameters[0], parameters[1]);
            await commandManager.SaveKeywords();
            await Program.Log(new LogMessage(LogSeverity.Info, "Program", "edited keyword"));
        }

        public async Task List()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var keyword in CommandManager.keywords.Keys)
            {
                sb.Append("\n" + keyword);
            }
            await Program.Log(new LogMessage(LogSeverity.Info, "Program", sb.ToString()));
        }

    }

}

