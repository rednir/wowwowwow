using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        AudioOutStream audioOutStream;
        private static Queue<string[]> audioQueue = new Queue<string[]>();
        private static IVoiceChannel activeVoiceChannel;

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


        private async Task Continue(IVoiceChannel newActiveVoiceChannel = null)
        {
            try
            {
                if (await Download())
                {
                    await Join(activeVoiceChannel == null ? newActiveVoiceChannel : activeVoiceChannel);
                    await Play();
                }
            }
            catch (Exception ex)
            {
                if (activeVoiceChannel == null)
                {
                    // the bot probably left intentionally
                    return;
                }
                await verboseManager.SendEmbedMessage(embedMessage.Error($"\nCould not execute the command, the following error was returned:```{ex}```"));
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
                    await audioOutStream.FlushAsync();
                }
            }

            // once playback has ended, download and play next in queue
            Console.WriteLine("playback ended, continuing\n-\n--\n---\n----");
            _ = Continue();
        }


        private async Task<bool> Download()
        {
            string toPlay = string.Empty;
            try
            {
                toPlay = audioQueue.Peek()[0];
                audioQueue.Dequeue();
            }
            catch (InvalidOperationException)
            {
                await Leave(string.Empty);
                return false;
            }

            RestUserMessage downloadingMessage = await verboseManager.SendEmbedMessage(embedMessage.Progress("Downloading..."));
            string output = YoutubeDl(toPlay, toPlay.StartsWith("http") ? true : false);
            await downloadingMessage.DeleteAsync();
            if (output != string.Empty)
            {
                await verboseManager.SendEmbedMessage(embedMessage.Warning($"An error was returned when downloading the audio file:```{output}```This may or may not be fatal."));
            }
            return true;
        }


        // todo: this shouldnt need a bool return after error handle rework
        public async Task<bool> Join(IVoiceChannel vc = null)
        {
            if (vc == null)
            {
                await verboseManager.SendEmbedMessage(embedMessage.Error("You must join a voice channel first before summoning the bot"));
                return false;
            }

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

            await Task.Delay(100);
            activeVoiceChannel = vc;
            audioClient = await vc.ConnectAsync();

            return true;
        }


        public async Task Leave(string message = "then i'll leave.... Sadge")
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
            await audioClient.StopAsync();
        }


        // todo: this suffers from baed error handling
        public async Task Add(string toPlay, SocketUser user)
        {
            audioQueue.Enqueue(new string[] { toPlay, user.Id.ToString() });
            await verboseManager.SendEmbedMessage(embedMessage.Info($"At the request of {user.Mention}, the following was placed in the {(audioQueue.Count == 1 ? "**next**" : $"**number {audioQueue.Count}**")} spot in the queue:\n\n{(toPlay.StartsWith("http") ? toPlay : $"Search for: `{toPlay}`")}"));
            if (audioOutStream == null)
            {
                // if nothing is playing, then download and play
                _ = Continue((user as IGuildUser).VoiceChannel);
            }
        }


        public async Task Skip()
        {
            await verboseManager.SendEmbedMessage(embedMessage.Warning("This doesn't do anything yet"));
        }


    }

}

