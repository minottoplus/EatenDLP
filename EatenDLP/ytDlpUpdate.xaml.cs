using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using Path = System.IO.Path;

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


                MessageBox.Show("Update complete successfully.", "Complete", MessageBoxButton.OK);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error has occurred: {ex.Message}", "Error", MessageBoxButton.OK);
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