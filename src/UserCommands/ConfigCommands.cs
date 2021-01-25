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
    public class Config : UserCommands
    {
        public async Task Ignore(string user, string value)
        {
            await verboseManager.sendEmbedMessage(embedMessage.Warning("This doesn't do anything yet"));
        }

        public async Task ReactToDelete(string value)
        {
            await verboseManager.sendEmbedMessage(embedMessage.Warning("This doesn't do anything yet"));
        }

        public async Task QuietMode(string value)
        {
            await verboseManager.sendEmbedMessage(embedMessage.Warning("This doesn't do anything yet"));
        }
        public async Task Reset()
        {
            await verboseManager.sendEmbedMessage(embedMessage.Warning("This doesn't do anything yet"));
        }


    }

}