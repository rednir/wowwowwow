using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using Discord.Rest;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;


namespace wowwowwow.UserCommands
{
    public class Osu : UserCommands
    {
        private HttpClient httpClient = new HttpClient();
        private static string osuSecret = File.ReadAllText("osuToken.txt");
        private static Dictionary<string, IEmote> rankingIcons = new Dictionary<string, IEmote>()
        {
            {"a", Emote.Parse("<:osua:807023193264881664>")},
            {"s", Emote.Parse("<:osus:807023232116981801>")},
            {"sh", Emote.Parse("<:osush:807023257357123595>")},
            {"ss", Emote.Parse("<:osuss:807023277180583958>")},
            {"ssh", Emote.Parse("<:osussh:807023289742262272>")}
        };

        private class UserData
        {
            public bool is_online { get; set; }
            public bool is_supporter { get; set; }
            public string username { get; set; }
            public string playmode { get; set; }
            public string join_date { get; set; }
            public string cover_url { get; set; }
            public Statistics statistics { get; set; }

            public class Statistics
            {
                public double pp { get; set; }
                public double hit_accuracy { get; set; }
                public int play_time { get; set; }
                public Dictionary<string, object> rank { get; set; }       // key: global/country
                public GradeCounts grade_counts { get; set; }
                public class GradeCounts
                {
                    public int ss { get; set; }
                    public int ssh { get; set; }
                    public int s { get; set; }
                    public int sh { get; set; }
                    public int a { get; set; }
                }
            }

        }

        private class UserScores
        {
            // theres probably a better way to do this but this works for now
            public ulong id { get; set; }
        }

        private async Task SetAuthorizationHeader()
        {
            Dictionary<string, string> tokenReqValues = new Dictionary<string, string>
            {
                {"client_id", "5050"},
                {"client_secret", osuSecret},
                {"grant_type", "client_credentials"},
                {"scope", "public"}
            };

            var tokenReqResponseJson = await httpClient.PostAsync("https://osu.ppy.sh/oauth/token", new FormUrlEncodedContent(tokenReqValues));
            Dictionary<string, object> tokenReqResponse = await tokenReqResponseJson.Content.ReadFromJsonAsync<Dictionary<string, object>>();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenReqResponse["access_token"].ToString());
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private VerboseManager.EmbedMessage ParseUserDataIntoMessage(UserData userData, UserScores[] userScores)
        {
            string rank = userData.statistics.rank["global"] is null ? "(unranked)" : $"#{userData.statistics.rank["global"]}";
            string isOnline = Convert.ToBoolean(userData.is_online) ? "🟢" : "🔴";
            string isSupporter = Convert.ToBoolean(userData.is_supporter) ? "*(osu!supporter)*" : "";
            return embedMessage.GenericResponse($"{userData.cover_url} Performance: {userData.statistics.pp}pp\nAccuracy: {Math.Round(userData.statistics.hit_accuracy, 2)}%\nPlay Time: {userData.statistics.play_time / 3600}h\n\n{rankingIcons["ss"]}: {userData.statistics.grade_counts.ss} | {rankingIcons["ssh"]}: {userData.statistics.grade_counts.ssh} | {rankingIcons["s"]}: {userData.statistics.grade_counts.s} | {rankingIcons["sh"]}: {userData.statistics.grade_counts.sh} | {rankingIcons["a"]}: {userData.statistics.grade_counts.a}\n\nTop Play: https://osu.ppy.sh/scores/{userData.playmode}/{userScores[0].id}", true, false, $"{isOnline} **{userData.username}** {isSupporter}  |  {rank}", $"https://osu.ppy.sh/u/{userData.username}", $"Joined: {DateTime.Parse(userData.join_date)}");
        }

        public async Task User(string user)
        {
            RestUserMessage loadingMessage = await verboseManager.SendEmbedMessage(embedMessage.Progress("Fetching user stats..."));

            await SetAuthorizationHeader();

            HttpResponseMessage userData = httpClient.GetAsync($"https://osu.ppy.sh/api/v2/users/{user}").Result;

            Uri finalUserDataUri = userData.RequestMessage.RequestUri;

            if (userData.StatusCode == HttpStatusCode.Unauthorized)
            {
                // when user is not an id but a username, the request is redirected and headers are lost, returning unaurthorized error
                // this requests again with the last url (so there is no redirect)
                Console.WriteLine("Redirect");
                userData = httpClient.GetAsync(finalUserDataUri).Result;
            }

            HttpResponseMessage userScores = httpClient.GetAsync($"{finalUserDataUri}/scores/best").Result;
            await loadingMessage.DeleteAsync();

            if (!userData.IsSuccessStatusCode || !userScores.IsSuccessStatusCode)
            {

                await verboseManager.SendEmbedMessage(embedMessage.Error($"todo error message: `{userData.StatusCode} {userData.ReasonPhrase}`"));
                return;
            }
            await verboseManager.SendEmbedMessage(ParseUserDataIntoMessage(await userData.Content.ReadFromJsonAsync<UserData>(), await userScores.Content.ReadFromJsonAsync<UserScores[]>()));
        }

        public async Task Beatmap(string map)
        {
            await verboseManager.SendEmbedMessage(embedMessage.Warning("This doesn't do anything yet"));
        }

    }

}