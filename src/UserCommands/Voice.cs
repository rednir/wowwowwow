using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Audio;

namespace wowwowwow.UserCommands
{
    public class Voice : UserCommands
    {
        public static IAudioClient audioClient;
        public async Task Join(IVoiceChannel vc = null)
        {
            if (vc == null)
            {
                await verboseManager.SendEmbedMessage(embedMessage.Error("You must join a voice channel first before summoning the bot"));
                return;
            }
            audioClient = await vc.ConnectAsync();
        }

        public async Task Leave()
        {
            await audioClient.StopAsync();
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

