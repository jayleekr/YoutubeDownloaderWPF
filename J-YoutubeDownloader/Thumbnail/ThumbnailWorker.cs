using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.IO;
using System.Threading;

namespace J_YoutubeDownloader.Thumbnail
{
    public class ThumbnailChangeWorker 
    {

        public static Thread WorkerThread;
        public bool IsWorkerRunning = true;
        public int DurationMillisecond = 3000;
        private string _RecentFileName;

        public ThumbnailChangeWorker()
        {
            WorkerThread = new Thread(new ThreadStart(WorkerLoop));
            WorkerThread.Start();
        }

        public void WorkerLoop()
        {
            bool IsThumbnailExist = false;
            while (IsWorkerRunning)
            {
                Thread.Sleep(DurationMillisecond);

                IsThumbnailExist = UpdateRecentThumbnail();

                if (IsThumbnailExist == false)
                {
                    continue;
                }

                // Thumbnail Update
            }
        }

        public bool UpdateRecentThumbnail()
        {
            try
            {
                DirectoryInfo directory = new DirectoryInfo(Environment.CurrentDirectory);
                FileInfo[] files = directory.GetFiles("*.jpg");

                if (files.Length > 0)
                {
                    List<FileInfo> list = new List<FileInfo>(files);
                    list = list.OrderBy(o => o.LastWriteTimeUtc).ToList();

                    foreach (FileInfo f in list)
                    {
                        try
                        {
                            if (f != list.Last())
                            {
                                f.Delete();
                            }
                        }
                        catch{ }
                    }

                    _RecentFileName = list.Last().FullName;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

        }

        public void Stop()
        {

        }

    }
}
