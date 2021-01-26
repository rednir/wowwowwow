using System;
using System.Threading.Tasks;
using Discord;

namespace wowwowwow.UserCommands
{
    public class Voice : UserCommands
    {

        public async Task Join()
        {
            await verboseManager.SendEmbedMessage(embedMessage.Warning("This doesn't do anything yet"));
        }

        public async Task Leave()
        {
            await verboseManager.SendEmbedMessage(embedMessage.Warning("This doesn't do anything yet"));
        }

        public async Task Add()
        {
            await verboseManager.SendEmbedMessage(embedMessage.Warning("This doesn't do anything yet"));
        }

        public async Task Skip()
        {
            await verboseManager.SendEmbedMessage(embedMessage.Warning("This doesn't do anything yet"));
        }


    }

}

