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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;



namespace wowwowwow.UserCommands
{
    public class GeometryDash : UserCommands
    {
        //private static string script = "import sys\nimport gd\n\nclient = gd.Client()\n\n\nasync def Main():\n    args = ''\n\n    for a in sys.argv[1:]:\n        if a == sys.argv[len(sys.argv) - 1]:\n            args += a\n        else:\n            args += a + ' '\n\n    # command: search\n    if args.startswith('Daily'):\n        await daily()\n    elif args.startswith('Weekly'):\n        await weekly()\n    elif args.startswith('SEARCH_TERM'):\n        await search(args[len('SEARCH_TERM') + 1:])\n\n\nasync def print_level_data(level):\n    print(level.id)\n    print(level.name)\n    print(level.creator)\n    print(level.difficulty)\n    print(level.stars)\n    print(level.downloads)\n    print(level.rating)\n\n\n\n#####    MAIN FUNCTIONS    ####\n\nasync def search(term):\n    found_levels = await client.search_levels_on_page(0, term)\n    found_levels = found_levels[:3]\n    print(len(found_levels))\n    for i in range(len(found_levels)):\n        await print_level_data(found_levels[i])\n\n\nasync def daily():\n    current_daily = await client.get_daily()\n    print(1)\n    await print_level_data(current_daily)\n\n\nasync def weekly():\n    current_weekly = await client.get_weekly()\n    print(1)\n    await print_level_data(current_weekly)\n\n\n\n####    CODE START   ####\n\n\ntry:\n    client.run(Main())\nexcept gd.errors.MissingAccess:\n    print('NO_LEVELS')\n";
        private HttpClient httpclient = new HttpClient() { BaseAddress = new Uri("https://pointercrate.com") };

        private class LevelData
        {
            public int id { get; set; }
            public string name { get; set; }
            public string creator { get; set; }
            public Emote difficultyField { get; set; }
            public int stars { get; set; }
            public int downloads { get; set; }
            public int rating { get; set; }

            public dynamic difficulty
            {

                get
                {
                    return difficultyField;
                }

                set
                {
                    switch (value.Substring(value.IndexOf(".") + 1))
                    {
                        case "NA":
                            value = Emote.Parse("<:na:805843202594701383>");
                            break;

                        case "EASY":
                            value = Emote.Parse("<:easy:805843230271864858>");
                            break;

                        case "NORMAL":
                            value = Emote.Parse("<:normal:805843243446042653>");
                            break;

                        case "HARD":
                            value = Emote.Parse("<:hard:805843253600452619>");
                            break;

                        case "HARDER":
                            value = Emote.Parse("<:harder:805843267806691369>");
                            break;

                        case "INSANE":
                            value = Emote.Parse("<:insane:805843280508092497>");
                            break;

                        case "EASY_DEMON":
                            value = Emote.Parse("<:easydemon:805845065065824327>");
                            break;

                        case "MEDIUM_DEMON":
                            value = Emote.Parse("<:mediumdemon:805845131580276796>");
                            break;

                        case "HARD_DEMON":
                            value = Emote.Parse("<:harddemon:805845146376863744>");
                            break;

                        case "INSANE_DEMON":
                            value = Emote.Parse("<:insanedemon:805845159769800744>");
                            break;

                        case "EXTREME_DEMON":
                            value = Emote.Parse("<:extremedemon:805845173447557150>");
                            break;

                        default:
                            value = Emote.Parse("<:na:805843202594701383>");
                            break;
                    }
                    difficultyField = value;
                }

            }

        }

