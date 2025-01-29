using System;
using System.Diagnostics;
using System.IO;
using System.Net;
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
using iNKORE.UI.WPF.Modern;
using iNKORE.UI.WPF.Modern.Controls;
using System.Configuration;

using Microsoft.WindowsAPICodePack.Dialogs;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using Path = System.IO.Path;
using System.Security.Claims;

namespace EatenDLP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {




        public MainWindow()
        {
            InitializeComponent();
            
            Loaded += MainWindow_Loaded; // Loadedイベントハンドラを登録
                                         // ウィンドウが閉じられる際のイベントハンドラー
            this.Closing += MainWindow_Closing;

        }



        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
        //変数設定
            string appDataPath = Environment.GetEnvironmentVariable("APPDATA");
            string EatenDlpFolderPath = Path.Combine(appDataPath, "EatenDLP");
            string exePath = Path.Combine(EatenDlpFolderPath, "yt-dlp.exe");
            string ffmpegPath = Path.Combine(EatenDlpFolderPath, "ffmpeg.exe");
            string iniPath = Path.Combine(EatenDlpFolderPath, "settings.ini");


            LoadSettings(iniPath);


            //exePathが存在するか
            if (File.Exists(exePath) && File.Exists(ffmpegPath))
            {
                // exePath が存在する場合の処理
                Console.WriteLine($"yt-dlp.exe は存在します: {exePath}");
            }
            else
            {
                //exePathが存在しない場合の処理
                MessageBox.Show("必須ファイルが見つかりません。ダウンロードを行います。",
                    "エラー",
                    MessageBoxButton.OK);

                ytDlpDownload DlpDownload = new ytDlpDownload();
                DlpDownload.ShowDialog();


            }
        }









        private void LoadSettings(string settingsFilePath)
        {
            Location_TextBox.IsEnabled = false;
            Browse_Button.IsEnabled = false;
            Download_Button.IsEnabled = false;

            if (File.Exists(settingsFilePath))
            {
                Dictionary<string, string> settings = new Dictionary<string, string>();
                using (StreamReader reader = new StreamReader(settingsFilePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            settings[parts[0]] = parts[1];
                        }
                    }
                }

                // 設定をUIに反映
                if (settings.ContainsKey("Quality"))
                {
                    if (int.TryParse(settings["Quality"], out int quality))
                    {
                        Quality_ComboBox.SelectedIndex = quality;
                    }
                }
                if (settings.ContainsKey("IsDefault"))
                {
                    if (bool.TryParse(settings["IsDefault"], out bool isDefault))
                    {
                        Default_RadioButton.IsChecked = isDefault;
                    }
                }
                if (settings.ContainsKey("IsCustom"))
                {
                    if (bool.TryParse(settings["IsCustom"], out bool isCustom))
                    {
                        Custom_RadioButton.IsChecked = isCustom;
                    }
                }
                if (settings.ContainsKey("Location"))
                {
                    Location_TextBox.Text = settings["Location"];
                }
            }
        }











        private void SaveSettings(string URL, int Quality, bool IsDefault, bool IsCustom, string Location, string settingsFilePath)
        {
            // 設定データをDictionaryに格納
            Dictionary<string, string> settings = new Dictionary<string, string>();
            settings["Quality"] = Quality.ToString();
            settings["IsDefault"] = IsDefault.ToString();
            settings["IsCustom"] = IsCustom.ToString();
            settings["Location"] = Location;

            // INIファイルに書き込み
            using (StreamWriter writer = new StreamWriter(settingsFilePath))
            {
                foreach (var setting in settings)
                {
                    writer.WriteLine($"{setting.Key}={setting.Value}");
                }
            }
        }







        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string appDataPath = Environment.GetEnvironmentVariable("APPDATA");
            string EatenDlpFolderPath = Path.Combine(appDataPath, "EatenDLP");
            string iniPath = Path.Combine(EatenDlpFolderPath, "settings.ini");

            bool IsDefault = Default_RadioButton.IsChecked ?? true; // 'bool?' から 'bool' への変換
            bool IsCustom = Custom_RadioButton.IsChecked ?? true; // 'bool?' から 'bool' への変換


            SaveSettings(URL_textBox.Text, Quality_ComboBox.SelectedIndex, IsDefault, IsCustom, Location_TextBox.Text, iniPath);
        }






        private void Info_Click(object sender, RoutedEventArgs e)
        {
            about AboutWindow = new about();
            AboutWindow.ShowDialog();
        }


        private void URL_Changed(object sender, RoutedEventArgs e)
        {
            Download_Button_Enable();
        }


        private void Default_Unchecked(object sender, RoutedEventArgs e)
        {
            Download_Button_Enable();
            Location_TextBox.IsEnabled = true;
            Browse_Button.IsEnabled = true;

        }



        private void Custom_Unchecked(object sender, RoutedEventArgs e)
        {
            Download_Button_Enable();
            Location_TextBox.IsEnabled = false;
            Browse_Button.IsEnabled = false;
        }


        private void Location_Changed(object sender, RoutedEventArgs e)
        {
            Download_Button_Enable();
        }



        private void Quality_Changed(object sender, RoutedEventArgs e)
        {
            
        }





        private void Download_Button_Enable()
        {
            if (URL_textBox.Text != "" && (Default_RadioButton.IsChecked == true || (Default_RadioButton.IsChecked == false && Location_TextBox.Text != "")))
            {
                Download_Button.IsEnabled = true;
            }
            else
            {
                Download_Button.IsEnabled = false;
            }
        }




        public static string GenerateCommand(string url, int quality, string outputPath)
        {
            // 基本コマンド
            string command = "";

            // 品質オプション
            string formatCode = "";
            switch (quality)
            {
                case 0: // Best Quality (MP4)
                    formatCode = "-f bv*[ext=mp4]+ba[ext=m4a]/b[ext=mp4]";
                    break;
                case 1: // Best Quality (Only MP3)
                    formatCode = "-x --audio-format mp3";
                    break;
                case 2: // Best Quality (Only WAV)
                    formatCode = "-x --audio-format wav";
                    break;
                case 3: // 1080p (MP4)
                    formatCode = "-f bestvideo[height<=1080][ext=mp4]+bestaudio[ext=m4a]/best[height<=1080][ext=mp4]";
                    break;
                case 4: // 720p (MP4)
                    formatCode = "-f bestvideo[height<=720][ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/best";
                    break;
                case 5: // 480p (MP4)
                    formatCode = "-f bestvideo[height<=480][ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/best";
                    break;
                case 6: // 360p (MP4)
                    formatCode = "-f bestvideo[height<=360][ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/best";
                    break;
                case 7: // 240p (MP4)
                    formatCode = "-f bestvideo[height<=240][ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/best";
                    break;
                case 8: // 144p (MP4)
                    formatCode = "-f bestvideo[height<=144][ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/best";
                    break;


            }
            command += " " + formatCode;

            // 出力オプション
            if (!string.IsNullOrEmpty(outputPath))
            {
                command += $" -o \"{Path.Combine(outputPath, "%(title)s.%(ext)s")}\""; // 出力パスとファイル名テンプレート
            }

            // URL
            command += $" \"{url}\"";

            return command;
        }



        public async void ExecuteCommand(string command)
        {
            try
            {
                string appDataPath = System.Environment.GetEnvironmentVariable("APPDATA");
                string EatenDlpFolderPath = System.IO.Path.Combine(appDataPath, "EatenDLP");
                string exePath = System.IO.Path.Combine(EatenDlpFolderPath, "yt-dlp.exe");




                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = command,
                    UseShellExecute = false,
                    CreateNoWindow = false
                };

                using (Process process = new Process { StartInfo = psi })
                {
                    process.Start();
                    await Task.Run(() => process.WaitForExit());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                // 例外処理
            }
        }


        public static string GetEatenDlpFolderPath()
        {
            // Documentsフォルダのパスを取得
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // EatenDLPフォルダのパスを結合
            string eatenDlpFolderPath = Path.Combine(documentsPath, "EatenDLP");

            // EatenDLPフォルダが存在するか確認
            if (!Directory.Exists(eatenDlpFolderPath))
            {
                Directory.CreateDirectory(eatenDlpFolderPath);
                Console.WriteLine("EatenDLPフォルダを作成しました: " + eatenDlpFolderPath); //デバッグ用
            }
            else
            {
                Console.WriteLine("EatenDLPフォルダは既に存在します: " + eatenDlpFolderPath); //デバッグ用
            }

            return eatenDlpFolderPath;
        }








        private void Download_Button_Click(object sender, RoutedEventArgs e)
        {
            string url = URL_textBox.Text; // 例
            int quality = Quality_ComboBox.SelectedIndex;

            string outputPath = ""; // 例


            if (Default_RadioButton.IsChecked == true)
            {
                outputPath = GetEatenDlpFolderPath(); // 例

            }
            else
            {

                outputPath = Location_TextBox.Text; // 例
            }

            string ytDlpCommand = GenerateCommand(url, quality, outputPath);
            Console.WriteLine(ytDlpCommand);

            ExecuteCommand(ytDlpCommand);
        }

        private void Browse_Button_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true; // フォルダ選択モードにする
            dialog.Title = "フォルダを選択してください";

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string selectedFolderPath = dialog.FileName;
                Location_TextBox.Text = selectedFolderPath;
            }
        }
    }
}