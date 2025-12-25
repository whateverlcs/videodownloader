using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using VideoDownloader.Models;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;

namespace VideoDownloader.Controllers
{
    public class ControlPrincipal
    {
        private ControlLogs clog = new ControlLogs();

        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(3, 3);

        public async Task<bool> DownloadAudioOrVideoFromYoutube(string url, string type)
        {
            try
            {
                var ytdl = new YoutubeDL();
                ytdl.YoutubeDLPath = @"./Utils/yt-dlp";
                ytdl.FFmpegPath = @"./Utils/ffmpeg";
                ytdl.OutputFolder = Global.DirectorySaveDownload;
                var res = type.Equals("Video") ? await ytdl.RunVideoDownload(url) : await ytdl.RunAudioDownload(url, AudioConversionFormat.Mp3);
                return true;
            }
            catch (Exception e)
            {
                clog.LogException(e.ToString(), $"DownloadAudioOrVideoFromYoutube(string {url}, string {(type.Equals("Video") ? "Video" : "Audio")})");
                return false;
            }
        }

        public async Task<bool> DownloadAudioOrVideoFromX(string url)
        {
            try
            {
                string fxTwitterUrl = url
                    .Replace("twitter.com", "d.fxtwitter.com")
                    .Replace("x.com", "d.fxtwitter.com");

                string pathFileDownloaded = $"{Global.DirectorySaveDownload}twittervid.com_{GenerateRandomCharacters(10)}.mp4";

                using (HttpClient client = new HttpClient() { Timeout = TimeSpan.FromMinutes(5) })
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");

                    HttpResponseMessage response = await client.GetAsync(fxTwitterUrl + ".mp4");
                    response.EnsureSuccessStatusCode();

                    Uri downloadUri = response.RequestMessage.RequestUri;

                    if (downloadUri != null && downloadUri.ToString().Contains("video.twimg.com"))
                    {
                        byte[] videoData = await client.GetByteArrayAsync(downloadUri);
                        await File.WriteAllBytesAsync(pathFileDownloaded, videoData);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                clog.LogException(e.ToString(), $"DownloadAudioOrVideoFromX(string {url}");
                return false;
            }
        }

        public async Task<DownloadResult> DownloadVideoViaFFMPEG(string url)
        {
            await _semaphore.WaitAsync();

            try
            {
                (string urlLink, string codigo) = ExtractCodeFromUrl(url);
                string outputPath = $"{Global.DirectorySaveDownload}{codigo}";
                string ffmpegPath = @"./Utils/ffmpeg.exe";

                string arguments = $"-i \"{urlLink}\" -c copy \"{outputPath}\"";

                using (var process = new Process())
                using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(60)))
                {
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = ffmpegPath,
                        Arguments = arguments,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        WorkingDirectory = Directory.GetCurrentDirectory()
                    };

                    process.Start();

                    var waitTask = process.WaitForExitAsync(cts.Token);
                    var readErrorTask = process.StandardError.ReadToEndAsync();

                    await Task.WhenAll(waitTask, readErrorTask);

                    bool success = process.ExitCode == 0 && File.Exists(outputPath);

                    if (!success)
                    {
                        string error = await readErrorTask;

                        if (File.Exists(outputPath))
                        {
                            try { File.Delete(outputPath); } catch { }
                        }
                    }

                    return new DownloadResult
                    {
                        Success = success,
                        Link = url,
                        FilePath = outputPath
                    };
                }
            }
            catch (Exception ex)
            {
                clog.LogException(ex.ToString(), $"DownloadVideoViaFFMPEG(string {url}");

                return new DownloadResult
                {
                    Success = false,
                    Link = url,
                    FilePath = string.Empty
                };
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private (string, string) ExtractCodeFromUrl(string url)
        {
            var splittedUrl = url.Split('"');

            string urlLink = splittedUrl[1];
            string codigo = splittedUrl[3];
            codigo = codigo.Substring(codigo.LastIndexOf('\\') + 1);

            return (urlLink, codigo);
        }

        public string GenerateRandomCharacters(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var result = new char[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }

            return new string(result);
        }

        public void AtualizarCaminhoSalvamento(string path)
        {
            string json = File.ReadAllText("appsettings.json");

            var jObject = JObject.Parse(json);

            jObject.SelectToken("Configuration.pathLastSelected")!.Replace(path);

            File.WriteAllText("appsettings.json", jObject.ToString());
        }
    }
}