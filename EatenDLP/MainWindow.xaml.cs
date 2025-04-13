using iNKORE.UI.WPF.Modern.Controls;
using IWshRuntimeLibrary;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using File = System.IO.File;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using Path = System.IO.Path;

namespace EatenDLP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private double _lastProgress = -1; // 前回の進捗率を保存する変数


        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded; // Loadedイベントハンドラを登録
                                         // ウィンドウが閉じられる際のイベントハンドラー
            this.Closing += MainWindow_Closing;


        }


        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {




            GlobalData.Version = "1.0.8";
            GlobalData.latestVersion = await GetLatestReleaseTagName("minottoplus", "EatenDLP");


           
            //未来人表記
            if (long.Parse(GlobalData.Version.Replace(".", "")) > long.Parse(GlobalData.latestVersion.Replace(".", "")))
            {
                InfoBar.Title = "未来人？";
                InfoBar.Message = "あなたは現行バージョンより新しいEatenDLPを実行しています！";
                InfoBar.Severity = InfoBarSeverity.Informational;
                InfoBar.IsOpen = true;
            }



            // 変数設定
            string executionPath = Environment.GetCommandLineArgs()[0];
            string directoryPath = Path.GetDirectoryName(executionPath);
            string oldPath = Path.Combine(directoryPath, "EatenDLP.exe");
            string shortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs", "EatenDLP.lnk");

            string appDataPath = Environment.GetEnvironmentVariable("APPDATA");
            string EatenDlpFolderPath = Path.Combine(appDataPath, "EatenDLP");
            string exePath = Path.Combine(EatenDlpFolderPath, "yt-dlp.exe");
            string ffmpegPath = Path.Combine(EatenDlpFolderPath, "ffmpeg.exe");
            string iniPath = Path.Combine(EatenDlpFolderPath, "settings.ini");


            //設定のロード
            LoadSettings(iniPath);
            if (long.Parse(GlobalData.Version.Replace(".", "")) < long.Parse(GlobalData.latestVersion.Replace(".", "")))
            {
                InfoBar.Title = "Update Available";
                InfoBar.Message = $"Please update in about window";
                InfoBar.Severity = InfoBarSeverity.Informational;
                InfoBar.IsOpen = true;
            }
            //GetEatenDlpFolderPath();



            //ショートカット作成
            // exePathが存在するか
            if (File.Exists(exePath) && File.Exists(ffmpegPath))
            {
                // exePath が存在する場合の処理
            }
            else
            {
                // exePathが存在しない場合の処理
                MessageBox.Show("Required file is missing.",
                    "Error",
                    MessageBoxButton.OK);

                ytDlpDownload DlpDownload = new ytDlpDownload();
                DlpDownload.ShowDialog();
            }

            if (File.Exists(shortcutPath))
            {
                // exePath が存在する場合の処理
            }
            else
            {
                CreateShortcut(executionPath, shortcutPath);
            }


            //レジストリ登録
            //RegisterUninstallInfo("EatenDLP", directoryPath, executionPath, GlobalData.Version);





            //アップデート後の処理
            bool isRestartedForUpdate = Environment.GetCommandLineArgs().Contains("--restarted-for-update");
            if (isRestartedForUpdate)
            {
                Updater UpdateWin = new Updater();
                UpdateWin.ShowDialog();
            }


            //アップデート後の処理
            bool isStartedForUninstall = Environment.GetCommandLineArgs().Contains("--uninstall");
            if (isStartedForUninstall)
            {
                MessageBox.Show("You CAN'T uninstall this program lol");
                ProcessStartInfo pi = new ProcessStartInfo()
                {
                    FileName = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                    UseShellExecute = true,
                };

                Process.Start(pi);
            }










        }

        static void RegisterUninstallInfo(string appName, string installDir, string exePath, string version)
        {
            string uninstallRegPath = $"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{appName}";

            using (var key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(uninstallRegPath))
            {
                if (key == null)
                {
                    throw new Exception("レジストリキーの作成に失敗しました。");
                }

                key.SetValue("DisplayName", appName);
                key.SetValue("Publisher", "minotto");
                key.SetValue("DisplayVersion", version);
                key.SetValue("InstallLocation", installDir);
                key.SetValue("UninstallString", $"{exePath} --uninstall");
                key.SetValue("DisplayIcon", exePath);
            }
        }

        private void CreateShortcut(string targetPath, string shortcutPath)
        {
            try
            {
                // ターゲットパスが存在するか確認
                if (!File.Exists(targetPath) && !Directory.Exists(targetPath))
                {
                    throw new FileNotFoundException($"Target path '{targetPath}' not found.");
                }

                // IWshRuntimeLibrary を使ってショートカットを作成
                IWshShell shell = new WshShell();
                IWshShortcut shortcut = shell.CreateShortcut(shortcutPath);

                shortcut.TargetPath = targetPath;
                shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
                shortcut.Save();
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Error creating shortcut: {ex.Message}");
            }
        }



        static async Task<string> GetLatestReleaseTagName(string owner, string repo)
        {
            string tagName = "";
            try
            {
                string apiUrl = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";
                using HttpClient client = new HttpClient();

                // GitHub APIのリクエストヘッダーを設定
                client.DefaultRequestHeaders.UserAgent.ParseAdd("CSharpApp");

                HttpResponseMessage response = await client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(json);

                JsonElement root = doc.RootElement;

                if (root.TryGetProperty("tag_name", out JsonElement tagNameProperty))
                {
                    tagName = tagNameProperty.GetString();
                }
            }
            catch (Exception ex)
            {
                // エラー処理を追加 (例: ロギング)
                Console.WriteLine($"Error getting tag name: {ex.Message}");
                // 必要に応じて、例外を再スローすることも検討
                // throw;
            }
            return tagName;
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
            if (URL_textBox.Text.Contains("soundcloud")){
                Quality_ComboBox.SelectedIndex = 2;
            }
            if (URL_textBox.Text.Contains("nico"))
            {
                string firefoxPath = null;
                bool isFirefoxAvailable = false;

                // 64-bit Windows
                firefoxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Mozilla Firefox", "firefox.exe");
                if (File.Exists(firefoxPath))
                {
                    isFirefoxAvailable = true;
                }

                // 32-bit Windows (or Firefox installed in the 32-bit Program Files on 64-bit Windows)
                firefoxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Mozilla Firefox", "firefox.exe");
                if (File.Exists(firefoxPath))
                {

                    isFirefoxAvailable = true;
                }

                // Check in the local application data folder (less common but possible)
                firefoxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mozilla Firefox", "firefox.exe");
                if (File.Exists(firefoxPath))
                {

                    isFirefoxAvailable = true;
                }


                if (!isFirefoxAvailable)
                {
                    InfoBar.Title = "Firefox not found";
                    InfoBar.Message = "Downloading from Niconico requires opening Niconico in Firefox once.";
                    InfoBar.Severity = InfoBarSeverity.Warning;
                    InfoBar.IsOpen = true;

                }
            }
            else
            {
                InfoBar.IsOpen = false;
            }
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





            if (Default_RadioButton.IsChecked == true || (Default_RadioButton.IsChecked == false && Location_TextBox.Text != ""))
            {
                Open_Button.IsEnabled = true;
            }
            else
            {
                Open_Button.IsEnabled = false;
            }


        }




        public static string GenerateCommand(string url, int quality, string outputPath, string nicoOption)
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
                default:
                    formatCode = nicoOption;
                    break;
                

            }

            formatCode += " --no-mtime -S vcodec:h264";


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



        public async Task<string> ExecuteCommand(string command)
        {
            string outputResult = "";
            try
            {
                InfoBar.IsOpen = false;
                string appDataPath = Environment.GetEnvironmentVariable("APPDATA");
                string EatenDlpFolderPath = Path.Combine(appDataPath, "EatenDLP");
                string exePath = Path.Combine(EatenDlpFolderPath, "yt-dlp.exe");

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = command + " --progress-template \"%(progress._percent_str)s\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = psi })
                {
                    bool first100 = false;
                    bool error = false;
                    string errorContent = "";
                    string outputContent = "";
                    process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
                    {
                        if (e.Data != null)
                        {
                            outputContent += e.Data + Environment.NewLine;
                            if (e.Data.Contains("100") && first100 == false)
                            {
                                first100 = true;
                            }
                            else
                            {
                                Debug.WriteLine(e.Data);
                                Dispatcher.Invoke(() =>
                                {
                                    ProgressText.Text = e.Data;
                                });
                            }
                        }
                    };
                    process.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e)
                    {
                        if (e.Data != null)
                        {
                            Debug.WriteLine(e.Data);
                            error = true;

                            char[] separators = { '.', ';' };

                            string[] parts = e.Data.Split(separators);

                            if (parts.Length > 0)
                            {
                                errorContent = parts[0];
                            }
                        }
                    };

                    ProgressBar.Visibility = Visibility.Visible;
                    ProgressText.Visibility = Visibility.Visible;

                    ProgressBar.IsIndeterminate = true;

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    await Task.Run(() => process.WaitForExit());

                    ProgressBar.Visibility = Visibility.Collapsed;
                    ProgressText.Visibility = Visibility.Collapsed;

                    if (error == true && !errorContent.Contains("WARNING"))
                    {
                        InfoBar.Title = "Failed";
                        InfoBar.Message = errorContent;
                        InfoBar.Severity = InfoBarSeverity.Error;
                        InfoBar.IsOpen = true;
                    }
                    else if ((process.ExitCode == 0 || errorContent.Contains("WARNING")) && !command.Contains("formats") && !outputContent.Contains("already"))
                    {
                        InfoBar.Title = "Success";
                        InfoBar.Message = "Download complete!";
                        InfoBar.Severity = InfoBarSeverity.Success;
                        InfoBar.IsOpen = true;
                    }
                    else if (outputContent.Contains("already"))
                    {
                        InfoBar.Title = "Info";
                        InfoBar.Message = "Video has already been downloaded.";
                        InfoBar.Severity = InfoBarSeverity.Informational;
                        InfoBar.IsOpen = true;
                    }
                    else
                    {
                        //InfoBar.Title = "Failed";
                        //InfoBar.Message = "Download failed.";
                        //InfoBar.Severity = InfoBarSeverity.Error;
                        //InfoBar.IsOpen = true;
                    }

                    _lastProgress = -1; // 進捗率をリセット

                    if (!command.Contains("formats") || (error == true && !errorContent.Contains("WARNING")))
                    {

                        Cancel_Button.Visibility = Visibility.Collapsed;
                        Download_Button.Visibility = Visibility.Visible;
                    }

                    outputResult = outputContent;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");

                InfoBar.Title = "Error";
                InfoBar.Message = $"Download failed! Exception: {ex.Message}";
                InfoBar.Severity = InfoBarSeverity.Error;
                InfoBar.IsOpen = true;
                outputResult = $"Error: {ex.Message}";
            }

            Download_Button.IsEnabled = true;

            KillProcess("yt-dlp");
            return outputResult;
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
            }
            else
            {
            }

            return eatenDlpFolderPath;
        }



        public static (string lastVideo, string lastAudio) ExtractLastVideoAudio(string input)
        {
            string lastVideo = null;
            string lastAudio = null;
            string[] lines = input.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            // 後ろから検索して最後の "video-" を含む行を見つける
            for (int i = lines.Length - 1; i >= 0; i--)
            {
                if (lines[i].Contains("video-"))
                {
                    lastVideo = lines[i].Substring(lines[i].IndexOf("video-")).Split(' ').First();
                    break;
                }
            }

            // 後ろから検索して最後の "audio-" を含む行を見つける
            for (int i = lines.Length - 1; i >= 0; i--)
            {
                if (lines[i].Contains("audio-"))
                {
                    lastAudio = lines[i].Substring(lines[i].IndexOf("audio-")).Split(' ').First();
                    break;
                }
            }

            return (lastVideo, lastAudio);
        }




        private async void Download_Button_Click(object sender, RoutedEventArgs e)
        {
            //Download_Button.IsEnabled = false;
            Download_Button.Visibility = Visibility.Collapsed;
            Cancel_Button.Visibility = Visibility.Visible;
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


            string ytDlpCommand = "";
            string formatCode = "";
            if (url.Contains("nico"))
            { //ニコニコ動画の処理
                formatCode += " --list-formats --cookies-from-browser firefox";
                ytDlpCommand += " " + formatCode;
                ytDlpCommand += $" \"{url}\"";

                Debug.WriteLine(ExecuteCommand(ytDlpCommand));
                string output = await ExecuteCommand(ytDlpCommand);
                (string highestVideo, string highestAudio) = ExtractLastVideoAudio(output);
                string nicoOption = "-f " + highestVideo + "+" + highestAudio;
                ytDlpCommand = GenerateCommand(url, 114514, outputPath, "");
                

            }
            else
            {

                ytDlpCommand = GenerateCommand(url, quality, outputPath, "");
            }


            
            Debug.WriteLine("[KORE]"+ytDlpCommand);

            ExecuteCommand(ytDlpCommand);

        }


        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            // yt-dlpプロセスを終了
            KillProcess("yt-dlp");
            KillProcess("ffmpeg");

            // プログレスバーとテキストを非表示にする
            ProgressBar.Visibility = Visibility.Collapsed;
            ProgressText.Visibility = Visibility.Collapsed;

            // キャンセルボタンを非表示にし、ダウンロードボタンを再表示する
            Cancel_Button.Visibility = Visibility.Collapsed;
            Download_Button.Visibility = Visibility.Visible;

            Task.Delay(100);

            // 一時ファイルを削除
            string outputPath = ""; // 例


            if (Default_RadioButton.IsChecked == true)
            {
                outputPath = GetEatenDlpFolderPath(); // 例

            }
            else
            {

                outputPath = Location_TextBox.Text; // 例
            }

            foreach (var file in Directory.GetFiles(outputPath))
            {
                if (Path.GetExtension(file).Contains("part"))
                {
                    try
                    {
                        File.Delete(file);
                        Debug.WriteLine($"tried to delete file: {file}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error deleting file: {ex.Message}");
                    }
                }
            }
            // 情報バーにキャンセルメッセージを表示
            InfoBar.Title = "Cancelled";
            InfoBar.Message = "Download has been cancelled.";
            InfoBar.Severity = InfoBarSeverity.Warning;
            InfoBar.IsOpen = true;
        }




        private void KillProcess(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);

            if (processes.Length == 0)
            {
                return;
            }

            foreach (Process process in processes)
            {
                try
                {
                    process.Kill();
                }
                catch (Exception ex)
                {
                }
            }
        }







        private void Browse_Button_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true; // フォルダ選択モードにする
            dialog.Title = "Pick a folder";

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string selectedFolderPath = dialog.FileName;
                Location_TextBox.Text = selectedFolderPath;
            }
        }


        private void Open_Button_Click(object sender, RoutedEventArgs e)
        {

            string EatenDLPPath = GetEatenDlpFolderPath(); // 例
            if (Default_RadioButton.IsChecked == true)
            {
                ShellExecute(IntPtr.Zero, "open", EatenDLPPath, null, null, 1);
            }
            else
            {
                ShellExecute(IntPtr.Zero, "open", Location_TextBox.Text, null, null, 1);
            }

        }






        [DllImport("shell32.dll")]
        private static extern int ShellExecute(IntPtr hWnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);


    }


}


