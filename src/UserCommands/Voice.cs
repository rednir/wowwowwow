using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text;
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
        private static Queue<string[]> audioQueue = new Queue<string[]>();
        private static IVoiceChannel activeVoiceChannel = null;

        private static bool IsCurrentlyDownloading = false;
        public static string downloadFilePath = "/tmp/wowwowwow-vc.m4a";


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
                    process.StartInfo.Arguments = $"-c \" rm \'{downloadFilePath}\'; youtube-dl \'{input}\' --no-playlist --audio-quality 7 --audio-format \'m4a\' -x -o {downloadFilePath}\"";
                }
                else
                {
                    process.StartInfo.Arguments = $"-c \" rm \'{downloadFilePath}\'; youtube-dl ytsearch:\'{input}\' --no-playlist --audio-quality 7 --audio-format \'m4a\' -x -o {downloadFilePath}\"";
                }
                process.StartInfo.UseShellExecute = false;
                //process.StartInfo.RedirectStandardOutput = true;
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
                Console.Write($"aaaaaaaaaaaaaaaaaaaaa LEAVING IN CONTINUE() {audioQueue.Count} {Program._client.GetUser(Convert.ToUInt64(audioQueue.Peek()[1]))}\n");
                SocketUser user = Program._client.GetUser(Convert.ToUInt64(audioQueue.Peek()[1]));
                //activeVoiceChannel = (user as IGuildUser).VoiceChannel;
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is FormatException)
            {

                await Leave(string.Empty);
                return;
            }
            Console.WriteLine($"---- activeVoiceChannel = {activeVoiceChannel}\n--\n");
            if (await Download())
            {
                await Join(activeVoiceChannel);
                await Play();
            }

        }


        private async Task Play()
        {
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
                    audioQueue.Dequeue();
                }
            }

            // once playback has ended, download and play next in queue
            _ = Continue();
        }


        private async Task<bool> Download()
        {
            // this is all in a try statement because no matter what happens, i need IsCurrentlyDownloading to be accurate
            try
            {
                IsCurrentlyDownloading = true;
                string toPlay = string.Empty;
                try
                {
                    toPlay = audioQueue.Peek()[0];
                }
                catch (InvalidOperationException)
                {
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
                IsCurrentlyDownloading = false;
            }
        }

        // doesnt work if called from command, fix this or rework.
        // todo: this shouldnt need a bool return after error handle rework
        public async Task<bool> Join(IVoiceChannel vc = null)
        {
            Console.WriteLine($"vc = {vc}\nactivevc= {activeVoiceChannel}\n--\n");
            if (vc == null)
            {
                await verboseManager.SendEmbedMessage(embedMessage.Error("You must join a voice channel first before summoning the bot"));
                return false;
            }
            //await Task.Delay(500);
            activeVoiceChannel = vc;
            Console.WriteLine("AUDIO CLIENT CREATED\n--\n");
            audioClient = await vc.ConnectAsync();

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
            audioQueue = new Queue<string[]>();
            await audioClient.StopAsync();
        }


        // todo: this suffers from baed error handling
        public async Task Add(string toPlay, SocketUser user)
        {
            audioQueue.Enqueue(new string[] { toPlay, user.Id.ToString() });
            await verboseManager.SendEmbedMessage(embedMessage.Info($"At the request of {user.Mention}, the following was placed in the {(audioQueue.Count == 1 ? "**next**" : $"**number {audioQueue.Count}**")} spot in the queue:\n\n{(toPlay.StartsWith("http") ? toPlay : $"Search for: `{toPlay}`")}"));
            if (activeVoiceChannel == null)
            {
                // if nothing is playing, then download and play
                activeVoiceChannel = (user as IGuildUser).VoiceChannel;
                Console.WriteLine($"activeVoiceChannel = {activeVoiceChannel}\n--\n");
                await Continue();
            }
            else
            {
                activeVoiceChannel = (user as IGuildUser).VoiceChannel;
            }
        }


        public async Task Skip()
        {
            if (IsCurrentlyDownloading)
            {
                await verboseManager.SendEmbedMessage(embedMessage.Error("For safety, please wait until downloading has finished before skipping"));
                return;
            }
            await Continue();
        }


    }

}

