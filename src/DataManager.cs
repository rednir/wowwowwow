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
        public const string keywordsFileName = "keywords.json";
        public const string configFileName = "config.json";
        public static Dictionary<string, string> keywords = new Dictionary<string, string>();
        public static Dictionary<string, dynamic> config = new Dictionary<string, dynamic>();

        public async Task LoadData()
        {
            await Task.Run(() => keywords = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(keywordsFileName)));
            await Task.Run(() => config = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(File.ReadAllText(configFileName)));

            config["ignore"] = JsonElementToList<ulong>(config["ignore"]);
        }

        public async Task SaveData()
        {
            await File.WriteAllTextAsync(keywordsFileName, JsonSerializer.Serialize(keywords));
            await File.WriteAllTextAsync(configFileName, JsonSerializer.Serialize(config));
        }

        public List<T> JsonElementToList<T>(JsonElement element)
        {
            return JsonSerializer.Deserialize<List<T>>(element.GetRawText());
        }

        public async Task SyncData()
        {
            await SaveData();
            await LoadData();
        }

    }
}
