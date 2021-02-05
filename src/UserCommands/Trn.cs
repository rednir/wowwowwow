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
    public class Trn : UserCommands
    {
        private HttpClient httpClient = new HttpClient();
        private static string trnSecret = File.ReadAllText("trnToken.txt");

        private class UserData
        {
            public Data data { get; set; }
            public class Data
            {
                public PlatformInfo platformInfo { get; set; }
                public Segments[] segments { get; set; }
                public class PlatformInfo
                {
                    public string platformUserId { get; set; }
                    public string platformUserHandle { get; set; }
                    public string avatarUrl { get; set; }
                }
                public class Segments
                {
                    public Stats stats { get; set; }
                    public class Stats
                    {
                        public Stat timePlayed { get; set; }
                        public Stat matchesPlayed { get; set; }
                        public Stat wlPercentage { get; set; }
                        public Stat level { get; set; } // apex

                        public Stat kills { get; set; }
                        public Stat deaths { get; set; }
                        public Stat kd { get; set; }

                        public Stat shotsAccuracy { get; set; }
                        public Stat shotsFired { get; set; }
                        public Stat shotsHit { get; set; }

                        public Stat headshotPct { get; set; }

                        public int mvpPct
                        {
                            get { return Convert.ToInt32(Math.Round(mvp.value / roundsPlayed.value * 100)); }
                            set { }
                        }
                        public Stat mvp { get; set; }
                        public Stat roundsPlayed { get; set; }

                        public class Stat
                        {
                            public string displayValue { get; set; }
                            public double value { get; set; }
                        }
                    }
                }

            }

        }

        public Trn()
        {
            SetAuthorizationHeader();
        }

        private void SetAuthorizationHeader()
        {
            httpClient.DefaultRequestHeaders.Add("TRN-Api-Key", trnSecret);
            //httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private VerboseManager.EmbedMessage ParseUserDataIntoMessage(UserData userData, string game)
        {
            var stats = userData.data.segments[0].stats;
            switch (game)
            {
                case "CS:GO":
                    return embedMessage.GenericResponse($"```Time Played: {Math.Round(stats.timePlayed.value / 3600)}h\nMatches Played: {stats.matchesPlayed.displayValue} ({stats.wlPercentage.displayValue} won)``` ```K/D: {stats.kd.displayValue}\n - Kills: {stats.kills.displayValue}\n - Deaths: {stats.deaths.displayValue}``` ```Headshot Accuracy: {stats.headshotPct.displayValue}\nOverall Accuracy: {stats.shotsAccuracy.displayValue}\n - Shots fired: {stats.shotsFired.displayValue}\n - Shots hit: {stats.shotsHit.displayValue}\n``` ```MVP Percentage: {stats.mvpPct}%\n - Total MVPs: {stats.mvp.displayValue}\n - Rounds played: {stats.roundsPlayed.displayValue}```", false, false, $"{userData.data.platformInfo.platformUserHandle}'s CS:GO Stats");

                case "Apex Legends":
                    return embedMessage.GenericResponse($"```Level: {stats}``````Kills: {stats}```", false, false, $"{userData.data.platformInfo.platformUserHandle}'s Apex Legends Stats");
                
                case "Overwatch":
                    return embedMessage.GenericResponse($"```Level: {stats}``````Kills: {stats}```", false, false, $"{userData.data.platformInfo.platformUserHandle}'s Overwatch Stats");

                default:
                    return embedMessage.Error($"This should never happen");
            }
        }

        private async Task<UserData> GetUserData(string platform, string user)
        {
            var response = httpClient.GetAsync($"https://public-api.tracker.gg/v2/csgo/standard/profile/{platform}/{user}").Result;
            return await response.Content.ReadFromJsonAsync<UserData>();
        }

        public async Task Csgo(string user)
        {
            UserData userData = await GetUserData("steam", user);
            if (userData.data is null)
            {
                await verboseManager.SendEmbedMessage(embedMessage.Error($"Could not find any stats, are you sure the user identifier is valid?\n\nYou can get a valid user identifier by going to your steam community profile and copying the final segment of the URL."));
                return;
            }
            await verboseManager.SendEmbedMessage(ParseUserDataIntoMessage(userData, "CS:GO"));
        }

        public async Task Apex(string user) // probably want to do different platforms other than origin
        {
            UserData userData = await GetUserData("origin", user);
            if (userData.data is null)
            {
                await verboseManager.SendEmbedMessage(embedMessage.Error($"Could not find any stats, are you sure the user identifier is valid?"));
                return;
            }
            await verboseManager.SendEmbedMessage(ParseUserDataIntoMessage(userData, "Apex Legends"));
        }

        public async Task Overwatch(string user) // probably want to do different platforms other than battlenet
        {
            UserData userData = await GetUserData("battlenet", user);
            if (userData.data is null)
            {
                await verboseManager.SendEmbedMessage(embedMessage.Error($"Could not find any stats, are you sure the user identifier is valid?"));
                return;
            }
            await verboseManager.SendEmbedMessage(ParseUserDataIntoMessage(userData, "Overwatch"));
        }


    }
}