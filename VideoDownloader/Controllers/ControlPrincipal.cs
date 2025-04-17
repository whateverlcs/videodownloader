using System.IO;
using System.Net.Http;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;

namespace VideoDownloader.Controllers
{
    public class ControlPrincipal
    {
        private ControlLogs clog = new ControlLogs();

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
    }
}