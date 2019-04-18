using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Threading;
using J_YoutubeDownloader.Control;
using J_YoutubeDownloader.Database;

namespace J_YoutubeDownloader
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window 
    {
        private Thread downloadThread = null;
        private Thread progressThread = null;
        private Thread qualityListThread = null;
        
        public static double ProgressValue = 0;

        private static YoutubeInfo YoutubeInfo;
        private string Version;
        private string SelectedExtension;
        private int SelectedQualityNumber;
        private string SelectedQualityNote;

        public static bool IsLoadDone = false;
        public bool IsVideo = true;

        private RadioButton SelectedVideoExtensionRadioButton;
        private RadioButton SelectedAudioExtensionRadioButton;
        private SetupDB Setup;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Setup = new SetupDB();

            VideoRadio.IsChecked = IsVideo;

            PathTextBox.Text = Setup.SavePath;

            SetVersion();

            // mp4
            SelectedVideoExtensionRadioButton = ((RadioButton)VideoExtensionStackPanel.Children[0]);
            SelectedVideoExtensionRadioButton.IsChecked = true;

            // mp3
            SelectedAudioExtensionRadioButton = ((RadioButton)AudioExtensionStackPanel.Children[0]);
            SelectedAudioExtensionRadioButton.IsChecked = true;

            // Visibility
            VideoExtensionStackPanel.Visibility = AudioExtensionStackPanel.Visibility = Visibility.Collapsed;
        }

        private void SetVersion()
        {
            string version = "";
            try
            {
                Control.DLController.GetVersion(ref version);
            }
            catch
            {
                version = "unknown";
            }
            finally
            {
                VersionLabel.Content = "Core Version : "  + version;
                Version = version;
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            string version = "";
            try
            {
                if (Control.DLController.DoUpdate(ref version) == true)
                {
                    this.Close();
                }
            }
            catch
            {
            }
            finally
            {
                VersionLabel.Content = "Version : " + version;
                Version = version;
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (VideoRadio.IsChecked == true) // Video
            {
                IsVideo = true;
                AudioRadio.IsChecked = false;
                VideoExtensionStackPanel.Visibility = Visibility.Visible;
                AudioExtensionStackPanel.Visibility = Visibility.Collapsed;

                if (SelectedVideoExtensionRadioButton != null)
                    SelectedExtension = SelectedVideoExtensionRadioButton.Content.ToString();
            }
            else // Audio
            {
                VideoRadio.IsChecked = false;
                IsVideo = false;
                VideoExtensionStackPanel.Visibility = Visibility.Collapsed;
                AudioExtensionStackPanel.Visibility = Visibility.Visible;

                if (SelectedAudioExtensionRadioButton != null)
                    SelectedExtension = SelectedAudioExtensionRadioButton.Content.ToString();
            }
        }

        private static string RemoveIllegalPathCharacters(string path)
        {
            string regexSearch = new string(System.IO.Path.GetInvalidFileNameChars()) + new string(System.IO.Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(path, "");
        }

        private void ChangeButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.SelectedPath = Setup.SavePath;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Setup.SavePath = dialog.SelectedPath;
                PathTextBox.Text = Setup.SavePath;
                Setup.Save(Setup.SavePath);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (downloadThread != null)
                downloadThread.Abort();
            if (progressThread != null)
                progressThread.Abort();

            DLController.Close();
        }

        private void URLTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (URLTextBox.Text.Contains("https://") == false ||
                URLTextBox.Text.Contains("youtu") == false)
            {
                return;
            }

            int count = 0;
            do
            {
                if (Control.DLController.IsWorking == true)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                else
                {
                    break;
                }
            }
            while (++count < 10);

            Mouse.OverrideCursor = Cursors.Wait;
            DownloadProgress.Value = ProgressValue = 0;
            Control.YoutubeInfo youtubeInfo = new Control.YoutubeInfo()
            {
                Url = URLTextBox.Text
            };

            ThumbnailPage.ThumbnailPage.ViewModel.UpdateSnapthotByFilename("");

            ThumbnailPage.ThumbnailPage.ViewModel.LabelVisibility = Visibility;

            bool isVaildUrl = Control.DLController.GetTitleName(ref youtubeInfo);

            YoutubeInfo = youtubeInfo;

            if (!isVaildUrl)
            {
                ThumbnailPage.ThumbnailPage.ViewModel.LabelVisibility = Visibility.Hidden;
                return;
            }
            if (qualityListThread != null)
            {
                try
                {
                    qualityListThread.Abort();
                }
                catch { }
                Thread.Sleep(300);
            }

            qualityListThread = new Thread(() => {
                do
                {
                    IsLoadDone = false;
                    Control.DLController.GetThumbnail(youtubeInfo.Url);
                    Control.DLController.GetQualityList(ref youtubeInfo);
                    UpdateQualityComboBox(youtubeInfo);
                    YoutubeInfo = youtubeInfo;
                    IsLoadDone = true;
                } while (false);
            });
            qualityListThread.Start();

            if (VideoExtensionStackPanel.Visibility == AudioExtensionStackPanel.Visibility) // Collapsed
            {
                if (VideoRadio.IsChecked == true)
                {
                    VideoExtensionStackPanel.Visibility = Visibility.Visible;
                    AudioExtensionStackPanel.Visibility = Visibility.Collapsed;
                    SelectedExtension = SelectedVideoExtensionRadioButton.Content.ToString();
                }
                else
                {
                    VideoExtensionStackPanel.Visibility = Visibility.Collapsed;
                    AudioExtensionStackPanel.Visibility = Visibility.Visible;
                    SelectedExtension = SelectedAudioExtensionRadioButton.Content.ToString();
                }
            }

            ThumbnailPage.ThumbnailPage.ViewModel.LabelVisibility = Visibility.Collapsed;
        }

        private void UpdateQualityComboBox(Control.YoutubeInfo youtubeInfo)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
            {
                QualityComboBox.Items.Clear();
            }));

            if (youtubeInfo == null)
            {
                return;
            }

            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
            {

                foreach (string qualitystring in youtubeInfo.QualityDictionary.Values)
                {
                    QualityComboBox.Items.Add(new ComboBoxItem() { Content = qualitystring });
                }
                QualityComboBox.SelectedIndex = 0;
            }));
        }

        private void QualityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (QualityComboBox.SelectedItem == null ||
                    YoutubeInfo == null)
                    return;
                SelectedQualityNote = ((ComboBoxItem)QualityComboBox.SelectedItem).Content.ToString();
                SelectedQualityNumber = YoutubeInfo.GetQualityNumberByNote(SelectedQualityNote);
            }
            catch
            {
            }
        }

        private void VideoExtensionRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                SelectedVideoExtensionRadioButton = (RadioButton)sender;
                SelectedExtension = SelectedVideoExtensionRadioButton.Content.ToString();
            }
        }

        private void AudioExtensionRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                SelectedAudioExtensionRadioButton = (RadioButton)sender;
                SelectedExtension = SelectedAudioExtensionRadioButton.Content.ToString();
            }
        }

        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            if (URLTextBox.Text == "")
            {
                MessageBox.Show("유튜브 링크를 입력하세요.", "에러", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Mouse.OverrideCursor == Cursors.Wait)
            {
                MessageBox.Show("잠시 후 시작해주세요.", "경고", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
            {
                Mouse.OverrideCursor = Cursors.Wait;
            }));

            if (progressThread != null &&
                     progressThread.ThreadState == System.Threading.ThreadState.Running)
            {
                progressThread.Abort();
            }

            progressThread = null;
            DownloadProgress.Value = ProgressValue = 0;

            progressThread = new Thread(() =>
            {
                do
                {
                    Thread.Sleep(100);

                    Dispatcher.Invoke((Action)delegate ()
                    {
                        DownloadProgress.Value = ProgressValue;
                    });
                }
                while (ProgressValue < 100);

                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
                {
                    Mouse.OverrideCursor = null;
                }));
            });


            if (downloadThread != null &&
               downloadThread.ThreadState == System.Threading.ThreadState.Running)
            {
                downloadThread.Abort();
            }
            downloadThread = new Thread(() =>
            {
                DLController.Download(
                    YoutubeInfo,
                    SelectedQualityNumber,
                    IsVideo,
                    SelectedExtension,
                    Setup.SavePath);
            });

            progressThread.Start();
            downloadThread.Start();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.OverrideCursor == Cursors.Wait)
            {
                e.Handled = true;
            }
        }

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                URLTextBox.Text = Clipboard.GetText();
            }
            catch { }
        }
    }
}
