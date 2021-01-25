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
        public async Task Ignore(IReadOnlyCollection<Discord.WebSocket.SocketUser> userCollection, string value)
        {
            SocketUser user = Enumerable.ElementAt(userCollection, 0);
            if (user.Id == Program.botAccountID)
            {
                await verboseManager.SendEmbedMessage(embedMessage.Error("The bot is always on the ignore list."));
                return;
            }

            if (Convert.ToBoolean(value))
            {
                if (DataManager.config["ignore"].Contains(user.Id))
                {
                    await verboseManager.SendEmbedMessage(embedMessage.Error($"The user {user.Mention} is already on the ignore list."));
                    return;
                }
                DataManager.config["ignore"].Add(user.Id);
                await verboseManager.SendEmbedMessage(embedMessage.Info($"The user {user.Mention} has been added to the ignore list."));
                await dataManager.SyncData();
            }
            else
            {
                if (!DataManager.config["ignore"].Contains(user.Id))
                {
                    await verboseManager.SendEmbedMessage(embedMessage.Error($"The user {user.Mention} is not on the ignore list."));
                    return;
                }
                DataManager.config["ignore"].Remove(user.Id);
                await verboseManager.SendEmbedMessage(embedMessage.Info($"The user {user.Mention} has been removed from the ignore list."));
                await dataManager.SyncData();
            }
            
        }

        public async Task ReactToDelete(string value)
        {
            await verboseManager.SendEmbedMessage(embedMessage.Warning("This doesn't do anything yet"));
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