using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.IO;
using Discord;
using Discord.WebSocket;
using wowwowwow.UserCommands;

namespace wowwowwow
{
    public class DataManager
    {
        public const string keywordsFile = "keywords.json";
        public const string configFile = "config.json";
        public static Dictionary<string, string> keywords = new Dictionary<string, string>();
        public static Dictionary<string, dynamic> config = new Dictionary<string, dynamic>();

        public async Task LoadData()
        {
            await Task.Run(() => keywords = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(keywordsFile)));
            await Task.Run(() => config = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(File.ReadAllText(configFile)));
        }

        public async Task SaveData()
        {
            await File.WriteAllTextAsync(keywordsFile, JsonSerializer.Serialize(keywords));
            await File.WriteAllTextAsync(configFile, JsonSerializer.Serialize(config));
        }

    }
}
