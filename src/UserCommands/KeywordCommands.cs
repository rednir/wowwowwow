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
    public class Keyword : UserCommands
    {
        public async Task Add(List<string> parameters)
        {
            //await verboseManager.sendEmbedMessage(new LogMessage(LogSeverity.Debug, "Program", $"{parameters[0]}"));

            DataManager.keywords.Add(parameters[0], parameters[1]);
            await dataManager.SaveData();
            await verboseManager.sendEmbedMessage(embedMessage.Info($"{parameters[0]} was added as a keyword"));
        }

        public async Task Remove(List<string> parameters)
        {
            if (!DataManager.keywords.ContainsKey(parameters[0]))
            {
                await verboseManager.sendEmbedMessage(embedMessage.Error($"The keyword '{parameters[0]}' does not exist"));
                return;
            }
            DataManager.keywords.Remove(parameters[0]);
            await dataManager.SaveData();
            await verboseManager.sendEmbedMessage(embedMessage.Info($"The keyword '{parameters[0]}' was removed"));
        }

        public async Task Edit(List<string> parameters)
        {
            if (!DataManager.keywords.ContainsKey(parameters[0]))
            {
                await verboseManager.sendEmbedMessage(embedMessage.Error($"The keyword '{parameters[0]}' does not exist"));
                return;
            }
            DataManager.keywords.Remove(parameters[0]);
            DataManager.keywords.Add(parameters[0], parameters[1]);
            await dataManager.SaveData();
            await verboseManager.sendEmbedMessage(embedMessage.Info($"The keyword '{parameters[0]}' was edited"));
        }

        public async Task List()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("**List of keywords:**");
            foreach (var keyword in DataManager.keywords.Keys)
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

    }

}

