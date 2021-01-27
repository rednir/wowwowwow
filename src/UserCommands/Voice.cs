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


        private async Task SendVideo(string path)
        {
            using (var ffmpeg = CreateStream(path))
            using (var stream = audioClient.CreatePCMStream(AudioApplication.Music))
            {
                try { await ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream); }
                finally { await stream.FlushAsync(); }
            }
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
                    await Task.Delay(100);
                }
            }
            catch {}
            
            audioClient = await vc.ConnectAsync();
            return true;
        }

        public async Task Leave()
        {
            await verboseManager.SendEmbedMessage(embedMessage.Info($"then i'll leave.... Sadge"));
            await audioClient.StopAsync();
        }

        // todo: this suffers from baed error handling
        public async Task Add(string url)
        {
            RestUserMessage downloadingMessage = await verboseManager.SendEmbedMessage(embedMessage.Progress("Downloading..."));
            string output = YoutubeDl(url, url.StartsWith("http") ? true : false);
            await downloadingMessage.DeleteAsync();
            if (output != string.Empty)
            {
                await verboseManager.SendEmbedMessage(embedMessage.Warning($"An error was returned when downloading the audio file:```{output}```This may or may not be fatal."));
                return;
            }
            await verboseManager.SendEmbedMessage(embedMessage.Info($"The following audio was placed in the number {0.ToString()} spot in the queue:\n{url}"));
            await SendVideo(downloadFilePath);

        }


        public async Task Skip()
        {
            await verboseManager.SendEmbedMessage(embedMessage.Warning("This doesn't do anything yet"));
        }


    }

}

