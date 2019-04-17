using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace J_YoutubeDownloader.Control
{
    public class YoutubeInfo
    {
        public SortedDictionary<int, string> QualityDictionary; // <FormatNumber, Note>
        public string Url;
        public string TitleName;

        public YoutubeInfo()
        {
            QualityDictionary = new SortedDictionary<int, string>();
        }

        public int GetQualityNumberByNote(string note)
        {
            var a = from pair in QualityDictionary
                    where pair.Value == note
                    select pair;
            return a.First().Key;
        }
    }
}
