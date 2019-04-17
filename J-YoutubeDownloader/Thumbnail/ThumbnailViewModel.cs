using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace J_YoutubeDownloader.Thumbnail
{
    public class ThumbnailPageViewModel : ChangedNotificator
    {
        #region Binding Properties
        private BitmapImage thumbnailImage;
        public BitmapImage ThumbnailImage
        {
            get { return thumbnailImage; }
            set { thumbnailImage = value; OnPropertyChanged("ThumbnailImage"); }
        }

        private Visibility labelVisibility;
        public Visibility LabelVisibility
        {
            get
            {
                return labelVisibility;
            }
            set
            {
                labelVisibility = value; OnPropertyChanged("LabelVisibility"); 
            }
        }
        #endregion

        public ThumbnailPageViewModel()
        {

        }

        public void UpdateSnapthotByFilename(string fileName)
        {
            System.Drawing.Bitmap bitmap = null;

            try
            {
                bitmap = new System.Drawing.Bitmap(fileName);
            }
            catch
            {
                bitmap = null;
            }

            if (bitmap == null)
            {
                this.ThumbnailImage = ThumbnailPage.ThumbnailPage.EmptyImage;
            }
            else
            {
                this.ThumbnailImage = ToBitmapImage(bitmap);
            }
        }


        public static BitmapImage ToBitmapImage(System.Drawing.Bitmap bitmap)
        {
            BitmapImage bitmapImage = new BitmapImage();

            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }

            return bitmapImage;
        }
    }
}
