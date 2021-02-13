using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.IO;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System.Net.Http;
using System.Net.Http.Json;


namespace wowwowwow.UserCommands
{
    public class Misc : UserCommands
    {
        private HttpClient httpClient = new HttpClient();
        private string[] pepegaLanguages = new string[] { "es", "ru", "ar", "pt", "fr" };

        public async Task Count()
        {
            await verboseManager.SendEmbedMessage(embedMessage.GenericResponse("Counting has been started in this channel. I'll start:\n\n**1**", false, false));
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

        public async Task Pepega(string text)
        {
            RestUserMessage downloadingMessage = await verboseManager.SendEmbedMessage(embedMessage.Progress("Please wait..."));

            Random random = new Random();
            List<string> randomlySortedLanguages = pepegaLanguages.OrderBy(x => random.Next()).ToList(); 
            randomlySortedLanguages.Add("en");  // always finally translate back to english

            for (int i = 0; i < randomlySortedLanguages.Count; i++)
            {
                Dictionary<string, string> reqBody = new Dictionary<string, string>
                {
                    {"q", text},
                    {"source", i == 0 ? "en" : randomlySortedLanguages[i - 1]},       // if its the first time through the loop, text will always be english
                    {"target", randomlySortedLanguages[i]}
                };
                var response = httpClient.PostAsync("https://libretranslate.com/translate", new FormUrlEncodedContent(reqBody)).Result;
                Dictionary<string, string> translatedText = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                text = translatedText["translatedText"];
            }

            await downloadingMessage.DeleteAsync();
            await verboseManager.SendEmbedMessage(embedMessage.GenericResponse($"{Emote.Parse("<:Pepega:695943070281105460>")} 📢 {text}"));
        }

    }

}