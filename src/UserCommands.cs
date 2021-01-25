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

        private VerboseManager verboseManager = new VerboseManager();

        private VerboseManager.EmbedMessage embedMessage = new VerboseManager.EmbedMessage();

        public async Task Help(bool fromHelpCommand = false)
        {
            if (fromHelpCommand == false)
            {
                await verboseManager.sendEmbedMessage(embedMessage.Error("No such command"));
            }
            await verboseManager.sendEmbedMessage(embedMessage.Info(commandManager.helpText));
        }

        public async Task Reload()
        {
            await commandManager.LoadKeywords();
            await verboseManager.sendEmbedMessage(embedMessage.Info($"{CommandManager.keywords.Count} keywords were reloaded"));
        }

        public async Task Echo(string command)
        {
            await verboseManager.sendEmbedMessage(embedMessage.Log(command));
        }

        public async Task Add(List<string> parameters)
        {
            //await verboseManager.sendEmbedMessage(new LogMessage(LogSeverity.Debug, "Program", $"{parameters[0]}"));

            CommandManager.keywords.Add(parameters[0], parameters[1]);
            await commandManager.SaveKeywords();
            await verboseManager.sendEmbedMessage(embedMessage.Info($"{parameters[0]} was added as a keyword"));
        }

        public async Task Remove(List<string> parameters)
        {
            if (!CommandManager.keywords.ContainsKey(parameters[0]))
            {
                await verboseManager.sendEmbedMessage(embedMessage.Error($"The keyword '{parameters[0]}' does not exist"));
                return;
            }
            CommandManager.keywords.Remove(parameters[0]);
            await commandManager.SaveKeywords();
            await verboseManager.sendEmbedMessage(embedMessage.Info($"The keyword '{parameters[0]}' was removed"));
        }

        public async Task Edit(List<string> parameters)
        {
            if (!CommandManager.keywords.ContainsKey(parameters[0]))
            {
                await verboseManager.sendEmbedMessage(embedMessage.Error($"The keyword '{parameters[0]}' does not exist"));
                return;
            }
            CommandManager.keywords.Remove(parameters[0]);
            CommandManager.keywords.Add(parameters[0], parameters[1]);
            await commandManager.SaveKeywords();
            await verboseManager.sendEmbedMessage(embedMessage.Info($"The keyword '{parameters[0]}' was edited"));
        }

        public async Task List()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("**List of keywords:**");
            foreach (var keyword in CommandManager.keywords.Keys)
            {
                if (keyword.StartsWith("@"))
                {
                    sb.Append($"\n - @ {keyword.Substring(1)}");
                }
                else
                {
                    sb.Append("\n - " + keyword);
                }
                
            }
            await verboseManager.sendEmbedMessage(embedMessage.Info(sb.ToString()));
        }

        public async Task Pause(double minutes)
        {
            if (minutes > 999 || minutes <= 0)
            {
                await verboseManager.sendEmbedMessage(embedMessage.Error("The number of minutes specified was either too big or too small"));
                return;
            }
            Program.isBotPaused = true;
            await verboseManager.sendEmbedMessage(embedMessage.Info($"Bot has been paused for {minutes} minutes"));
            await Task.Delay(Convert.ToInt32(minutes * 60000.0));
            Program.isBotPaused = false;
        }

    }

}

