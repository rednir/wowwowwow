using System;
using System.Threading.Tasks;
using Discord;

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
            await dataManager.LoadData();
            await verboseManager.sendEmbedMessage(embedMessage.Info($"{DataManager.keywords.Count} keywords were reloaded"));
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

