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

            "\nGeometry Dash Commands:",
            " - `!wow gd search daily/weekly/<search>`",
            " - `!wow gd pointercrate`",

            "\nTRN commands:",
            " - `!wow trn csgo <steam_identifier>`",
            " - `!wow trn apex <origin_identifier>`",
            " - `!wow trn overwatch <battlenet_identifier>`",

            "\nosu! Commands:",
            " - `!wow osu user <username/id>`",
            " - `!wow osu score <id> (todo)`",

            "\nMisc commands",
            " - `!wow misc count`",
            " - `!wow misc pfp`",

            "\nConfiguration Commands:",
            " - `!wow config ignore <user> <true/false>`",
            " - `!wow config react_to_delete <true/false>`",
            " - `!wow config quiet_mode <true/false>` (todo)",
            " - `!wow config reset <config/keywords/all>` (todo)",

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

        public async Task<double> Pause(double minutes)
        {
            if (minutes > 999 || minutes <= 0)
            {
                await verboseManager.SendEmbedMessage(embedMessage.Error("The number of minutes specified was either too big or too small"));
                return 0;
            }

            await verboseManager.SendEmbedMessage(embedMessage.Info($"Bot has been paused for {minutes} minutes"));
            return minutes * 60000.0;
        }

    }

}

