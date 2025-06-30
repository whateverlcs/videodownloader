using System.IO;

namespace VideoDownloader.Controllers
{
    public class ControlLogs
    {
        public void LogException(string exception, string localException)
        {
            string err = $"{DateTime.Now} | Local: {localException} | Exception: {exception}\n\n";
            File.AppendAllText(@$"{Directory.GetCurrentDirectory()}\Errors.txt", err);
        }

        public void GenerateLinksNotSucceded(List<string> listLinks)
        {
            string txtFile = Global.DirectorySaveDownload + "\\links not downloaded.txt";

            File.WriteAllText(txtFile, string.Join("\n", listLinks));
        }
    }
}