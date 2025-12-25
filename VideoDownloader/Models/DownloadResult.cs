using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoDownloader.Models
{
    public class DownloadResult
    {
        public bool Success { get; set; }
        public string Link { get; set; }
        public string FilePath { get; set; }
    }
}
