using System;
using System.Threading.Tasks;
using Discord;

namespace wowwowwow.UserCommands
{
    public class Main : UserCommands
    {

        private string helpText = string.Join(Environment.NewLine,
        new string[]
        {
            "Main Commands:",
            " - `!wow help`",
            " - `!wow reload`",
            " - `!wow echo`",
            " - `!wow pause <minutes>`",

            "\nVoice Channel Commands",
            " - `!wow vc add \"<url/search>\"`",
            " - `!wow vc leave`",
            " - `!wow vc queue`",
            " - `!wow vc skip`",

            "\nKeyword Commands:",
            " - `!wow keyword list`",
            " - `!wow keyword add \"<keyword>\" \"*<optional:image> <value>\"`",
            " - `!wow keyword remove \"<keyword>\"`",
            " - `!wow keyword edit \"<keyword>\" \"<optional:image> <value>\"`",

            "\nGeometry Dash Commands: (todo)",
            " - `!wow gd search <user/level> <search>`",
            " - `!wow gd daily`",
            " - `!wow gd weekly`",
            " - `!wow gd top10`",

            "\nMisc commands",
            " - `!wow misc count`",
            " - `!wow misc pfp`",

            "\nConfiguration Commands:",
            " - `!wow config ignore <user> <true/false>`",
            " - `!wow config react_to_delete <true/false>`",
            " - `!wow config quiet_mode <true/false>` (todo)",
            " - `!wow config reset <config/keywords/all>`",

            "\nTODO: welcome text, different datamanager for different guilds, hardcoded perms with refresh, pause "
        });


        public async Task Help()
        {
            await verboseManager.SendEmbedMessage(embedMessage.Info(helpText));
        }

        public async Task Reload()
        {
            await dataManager.LoadData();
            await verboseManager.SendEmbedMessage(embedMessage.Info($"{DataManager.keywords.Count} keywords were reloaded"));
        }

        public async Task Echo(string command)
        {
            await verboseManager.SendEmbedMessage(embedMessage.Log(command, LogSeverity.Info, $"[{this.ToString()}]"));
        }

        public async Task Pause(double minutes)
        {
            if (minutes > 999 || minutes <= 0)
            {
                await verboseManager.SendEmbedMessage(embedMessage.Error("The number of minutes specified was either too big or too small"));
                return;
            }
            Program.isBotPaused = true;
            await verboseManager.SendEmbedMessage(embedMessage.Info($"Bot has been paused for {minutes} minutes"));
            await Task.Delay(Convert.ToInt32(minutes * 60000.0));
            Program.isBotPaused = false;
        }

    }

}