        private List<LevelData> ParseLevelData(string[] raw)
        {
            const int numberOfParameters = 7;
            int numberOfLevels = Convert.ToInt32(raw[0]);
            Console.WriteLine(raw[0]);
            List<LevelData> listOfLevelData = new List<LevelData>();

            for (int i = 0; i < numberOfLevels; i++)
            {
                int x = numberOfParameters * i;    // every level, use the next set of lines
                LevelData levelData = new LevelData();

                levelData.id = Convert.ToInt32(raw[1 + x]);
                levelData.name = raw[2 + x];
                levelData.creator = raw[3 + x];
                levelData.difficulty = raw[4 + x];
                levelData.stars = Convert.ToInt32(raw[5 + x]);
                levelData.downloads = Convert.ToInt32(raw[6 + x]);
                levelData.rating = Convert.ToInt32(raw[7 + x]);

                listOfLevelData.Add(levelData);
            }

            return listOfLevelData;
        }

        private string LevelDataToString(LevelData levelData)
        {
            return $"{levelData.difficulty} **{levelData.stars}\\*  |**  {levelData.name} by {levelData.creator}  **|** `{levelData.id}`\n{(levelData.downloads >= 1000 ? $"{levelData.downloads / 1000}K" : levelData.downloads)} downloads, {(levelData.rating >= 1000 ? $"{levelData.rating / 1000}K" : levelData.rating)} likes";
        }

        private async Task<string[]> RunScript(string args)
        {
            string[] outputLines;
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = "python";
            start.Arguments = $"src/GeometryDashScripts.py {args}";
            //start.Arguments = $"{args} -c {script}";
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string output = await reader.ReadToEndAsync();
                    outputLines = output.Split(Environment.NewLine);
                }
            }
            return outputLines;
        }

        public async Task Search(string arg)
        {
            RestUserMessage downloadingMessage;
            string resultMessageTitle;

            if (arg.Equals("daily", StringComparison.OrdinalIgnoreCase))
            {
                arg = "Daily";
                downloadingMessage = await verboseManager.SendEmbedMessage(embedMessage.Progress("Fetching daily level..."));
                resultMessageTitle = "Daily Level";
            }
            else if (arg.Equals("weekly", StringComparison.OrdinalIgnoreCase))
            {
                arg = "Weekly";
                downloadingMessage = await verboseManager.SendEmbedMessage(embedMessage.Progress("Fetching weekly demon..."));
                resultMessageTitle = "Weekly Demon";
            }
            else
            {
                arg = $"SEARCH_TERM {arg}";
                downloadingMessage = await verboseManager.SendEmbedMessage(embedMessage.Progress("Searching level..."));
                resultMessageTitle = "Level Found";
            }

            string[] output = await RunScript(arg);
            if (output[0] == "NO_LEVELS")
            {
                await downloadingMessage.DeleteAsync();
                await verboseManager.SendEmbedMessage(embedMessage.Warning("No levels were found. Try a different search term."));
                return;
            }

            await downloadingMessage.DeleteAsync();
            List<LevelData> listOfLevelData = ParseLevelData(output);
            foreach (LevelData levelData in listOfLevelData)
            {
                await verboseManager.SendEmbedMessage(embedMessage.GenericResponse(LevelDataToString(levelData), false, false, resultMessageTitle, $"https://gdbrowser.com/{levelData.id}"));
            }
        }

        public async Task Pointercrate()
        {
            StringBuilder stringOfDemons = new StringBuilder();
            httpclient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = httpclient.GetAsync("/api/v2/demons/listed/").Result;
            if (response.IsSuccessStatusCode)
            {
                var dataObjects = response.Content.ReadFromJsonAsync<List<JsonElement>>().Result;
                Console.WriteLine("ss " + dataObjects);
                for (int i = 0; i < 20; i++)
                {
                    stringOfDemons.AppendLine($"{i + 1}) *{dataObjects[i].GetProperty("name")}* | https://pointercrate.com/demonlist/{i + 1}");
                }
                await verboseManager.SendEmbedMessage(embedMessage.GenericResponse(stringOfDemons.ToString(), false, false, "Pointercrate Demons List"));
            }
            else
            {
                await verboseManager.SendEmbedMessage(embedMessage.Error($"{response.StatusCode} {response.ReasonPhrase}"));
            }
        }

    }

}