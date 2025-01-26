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

namespace EatenDLP
{
    /// <summary>
    /// ytDlpUpdate.xaml の相互作用ロジック
    /// </summary>
    public partial class ytDlpUpdate : Window
    {
        public ytDlpUpdate()
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

            try
            {


                // ダウンロード完了後にアップデートを実行 (非同期)
                await UpdateYtDlpAsync(exePath);


                MessageBox.Show("正常にアップデートが完了しました。", "完了", MessageBoxButton.OK);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"エラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK);
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