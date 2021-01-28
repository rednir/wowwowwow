using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Text.Json;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Audio;


// THIS CODE IS BAD \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
namespace wowwowwow.UserCommands
{
    public class Voice : UserCommands
    {
        public static IAudioClient audioClient;
        AudioOutStream audioOutStream = null;
        private static Queue<object[]> audioQueue = new Queue<object[]>();    // [0] should be string, [1] should be SocketUser, [2] should be IVoiceChannel
        private static IVoiceChannel activeVoiceChannel = null;

        private static bool isCurrentlyDownloading = false;
        public static string downloadFilePath = "/tmp/wowwowwow-vc.opus";
        public static string downloadMetadataPath = $"{downloadFilePath}.info.json";


        private class Metadata
        {
            public string title { get; set; }
            public string webpage_url { get; set; }
            public string description { get; set; }
            public List<MetadataThumbnails> thumbnails { get; set; }

        }

        private class MetadataThumbnails
        {
            public string url { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public string resolution { get; set; }
            public string id { get; set; }
        }


        private Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
        }

        private string YoutubeDl(string input, bool isUrl)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "bash";
                if (isUrl)
                {
                    process.StartInfo.Arguments = $"-c \" rm \'{downloadFilePath}\'; youtube-dl \'{input}\' --print-json -f worst --no-playlist --audio-format \'opus\' -x -o {downloadFilePath} > {downloadMetadataPath}\"";
                }
                else
                {
                    process.StartInfo.Arguments = $"-c \" rm \'{downloadFilePath}\'; youtube-dl ytsearch:\'{input}\' --print-json -f worst --no-playlist --audio-format \'opus\' -x -o {downloadFilePath} > {downloadMetadataPath}\"";
                }
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardError = true;
                process.Start();

                StreamReader reader = process.StandardError;
                string output = reader.ReadLine();

