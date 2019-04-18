using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace J_YoutubeDownloader.Control
{
    public static class DLController
    {
        public static bool IsWorking = false;
        private static Process youtubeDlProcess = new Process();

        public static void GetVersion(ref string version)
        {
            try
            {
                youtubeDlProcess = new Process();
                youtubeDlProcess.StartInfo.FileName = "youtube-dl.exe";
                youtubeDlProcess.StartInfo.Arguments = "--version";
                youtubeDlProcess.StartInfo.RedirectStandardInput = true;
                youtubeDlProcess.StartInfo.RedirectStandardOutput = true;
                youtubeDlProcess.StartInfo.RedirectStandardError = true;
                youtubeDlProcess.StartInfo.UseShellExecute = false;
                youtubeDlProcess.StartInfo.CreateNoWindow = true;
                youtubeDlProcess.Start();
                version = youtubeDlProcess.StandardOutput.ReadToEnd();
            }
            catch
            {
                version = "unknown";
            }
        }

        public static void GetQualityList(ref YoutubeInfo youtubeInfo)
        {
            try
            {
                IsWorking = true;
                Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                }));

                if (youtubeInfo == null)
                {
                    return;
                }

                Process youtubeDlProcess = new Process();
                youtubeDlProcess.StartInfo.FileName = "youtube-dl.exe";
                youtubeDlProcess.StartInfo.Arguments = "--list-format " + youtubeInfo.Url;
                youtubeDlProcess.StartInfo.RedirectStandardInput = true;
                youtubeDlProcess.StartInfo.RedirectStandardOutput = true;
                youtubeDlProcess.StartInfo.RedirectStandardError = true;
                youtubeDlProcess.StartInfo.UseShellExecute = false;
                youtubeDlProcess.StartInfo.CreateNoWindow = true;
                youtubeDlProcess.Start();

                int failCount = 0;
                string stdout;
                while (failCount < 10)
                {
                    try
                    {
                        stdout = youtubeDlProcess.StandardOutput.ReadLine();
                        if (stdout == null || stdout == "")
                        {
                            failCount++;
                            Thread.Sleep(500);
                            continue;
                        }
                        else if (stdout.Contains("mp4") == true ||
                            stdout.Contains("webm") == true)
                        {
                            Debug.Print(stdout);
                            string[] arr = { "mp4", "webm"};
                            List<string> split = stdout.Split(arr, StringSplitOptions.RemoveEmptyEntries).ToList();
                            if (split[0].Trim().Length > 3)
                            {
                                continue;
                            }
                            int qualityCode = int.Parse(split[0].Trim());
                            string note = split[1].Trim();
                            // qualityCode/extension/resolution/note
                            youtubeInfo.QualityDictionary.Add(qualityCode, note);
                        }
                    }
                    catch
                    {
                        failCount++;
                    }
                }
            }
            catch{}
            finally
            {
                IsWorking = false;
                Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
                {
                    Mouse.OverrideCursor = null;
                }));

            }
        }

        public static bool GetTitleName(ref YoutubeInfo youtubeInfo)
        {
            try
            {
                Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                }));

                IsWorking = true;

                if (youtubeInfo == null)
                {
                    youtubeInfo = new YoutubeInfo()
                    {
                        TitleName = "",
                        Url = "",
                        QualityDictionary = new SortedDictionary<int, string>()
                    };
                }

                youtubeDlProcess = new Process();
                youtubeDlProcess.StartInfo.FileName = "youtube-dl.exe";
                youtubeDlProcess.StartInfo.Arguments = "-e " + youtubeInfo.Url;
                youtubeDlProcess.StartInfo.RedirectStandardInput = true;
                youtubeDlProcess.StartInfo.RedirectStandardOutput = true;
                youtubeDlProcess.StartInfo.RedirectStandardError = true;
                youtubeDlProcess.StartInfo.UseShellExecute = false;
                youtubeDlProcess.StartInfo.CreateNoWindow = true;
                youtubeDlProcess.Start();

                int failCount = 0;
                bool IsSuccess = true;
                string stdout;
                while (failCount < 10)
                {
                    try
                    {
                        stdout = youtubeDlProcess.StandardOutput.ReadToEnd();
                        if (stdout.Contains("ERROR")== true)
                        {
                            IsSuccess = false;
                            break;
                        }
                        youtubeInfo.TitleName = stdout.Trim();
                        break;
                    }
                    catch
                    {
                        failCount++;
                    }
                }
                return IsSuccess;
            }
            catch
            {
                return false;
            }
            finally
            {
                IsWorking = false;
                Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
                {
                    Mouse.OverrideCursor = null;
                }));
            }
        }

        public static bool DoUpdate(ref string version)
        {
            try
            {
                if (Mouse.OverrideCursor == Cursors.Wait)
                {
                    Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
                    {
                        MessageBox.Show("지금은 업데이트를 진행할 수 없습니다.");
                    }));
                    return false;
                }

                Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                }));

                Mouse.OverrideCursor = Cursors.Wait;

                youtubeDlProcess = new Process();
                youtubeDlProcess.StartInfo.FileName = "youtube-dl.exe";
                youtubeDlProcess.StartInfo.Arguments = "-U";
                youtubeDlProcess.StartInfo.RedirectStandardInput = true;
                youtubeDlProcess.StartInfo.RedirectStandardOutput = true;
                youtubeDlProcess.StartInfo.RedirectStandardError = true;
                youtubeDlProcess.StartInfo.UseShellExecute = false;
                youtubeDlProcess.StartInfo.CreateNoWindow = true;
                youtubeDlProcess.Start();

                string result = youtubeDlProcess.StandardOutput.ReadToEnd();
                if (result.Contains("up-to-date") == true)
                {
                    MessageBox.Show("프로그램이 최신버전입니다.");
                    return false;
                }
                else
                {
                    MessageBox.Show("버전 업데이트 완료\n프로그램을 종료합니다.");
                    return true;
                }
            }
            catch
            {
                version = "unknown";
                MessageBox.Show("알 수 없는 이유로 업데이트에 실패했습니다.");
                return false;
            }
            finally
            {
                Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
                {
                    Mouse.OverrideCursor = null;
                }));
            }
        }

        public static void GetThumbnail(string url)
        {
            try
            {
                IsWorking = true;
                Thread thread = new Thread((o) => 
                {
                    string _url = (string)o;

                    Process youtubeDlProcess = new Process();
                    youtubeDlProcess.StartInfo.FileName = "youtube-dl.exe";
                    youtubeDlProcess.StartInfo.Arguments = "--skip-download --write-thumbnail " + _url;
                    youtubeDlProcess.StartInfo.RedirectStandardInput = true;
                    youtubeDlProcess.StartInfo.RedirectStandardOutput = true;
                    youtubeDlProcess.StartInfo.RedirectStandardError = true;
                    youtubeDlProcess.StartInfo.UseShellExecute = false;
                    youtubeDlProcess.StartInfo.CreateNoWindow = true;
                    youtubeDlProcess.Start();

                    try
                    {
                        int failCount = 0;
                        string stdout, thumbnail = "";
                        while (failCount < 3)
                        {
                            try
                            {
                                stdout = youtubeDlProcess.StandardOutput.ReadLine();
                                if (stdout == null || stdout == "")
                                {
                                    failCount++;
                                    Thread.Sleep(500);
                                    continue;
                                }
                                else if (stdout.Contains("Writing thumbnail to:") == true )
                                {
                                    Debug.Print(stdout);
                                    thumbnail = stdout.Split(':').Last().Trim();
                                    break;
                                }
                            }
                            catch
                            {
                                failCount++;
                            }
                        }

                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            ThumbnailPage.ThumbnailPage.ViewModel.UpdateSnapthotByFilename(thumbnail);
                        }));
                    }
                    catch{}
                });
                thread.Start(url);
            }
            catch{}
            finally
            {
                IsWorking = false;
            }
        }

        public static bool Download(YoutubeInfo youtubeInfo, int quality, bool isVideo,  string extension, string savePath)
        { 
            try
            {
                IsWorking = true;

                bool IsDefalutMode = false;
                if (MainWindow.IsLoadDone == false)
                {
                    IsDefalutMode = true;
                }

                Process youtubeDlProcess = new Process();
                youtubeDlProcess.StartInfo.FileName = "youtube-dl.exe";
                if (isVideo)
                {
                    youtubeDlProcess.StartInfo.Arguments = "--recode-video " + extension +
                        " -o \"" + savePath + "\\\\" + youtubeInfo.TitleName + "." + extension + "\" " +
                        (IsDefalutMode? "": " -f " + quality) +
                        " \"" + youtubeInfo.Url + "\"";
                }
                else
                {
                    youtubeDlProcess.StartInfo.Arguments = "--extract-audio " +
                        (IsDefalutMode ? "" : " -f " + quality) +
                        " --audio-format " + extension +
                        " --audio-quality 0 " +
                        " -o \"" + savePath + "\\\\" + youtubeInfo.TitleName + "." + extension + "\" " +
                        " \"" + youtubeInfo.Url + "\"";
                }
                youtubeDlProcess.StartInfo.RedirectStandardInput = true;
                youtubeDlProcess.StartInfo.RedirectStandardOutput = true;
                youtubeDlProcess.StartInfo.RedirectStandardError = true;
                youtubeDlProcess.StartInfo.UseShellExecute = false;
                youtubeDlProcess.StartInfo.CreateNoWindow = true;
                youtubeDlProcess.Start();

                string result, percentString;
                while (true)
                {
                    result = percentString = "";
                    result = youtubeDlProcess.StandardOutput.ReadLine();

                    if (result == null || result.Contains("merged") == true)
                    {
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                        {
                             MainWindow.ProgressValue = 100;
                        }));
                        break;
                    }

                    if (result.Contains("download") == true &&
                        result.Contains("%") == true)
                    {
                        try
                        {
                            percentString = result.Substring("[download]".Length, result.IndexOf("%") - "[download]".Length).Trim();
                            Debug.Print("percent : " + percentString);
                            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                            {
                                MainWindow.ProgressValue = double.Parse(percentString);
                            }));
                        }
                        catch{}
                    }

                    Thread.Sleep(500);
                    if (result.Contains("ERROR") == true || result.Contains("100%") == true)
                    {
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            MainWindow.ProgressValue = 100;
                        }));
                        break;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("알수없는 이유로 다운로드에 실패했습니다.\n" +
                    "프로그램을 업데이트하시거나 관리자에게 문의하세요.\n\n" +
                    "Error:\n" +
                    ex.Message);
                return false;
            }
            finally
            {
                IsWorking = false;
            }
        }

        public static void Close()
        {
            try
            {
                youtubeDlProcess.Dispose();
            }
            catch { }

            KillAll();
        }

        public static void KillAll()
        {
            try
            {
                Process.Start
            (
                new ProcessStartInfo
                {
                    FileName = "taskkill.exe",
                    Arguments = "/im youtube-dl.exe /t /f",
                    RedirectStandardInput = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            );

                Process.Start
                (
                    new ProcessStartInfo
                    {
                        FileName = "taskkill.exe",
                        Arguments = "/im ffmpeg.exe /t /f",
                        RedirectStandardInput = false,
                        RedirectStandardOutput = false,
                        RedirectStandardError = false,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                );

                Process.Start
                (
                    new ProcessStartInfo
                    {
                        FileName = "taskkill.exe",
                        Arguments = "/im ffprobe.exe /t /f",
                        RedirectStandardInput = false,
                        RedirectStandardOutput = false,
                        RedirectStandardError = false,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                );
            }
            catch { }
        }

    }
}
