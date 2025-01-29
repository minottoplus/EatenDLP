using Microsoft.Win32;
using System.IO;
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
using Path = System.IO.Path;
using System.Diagnostics;
using System.Net;
using System.Security.Principal;
using System.Management.Automation;
using System.Net.Http;
using System.Text.Json;
using System.Management.Automation.Runspaces;
using static System.Net.Mime.MediaTypeNames;
using System;
using iNKORE.UI.WPF.Modern.Controls;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static string repoOwner = "minottoplus";
        public static string repoName = "EatenDLP";
        public static string targetFileName = "EatenDLP.exe";


        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }


        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            string[] releaseInfo = await GetLatestReleaseAssetUrl(repoOwner, repoName, targetFileName);

            string releaseTag = releaseInfo[0]; // tag name


            publisherText.Text = "Publisher: minottoplus";
            versionText.Text = $"Version: {releaseTag}";
            installButton.IsEnabled = true;
        }


        private async void installButton_Click(object sender, RoutedEventArgs e)
        {



            var releaseInfo = await GetLatestReleaseAssetUrl(repoOwner, repoName, targetFileName);

            string releaseTag = releaseInfo[0]; // tag name
            string downloadUrl = releaseInfo[1]; // ダウンロード URL
            string programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string targetPath = Path.Combine(programFilesPath, "EatenDLP");
            string exePath = Path.Combine(targetPath, "EatenDLP.exe"); // 保存先のパス


            if(Title.Text.Contains("Install"))
            {
                try
                {
                    progressPanel.Visibility = Visibility.Visible;

                    Title.Text = "Installing EatenDLP...";
                    statusText.Text = "Downloading...";
                    progressBar.IsIndeterminate = false;

                    // ディレクトリ作成
                    Directory.CreateDirectory(targetPath);

                    // ファイルダウンロード
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadProgressChanged += (s, args) =>
                        {
                            progressBar.Value = args.ProgressPercentage;
                        };

                        await client.DownloadFileTaskAsync(new Uri(downloadUrl), exePath);

                    }



                    statusText.Text = "Installing...";
                    progressBar.IsIndeterminate = true;

                    // ショートカット作成 (IWshRuntimeLibrary を使わない方法)
                    string shortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "programs", "EatenDLP.lnk");
                    CreateShortcut(exePath, shortcutPath);



                    RegisterUninstallInfo("EatenDLP", targetPath, exePath, releaseTag);


                    Title.Text = "EatenDLP is ready!";
                    installButton.Content = "Launch";



                    if (launchCheckBox.IsChecked == true)
                    {
                        runProgram(exePath);
                        this.Close();
                    }

                    progressPanel.Visibility = Visibility.Hidden;


                }
                catch (Exception ex)
                {
                }
            }
            else
            {
                runProgram(exePath);
                this.Close();


            }

        }




        private async void runProgram(string exePath)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = false,
                CreateNoWindow = false
            };

            using (Process process = new Process { StartInfo = psi })
            {
                process.Start();
                await Task.Run(() => process.WaitForExit());
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
                key.SetValue("UninstallString", exePath);
                key.SetValue("DisplayIcon", exePath);
            }
        }






        // ショートカット作成 (IWshRuntimeLibrary を使わない方法)
        public static void CreateShortcut(string targetPath, string shortcutPath)
        {
            // PowerShellスクリプト
            string script = $@"
                $WshShell = New-Object -ComObject WScript.Shell
                $Shortcut = $WshShell.CreateShortcut('{shortcutPath}')
                $Shortcut.TargetPath = '{targetPath}'
                $Shortcut.Save()
            ";

            try
            {
                // PowerShellを実行
                using (PowerShell ps = PowerShell.Create())
                {
                    ps.AddScript(script);
                    ps.Invoke();

                    if (ps.Streams.Error.Count > 0)
                    {
                        foreach (var error in ps.Streams.Error)
                        {
                            Debug.WriteLine("Error: " + error.ToString());
                        }
                    }
                    else
                    {
                        Debug.WriteLine("ショートカットが正常に作成されました。");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("エラーが発生しました: " + ex.Message);
            }
        }





        static async Task<string[]> GetLatestReleaseAssetUrl(string owner, string repo, string targetFileName)
        {
            string[] returns = new string[2];


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
                returns[0] = tagNameProperty.GetString();
            }



            if (root.TryGetProperty("assets", out JsonElement assets))
            {
                foreach (JsonElement asset in assets.EnumerateArray())
                {
                    if (asset.TryGetProperty("name", out JsonElement nameProperty) &&
                        nameProperty.GetString() == targetFileName)
                    {
                        if (asset.TryGetProperty("browser_download_url", out JsonElement downloadUrlProperty))
                        {
                            returns[1] = downloadUrlProperty.GetString();
                        }
                    }
                }
            }

            return returns;
        }

        private void aboutButton_Click(object sender, RoutedEventArgs e)
        {
            about aboutwin = new about();
            aboutwin.ShowDialog();
        }
    }
}