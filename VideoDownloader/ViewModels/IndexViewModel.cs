using Caliburn.Micro;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using VideoDownloader.Controllers;
using VideoDownloader.Models;

namespace VideoDownloader.ViewModels
{
    public class IndexViewModel : Screen
    {
        private bool _loading;

        public bool Loading
        {
            get { return _loading; }
            set
            {
                _loading = value;
                NotifyOfPropertyChange(() => Loading);
            }
        }

        private string _textLoading;

        public string TextLoading
        {
            get { return _textLoading; }
            set
            {
                _textLoading = value;
                NotifyOfPropertyChange(() => TextLoading);
            }
        }

        private bool _exibirBotaoDownloadX = true;

        public bool ExibirBotaoDownloadX
        {
            get { return _exibirBotaoDownloadX; }
            set
            {
                _exibirBotaoDownloadX = value;
                NotifyOfPropertyChange(() => ExibirBotaoDownloadX);
            }
        }

        private bool _exibirBotaoDownloadFFMPEG = true;

        public bool ExibirBotaoDownloadFFMPEG
        {
            get { return _exibirBotaoDownloadFFMPEG; }
            set
            {
                _exibirBotaoDownloadFFMPEG = value;
                NotifyOfPropertyChange(() => ExibirBotaoDownloadFFMPEG);
            }
        }

        private string _txtLinkX;

        public string TxtLinkX
        {
            get { return _txtLinkX; }
            set
            {
                _txtLinkX = value;

                var listLinks = _txtLinkX.Split('\n', ';', ',').ToList();

                TxtCountDownloads = $"{listLinks.Count} links to download detected";

                NotifyOfPropertyChange(() => TxtLinkX);
            }
        }

        private string _txtLinkMainUrl;

        public string TxtLinkMainUrl
        {
            get { return _txtLinkMainUrl; }
            set
            {
                _txtLinkMainUrl = value;

                NotifyOfPropertyChange(() => TxtLinkMainUrl);
            }
        }

        private string _txtCountDownloads;

        public string TxtCountDownloads
        {
            get { return _txtCountDownloads; }
            set
            {
                _txtCountDownloads = value;
                NotifyOfPropertyChange(() => TxtCountDownloads);
            }
        }

        private bool _rbVideo = true;

        public bool RbVideo
        {
            get { return _rbVideo; }
            set
            {
                _rbVideo = value;

                ExibirBotaoDownloadX = _rbVideo;
                ExibirBotaoDownloadFFMPEG = _rbVideo;

                NotifyOfPropertyChange(() => RbVideo);
            }
        }

        private bool _rbAudio;

        public bool RbAudio
        {
            get { return _rbAudio; }
            set
            {
                _rbAudio = value;

                ExibirBotaoDownloadX = !_rbAudio;
                ExibirBotaoDownloadFFMPEG = !_rbAudio;

                NotifyOfPropertyChange(() => RbAudio);
            }
        }

        private ControlLogs clog = new ControlLogs();

        private ControlPrincipal cp = new ControlPrincipal();

        public IndexViewModel()
        {
            TextLoading = "DOWNLOADING (-/-)";
        }

