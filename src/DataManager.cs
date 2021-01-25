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

        public static Dictionary<string, string> keywords = new Dictionary<string, string>();

        public async Task LoadData()
        {
            await Task.Run(() => keywords = JsonSerializer.Deserialize<Dictionary<String, String>>(File.ReadAllText("keywords.json")));
        }

        public async Task SaveData()
        {
            await File.WriteAllTextAsync("keywords.json", JsonSerializer.Serialize(keywords));
        }

    }
}
