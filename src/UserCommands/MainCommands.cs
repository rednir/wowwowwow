using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.IO;
using Discord;
using Discord.WebSocket;

namespace wowwowwow.UserCommands
{
    public class Main : UserCommands
    {
        
        public async Task Help()
        {
            await verboseManager.sendEmbedMessage(embedMessage.Info(commandManager.helpText));
        }

        public async Task Reload()
        {
            await commandManager.LoadKeywords();
            await verboseManager.sendEmbedMessage(embedMessage.Info($"{CommandManager.keywords.Count} keywords were reloaded"));
        }

        public async Task Echo(string command)
        {
            await verboseManager.sendEmbedMessage(embedMessage.Log(command, LogSeverity.Info, $"[{this.ToString()}]"));
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

