using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using J_YoutubeDownloader.Thumbnail;

namespace J_YoutubeDownloader.ThumbnailPage
{
    /// <summary>
    /// Thumbnail.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ThumbnailPage : Page
    {
        public static ThumbnailPageViewModel ViewModel = new ThumbnailPageViewModel();
        public static BitmapImage EmptyImage = new BitmapImage(new Uri("pack://application:,,,/Images/YouTube.png"));

        public static ThumbnailPage Instance;
        public ThumbnailPage()
        {
            Instance = this;
            DataContext = ViewModel;
            ViewModel.UpdateSnapthotByFilename("");
            InitializeComponent();

            LabelVisibility = Visibility.Hidden;
        }
        
        public Visibility LabelVisibility
        {
            get
            {
                return ViewModel.LabelVisibility;
            }
            set
            {
                ViewModel.LabelVisibility = value;
            }
        }
    }
}
