using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.IO;
using Discord;
using Discord.WebSocket;

namespace wowwowwow.UserCommands
{
    public class Config : UserCommands
    {

        private async Task SendGenericUpdateMessage(bool value, string messageTrue, string messageFalse)
        {
            switch (value)
            {
                case true:
                    await verboseManager.SendEmbedMessage(embedMessage.Info($"Option was turned on.\n{messageTrue}"));
                    return;

                case false:
                    await verboseManager.SendEmbedMessage(embedMessage.Info($"Option was turned off.\n{messageFalse}"));
                    return;
            }
        }

        public async Task Ignore(SocketUser user, string value)
        {
            bool boolValue = Convert.ToBoolean(value);
            if (user.Id == Program.botAccountID)
            {
                await verboseManager.SendEmbedMessage(embedMessage.Error("The bot is always on the ignore list."));
                return;
            }

            switch (boolValue)
            {
                case true:
                    if (DataManager.config["ignore"].Contains(user.Id))
                    {
                        await verboseManager.SendEmbedMessage(embedMessage.Error($"The user {user.Mention} is already on the ignore list."));
                        return;
                    }
                    DataManager.config["ignore"].Add(user.Id);
                    await verboseManager.SendEmbedMessage(embedMessage.Info($"The user {user.Mention} has been added to the ignore list."));
                    await dataManager.SyncData();
                    return;

                case false:
                    if (!DataManager.config["ignore"].Contains(user.Id))
                    {
                        await verboseManager.SendEmbedMessage(embedMessage.Error($"The user {user.Mention} is not on the ignore list."));
                        return;
                    }
                    DataManager.config["ignore"].Remove(user.Id);
                    await verboseManager.SendEmbedMessage(embedMessage.Info($"The user {user.Mention} has been removed from the ignore list."));
                    await dataManager.SyncData();
                    return;

            }

        }

        public async Task ReactToDelete(string value)
        {
            bool boolValue = Convert.ToBoolean(value);
            DataManager.config["reactToDelete"] = boolValue;
            await dataManager.SyncData();

            await SendGenericUpdateMessage(boolValue, "Responses to user messages will now contain a delete reaction.", "Responses to user messages will not contain a delete reaction.");
        }

        public async Task QuietMode(string value)
        {
            await verboseManager.SendEmbedMessage(embedMessage.Warning("This doesn't do anything yet"));
        }
        
        public async Task Reset()
        {
            await verboseManager.SendEmbedMessage(embedMessage.Warning("This doesn't do anything yet"));
        }


    }

}