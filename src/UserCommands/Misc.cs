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
    public class Misc : UserCommands
    {
        public async Task Count()
        {
            
        }

        public async Task Pfp(IEnumerable<SocketUser> usersMentioned, SocketUser requestBy)
        {
            SocketUser user;
            
            if (usersMentioned.Count() == 0)
            {
                user = requestBy;
            }
            else
            {
                user = usersMentioned.First();
            }

            await verboseManager.SendEmbedMessage(embedMessage.GenericResponse(user.GetAvatarUrl(), true, false, $"{user.Username}'s profile picture:"));
        }

    }

}