                process.WaitForExit();
                if (process.ExitCode == 0)
                {
                    return string.Empty;
                }
                else
                {
                    return output;
                }
            }
        }


        private async Task LeaveBeforeReJoin()
        {
            try
            {
                if (audioClient.ConnectionState == ConnectionState.Connecting || audioClient.ConnectionState == ConnectionState.Connected)
                {
                    // disconnect first before re-connecting, reduces chance of trying to connect too early
                    await audioClient.StopAsync();
                }
            }
            catch
            {
                // exception will be thrown here if the audio client doesn't exist. this doesn't matter.
            }
        }


        private async Task Continue()
        {

            try
            {
                activeVoiceChannel = (audioQueue.Peek()[2] as IVoiceChannel);
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is FormatException)    // i dont think i need FormatException anymore
            {
                Console.WriteLine(ex); // temp
                await Leave(string.Empty);
                return;
            }

            if (await Download() && await Join())
            {
                await Play();
            }

        }


        private async Task Play()
        {
            await NowPlaying();
            audioQueue.Dequeue();

            using (var ffmpeg = CreateStream(downloadFilePath))
            using (audioOutStream = audioClient.CreatePCMStream(AudioApplication.Music))
            {
                try
                {
                    await ffmpeg.StandardOutput.BaseStream.CopyToAsync(audioOutStream);
                }
                finally
                {
                    audioOutStream = null;
                }
            }

            // once playback has ended, download and play next in queue
            _ = Continue();
        }


        private async Task<bool> Download()
        {
            // this is all in a try statement because no matter what happens, i need isCurrentlyDownloading to be accurate
            try
            {
                isCurrentlyDownloading = true;
                string toPlay = string.Empty;
                try
                {
                    toPlay = (audioQueue.Peek()[0] as string);
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine(ex); // temp
                    await Leave(string.Empty);
                    return false;
                }
                await LeaveBeforeReJoin();

                RestUserMessage downloadingMessage = await verboseManager.SendEmbedMessage(embedMessage.Progress("Downloading..."));
                string output = YoutubeDl(toPlay, toPlay.StartsWith("http") ? true : false);
                await downloadingMessage.DeleteAsync();
                if (output != string.Empty)
                {
                    await verboseManager.SendEmbedMessage(embedMessage.Warning($"An error was returned when downloading the audio file:```{output}```This may or may not be fatal."));
                }
                return true;
            }
            finally
            {
                isCurrentlyDownloading = false;
            }
        }


        // todo: this shouldnt need a bool return after error handle rework
        private async Task<bool> Join()
        {
            audioClient = await activeVoiceChannel.ConnectAsync();
            return true;
        }


        public async Task Leave(string message = "The bot has been disconnected from the voice channel and the queue has been cleared.")
        {
            if (activeVoiceChannel == null)
            {
                await verboseManager.SendEmbedMessage(embedMessage.Warning("This command does nothing if I'm not in a voice call."));
                return;
            }
            if (message != string.Empty)
            {
                await verboseManager.SendEmbedMessage(embedMessage.Info(message));
            }
            activeVoiceChannel = null;
            audioOutStream = null;
            audioQueue = new Queue<object[]>();
            await audioClient.StopAsync();
        }


        public async Task NowPlaying()
        {
            const int maxDescriptionLength = 300;
            try
            {
                var metadata = JsonSerializer.Deserialize<Metadata>(File.ReadAllText(downloadMetadataPath));
                Console.WriteLine(metadata.description);
                // the long ternary operator here just shrinks the description if it's over maxDescriptionLength
                await verboseManager.SendEmbedMessage(embedMessage.NowPlaying($"▶️  Now playing: \n{(metadata.title)} ", metadata.webpage_url, $"{(metadata.description.Length > maxDescriptionLength ? $"{metadata.description.Substring(0, maxDescriptionLength - 3)}..." : metadata.description )}", metadata.thumbnails[2].url));
            }
            catch (Exception ex)
            {
                await verboseManager.SendEmbedMessage(embedMessage.Error($"\nA command was specified with a missing option.{ex}"));
            }
        }


        // todo: this suffers from baed error handling
        public async Task Add(string toPlay, SocketUser user)
        {
            IGuildUser guildUser = (user as IGuildUser);
            if (guildUser.VoiceChannel == null)
            {
                await verboseManager.SendEmbedMessage(embedMessage.Error("You must join a voice channel first before summoning the bot"));
                return;
            }

            audioQueue.Enqueue(new object[] { toPlay, user, (user as IGuildUser).VoiceChannel });
            await verboseManager.SendEmbedMessage(embedMessage.Info($"At the request of {user.Mention}, the following was placed in the {(audioQueue.Count == 1 ? "**next**" : $"**number {audioQueue.Count}**")} spot in the queue:\n\n{(toPlay.StartsWith("http") ? toPlay : $"Search for: `{toPlay}`")}"));
            if (activeVoiceChannel == null && !isCurrentlyDownloading)
            {
                // if nothing is playing, then download and play
                activeVoiceChannel = guildUser.VoiceChannel;
                await Continue();
            }
            else
            {
                activeVoiceChannel = guildUser.VoiceChannel;
            }
        }


        public async Task List()
        {
            if (audioQueue.Count == 0)
            {
                await verboseManager.SendEmbedMessage(embedMessage.Info("Nothing in the queue yet!\nUse `!wow vc add <url/search>` to add audio."));
                return;
            }
            object[][] arrayOfAudioQueue = audioQueue.ToArray();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < arrayOfAudioQueue.Length; i++)
            {
                string currentItem = (arrayOfAudioQueue[i][0] as string);
                string currentItemFormatted = (currentItem.StartsWith("http") ? currentItem : $"Search for: `{currentItem}`");
                if (i == 0)
                {
                    sb.Append($"**Now Playing:**\n {currentItem}\n\n");
                    continue;
                }
                sb.Append($"  {i + 1}) {currentItem}\n");
            }
            await verboseManager.SendEmbedMessage(embedMessage.Info(sb.ToString()));
        }


        public async Task Skip()
        {
            if (isCurrentlyDownloading)
            {
                await verboseManager.SendEmbedMessage(embedMessage.Error("For safety, please wait until downloading has finished before skipping"));
                return;
            }
            await Continue();
        }


    }

}

