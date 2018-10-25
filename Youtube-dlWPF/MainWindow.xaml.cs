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

namespace Youtube_dlWPF
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public string _URL = "";
        public bool IsVideo = true;

        private Thread downloadThread = null;
        private Thread progressThread = null;
        private double progressValue = 0;

        private string fileName;
        private string savePath;
        private string version;

        public MainWindow()
        {
            InitializeComponent();
            _URL = "";
            InitializeComponent();
            VideoRadio.IsChecked = IsVideo;
            PathTextBox.Text = Environment.CurrentDirectory;
            savePath = PathTextBox.Text;
            GetVersion();

        }

        private void GetVersion()
        {
            try
            {
                Process proc = new Process();
                proc.StartInfo.FileName = "youtube-dl.exe";
                proc.StartInfo.Arguments = "--version";
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;


                proc.Start();
                version = proc.StandardOutput.ReadToEnd();

                
            }
            catch
            {
                version = "unknown";
            }
            finally
            {
                VersionLabel.Content = "Version : "  + version;
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            string result;
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                Process proc = new Process();
                proc.StartInfo.FileName = "youtube-dl.exe";
                proc.StartInfo.Arguments = "-U";
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;

                proc.Start();

                result = proc.StandardOutput.ReadToEnd();
                if (result.Contains("up-to-date") == true)
                {
                    MessageBox.Show("프로그램이 최신버전입니다.");
                }
                else
                {
                    MessageBox.Show("버전 업데이트 완료\n프로그램을 종료합니다.");
                    Close();
                }
            }
            catch
            {
                version = "unknown";
            }
            finally
            {
                VersionLabel.Content = "Version : " + version;
                Mouse.OverrideCursor = null;
            }
        }



        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (URLTextBox.Text == "")
            {
                MessageBox.Show("유튜브 링크를 입력하세요.", "에러", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
            {
                Mouse.OverrideCursor = Cursors.Wait;
            }));

            _URL = URLTextBox.Text.Trim();

            if (progressThread != null &&
                     progressThread.ThreadState == System.Threading.ThreadState.Running)
            {
                progressThread.Abort();
            }

            progressThread = null;
            DownloadProgress.Value = progressValue = 0;

            progressThread = new Thread(() =>
            {
                do
                {
                    Thread.Sleep(100);

                    Dispatcher.Invoke((Action)delegate ()
                    {
                        DownloadProgress.Value = progressValue;
                    });
                }
                while (progressValue < 100);

            });


            if (downloadThread!= null &&
               downloadThread.ThreadState == System.Threading.ThreadState.Running)
            {
                downloadThread.Abort();
            }
            downloadThread = new Thread(() =>
            {
                // Check Download URL
                string result = fileName = "";
                try
                {
                    Process proc = new Process();
                    proc.StartInfo.FileName = "youtube-dl.exe";
                    proc.StartInfo.Arguments = "-e \"" + _URL + "\"";
                    proc.StartInfo.RedirectStandardInput = true;
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.RedirectStandardError = true;
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.CreateNoWindow = true;


                    proc.Start();
                    result = proc.StandardOutput.ReadToEnd();
                    if (result == "")
                        result = "ERROR";
                    string ext = IsVideo == true ? ".mp4" : ".mp3";
                    fileName = result.Trim() + ext;
                }
                catch
                {
                    MessageBox.Show("잘못된 URL입니다.", "에러", MessageBoxButton.OK, MessageBoxImage.Error);
                    _URL = "";
                    return;
                }

                if (result.Contains("ERROR") == true)
                {
                    MessageBox.Show("잘못된 URL입니다.", "에러", MessageBoxButton.OK, MessageBoxImage.Error);
                    _URL = "";
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
                    {
                        Mouse.OverrideCursor = null;
                    }));
                    return;
                }

                // 
                try
                {
                    Process proc = new Process();
                    proc.StartInfo.FileName = "youtube-dl.exe";
                    if (IsVideo)
                        proc.StartInfo.Arguments = "--recode-video mp4 -o \"" + savePath+ "\\\\" + fileName + "\" \"" + _URL + "\"";
                    else
                        proc.StartInfo.Arguments = "--extract-audio  --audio-format mp3 --audio-quality 0 -o \"" + savePath + "\\\\" + fileName + "\" "  
                         +"\"" + _URL + "\"";
                    proc.StartInfo.RedirectStandardInput = true;
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.RedirectStandardError = true;
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.CreateNoWindow = true;


                    proc.Start();

                    string percentString;

                    while (true)
                    {
                        result = percentString = "";
                        result = proc.StandardOutput.ReadLine();

                        if (result == null || result.Contains("merged") == true)
                        {
                            progressValue = 100;
                            break;
                        }

                        if (result.Contains("download") == true &&
                            result.Contains("%") == true)
                        {
                            try
                            {

                                percentString = result.Substring("[download]".Length, result.IndexOf("%") - "[download]".Length).Trim();
                                Debug.Print("percent : " + percentString);
                                progressValue = double.Parse(percentString);

                            }
                            catch
                            {

                            }

                        }

                        Thread.Sleep(500);

                        if (result.Contains("ERROR") == true || result.Contains("100%") == true)
                        {
                            break;
                        }
                    }


                }
                catch (Exception ex)
                {
                    MessageBox.Show("에러발생\n" + ex.Message, "에러", MessageBoxButton.OK, MessageBoxImage.Error);
                    _URL = "";
                    return;
                }
                finally
                {
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
                    {
                        Mouse.OverrideCursor = null;
                    }));

                }
            });

            progressThread.Start();
            downloadThread.Start();
        }


        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (VideoRadio.IsChecked == true) // Video
            {
                IsVideo = true;
                AudioRadio.IsChecked = false;

            }
            else // Audio
            {
                VideoRadio.IsChecked = false;
                IsVideo = false;
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
            dialog.SelectedPath = savePath;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                savePath = dialog.SelectedPath;
                PathTextBox.Text = savePath;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (downloadThread != null)
                downloadThread.Abort();
            if (progressThread != null)
                progressThread.Abort();
        }
    }
}
