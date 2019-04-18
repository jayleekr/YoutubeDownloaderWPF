using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.IO;
using System.Threading;

namespace J_YoutubeDownloader.Database
{
    public class SetupDB 
    {
        // Sqlite 추가 예정..
        // 귀찮다 천천히.. 
        private string dbFilename = "setup.txt";
        public string SavePath = "";
        public SetupDB()
        {
            Load();
        }

        public void Load()
        {
            string savePath = "";
            FileInfo fileInfo = new FileInfo(dbFilename);
            if (fileInfo.Exists == false)
            {
                fileInfo.Create();
                savePath = Environment.CurrentDirectory;
            }
            else
            {
                using (StreamReader stream = new StreamReader(dbFilename))
                {
                    savePath = stream.ReadLine();
                }
            }

            SavePath = savePath;
        }

        public void Save(string path)
        {
            FileInfo file = new FileInfo(dbFilename);
            if (file.Exists == true)
            {
                file.Delete();
            }

            using (StreamWriter streamWriter = new StreamWriter(dbFilename))
            {
                streamWriter.Write(path);
            }
        }
    }
}