        public void DownloadYoutubeVideoOrAudio()
        {
            if (string.IsNullOrEmpty(TxtLinkX))
            {
                MessageBox.Show("Please enter a link from YouTube to proceed with the download.", "Please enter a link", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var listLinks = TxtLinkX.Split('\n', ';', ',').ToList();

            if (listLinks.Any(x => !x.Contains("youtube.com/watch")))
            {
                MessageBox.Show("Please insert a valid YouTube video link to download.", "Please enter a valid link", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (SelectSaveDirectory())
            {
                Loading = true;
                TextLoading = $"DOWNLOADING (0/{listLinks.Count})";

                Task.Run(() => DownloadYoutubeVideoOrAudioThread(listLinks)).ConfigureAwait(false);
            }
        }

        public void DownloadXVideoOrAudio()
        {
            if (string.IsNullOrEmpty(TxtLinkX))
            {
                MessageBox.Show("Please enter a link from X to proceed with the download.", "Please enter a link", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var listLinks = TxtLinkX.Split('\n', ';', ',').ToList();

            if (listLinks.Any(x => !x.Contains("x.com/")))
            {
                MessageBox.Show("Please insert a valid X video link to download.", "Please enter a valid link", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (SelectSaveDirectory())
            {
                Loading = true;
                TextLoading = $"DOWNLOADING (0/{listLinks.Count})";

                Task.Run(() => DownloadXVideoOrAudioThread(listLinks)).ConfigureAwait(false);
            }
        }

        public void DownloadFFMPEGVideo()
        {
            if (string.IsNullOrEmpty(TxtLinkMainUrl))
            {
                MessageBox.Show("Please enter the page URL where the video is located to proceed with the download.", "Please enter the page URL", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (string.IsNullOrEmpty(TxtLinkX))
            {
                MessageBox.Show("Please enter a link via FFMPEG to proceed with the download.", "Please enter a link", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string ffmpegPattern = @"^(?:\.\\|\./)?ffmpeg\s+-i\s+""([^""]+)""\s+-c\s+copy\s+""([^""]+\.mp4)""$";

            var listLinks = TxtLinkX.Split('\n', ';', ',')
                .Select(c => c.Trim())
                .Where(c => !string.IsNullOrEmpty(c))
                .Where(c => Regex.IsMatch(c, ffmpegPattern, RegexOptions.IgnoreCase))
                .ToList();

            if (listLinks.Count == 0)
            {
                string example = @".\ffmpeg -i ""https://example.com/video.m3u8"" -c copy ""myvideo.mp4""";
                MessageBox.Show($"No valid FFmpeg commands found.\n\nExample:\n{example}",
                               "Invalid Format",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
                return;
            }

            if (SelectSaveDirectory())
            {
                Loading = true;
                TextLoading = $"DOWNLOADING (0/{listLinks.Count})";

                Task.Run(() => DownloadFFMPEGVideoThread(listLinks)).ConfigureAwait(false);
            }
        }

        public bool SelectSaveDirectory()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();

            var path = App.GetSetting("pathLastSelected");

            dialog.InitialDirectory = !string.IsNullOrEmpty(path) ? path : @"C:\";
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Global.DirectorySaveDownload = dialog.FileName + "\\";
                cp.AtualizarCaminhoSalvamento(Global.DirectorySaveDownload);
                return true;
            }

            return false;
        }

        public async Task DownloadYoutubeVideoOrAudioThread(List<string> listLinks)
        {
            try
            {
                var listItensNotDownloaded = new List<string>();

                int i = 1;

                foreach (var link in listLinks)
                {
                    if (!await cp.DownloadAudioOrVideoFromYoutube(link, RbVideo ? "Video" : "Audio"))
                    {
                        listItensNotDownloaded.Add(link);
                    }

                    TextLoading = $"DOWNLOADING ({i}/{listLinks.Count})";
                    i++;
                }

                if (listItensNotDownloaded.Count == 0)
                {
                    MessageBox.Show("Download Completed.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("An error occurred while downloading the inserted video/audio. The links that were not downloaded will be saved in a text file at the save location. Please contact support if the issue continues", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    clog.GenerateLinksNotSucceded(listItensNotDownloaded);
                }
            }
            catch (Exception e)
            {
                clog.LogException(e.ToString(), "DownloadYoutubeVideoOrAudioThread()");
                MessageBox.Show("An error occurred while downloading the inserted video/audio. Please try again, if the error persists, contact support.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Loading = false;
                TextLoading = "DOWNLOADING (-/-)";
                TxtLinkX = "";
                TxtLinkMainUrl = "";
                TxtCountDownloads = "0 links to download detected";
                RbVideo = true;
                RbAudio = false;
            }
        }

        public async Task DownloadXVideoOrAudioThread(List<string> listLinks)
        {
            try
            {
                var listItensNotDownloaded = new List<string>();

                int i = 1;

                foreach (var link in listLinks)
                {
                    if (!await cp.DownloadAudioOrVideoFromX(link))
                    {
                        listItensNotDownloaded.Add(link);
                    }

                    TextLoading = $"DOWNLOADING ({i}/{listLinks.Count})";
                    i++;
                }

                if (listItensNotDownloaded.Count == 0)
                {
                    MessageBox.Show("Download Completed.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("An error occurred while downloading the inserted video/audio. The links that were not downloaded will be saved in a text file at the save location. Please contact support if the issue continues", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    clog.GenerateLinksNotSucceded(listItensNotDownloaded);
                }
            }
            catch (Exception e)
            {
                clog.LogException(e.ToString(), "DownloadXVideoOrAudioThread()");
                MessageBox.Show("An error occurred while downloading the inserted video/audio. Please try again, if the error persists, contact support.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Loading = false;
                TextLoading = "DOWNLOADING (-/-)";
                TxtLinkX = "";
                TxtLinkMainUrl = "";
                TxtCountDownloads = "0 links to download detected";
                RbVideo = true;
                RbAudio = false;
            }
        }

        public async Task DownloadFFMPEGVideoThread(List<string> listLinks)
        {
            try
            {
                var listItensNotDownloaded = new List<string>();
                var downloadTasks = new Dictionary<Task<DownloadResult>, string>();

                foreach (var link in listLinks)
                {
                    var task = cp.DownloadVideoViaFFMPEG(link, TxtLinkMainUrl);
                    downloadTasks[task] = link;
                }

                int completedCount = 0;
                int totalCount = listLinks.Count;

                while (downloadTasks.Count > 0)
                {
                    var completedTask = await Task.WhenAny(downloadTasks.Keys);

                    if (!downloadTasks.TryGetValue(completedTask, out string link))
                    {
                        downloadTasks.Remove(completedTask);
                        continue;
                    }

                    var result = await completedTask;

                    downloadTasks.Remove(completedTask);

                    completedCount++;

                    Application.Current?.Dispatcher?.Invoke(() =>
                    {
                        TextLoading = $"DOWNLOADING ({completedCount}/{totalCount})";
                    });

                    if (!result.Success)
                    {
                        listItensNotDownloaded.Add(result.Link);
                    }

                    if (completedCount % 10 == 0)
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
                }

                if (listItensNotDownloaded.Count == 0)
                {
                    MessageBox.Show("Download Completed.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    string message = $"An error occurred while downloading {listItensNotDownloaded.Count} video(s). " +
                                   $"The links that were not downloaded will be saved in a text file at the save location. " +
                                   $"Please contact support if the issue continues";

                    MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    clog.GenerateLinksNotSucceded(listItensNotDownloaded);
                }
            }
            catch (Exception e)
            {
                clog.LogException(e.ToString(), "DownloadFFMPEGVideoThread()");
                MessageBox.Show("An error occurred while downloading the inserted video. Please try again, if the error persists, contact support.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Loading = false;
                TextLoading = "DOWNLOADING (-/-)";
                TxtLinkX = "";
                TxtLinkMainUrl = "";
                TxtCountDownloads = "0 links to download detected";
                RbVideo = true;
                RbAudio = false;
            }
        }
    }
}