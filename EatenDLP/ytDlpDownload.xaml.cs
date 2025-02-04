using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using IWshRuntimeLibrary;
using File = System.IO.File;

namespace EatenDLP
{
    /// <summary>
    /// ytDlpDownload.xaml の相互作用ロジック
    /// </summary>
    public partial class ytDlpDownload : Window
    {
        public ytDlpDownload()
        {
            InitializeComponent();
            Loaded += ytDlpDownloadWindow_Loaded; // Loadedイベントハンドラを登録

        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }


        private async void ytDlpDownloadWindow_Loaded(object sender, RoutedEventArgs e)
        {

            string appDataPath = Environment.GetEnvironmentVariable("APPDATA");
            string EatenDlpFolderPath = Path.Combine(appDataPath, "EatenDLP");
            string exePath = Path.Combine(EatenDlpFolderPath, "yt-dlp.exe");
            string ffmpegPath = Path.Combine(EatenDlpFolderPath, "ffmpeg.exe");
            string downloadUrl = "https://github.com/yt-dlp/yt-dlp/releases/download/2024.10.22/yt-dlp.exe";
            string ffmpegUrl = "https://media.githubusercontent.com/media/minottoplus/EatenDLP/refs/heads/master/EatenDLP/assets/ffmpeg.exe";

            try
            {
                if (!Directory.Exists(EatenDlpFolderPath))
                {
                    Directory.CreateDirectory(EatenDlpFolderPath);
                }



                // ダウンロードを実行 (非同期)
                await DownloadFileAsync(downloadUrl, exePath);
                await DownloadFileAsync(ffmpegUrl, ffmpegPath);


                // ダウンロード完了後にアップデートを実行 (非同期)
                await UpdateYtDlpAsync(exePath);




                MessageBox.Show("Download complete successfully.", "Complete", MessageBoxButton.OK);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error has occurred: {ex.Message}", "Error", MessageBoxButton.OK);
            }



        }








        private async Task DownloadFileAsync(string url, string path)
        {
            using (WebClient client = new WebClient())
            {
                await client.DownloadFileTaskAsync(new Uri(url), path);
            }
        }

        private async Task UpdateYtDlpAsync(string exePath)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = "--update",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process { StartInfo = psi })
            {
                process.Start();
                await Task.Run(() => process.WaitForExit());
            }
        }
    }
}