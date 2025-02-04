using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using Path = System.IO.Path;

namespace EatenDLP
{
    /// <summary>
    /// Updater.xaml の相互作用ロジック
    /// </summary>
    public partial class Updater : Window
    {
        public Updater()
        {
            InitializeComponent();

            Loaded += Updater_Loaded; // Loadedイベントハンドラを登録
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }



        private async void Updater_Loaded(object sender, RoutedEventArgs e)
        {
            string repoOwner = "minottoplus";
            string repoName = "EatenDLP";
            string targetFileName = "EatenDLP.exe";

            var releaseInfo = await GetLatestReleaseAssetUrl(repoOwner, repoName, targetFileName);

            string releaseTag = releaseInfo[0]; // tag name
            string downloadUrl = releaseInfo[1]; // ダウンロード URL
            string executionPath = Environment.GetCommandLineArgs()[0];
            string directoryPath = System.IO.Path.GetDirectoryName(executionPath);
            string tmpPath = Path.Combine(directoryPath, "EatenDLP.tmp"); // 保存先のパス




            // ファイルダウンロード
            using (WebClient client = new WebClient())
            {
                await client.DownloadFileTaskAsync(new Uri(downloadUrl), tmpPath);
            }


            RenameAndExecuteFile(tmpPath, executionPath);

        }






        public void RenameAndExecuteFile(string sourceFilePath, string destinationFilePath)
        {
            try
            {
                // バッチファイルを作成
                string batchFilePath = Path.Combine(Path.GetTempPath(), "rename_and_execute.bat");
                using (StreamWriter writer = new StreamWriter(batchFilePath))
                {
                    writer.WriteLine($"taskkill /f /im EatenDLP.exe");

                    writer.WriteLine("powershell -Command \"sleep -m 100\"");
                    writer.WriteLine($"del \"{destinationFilePath}\"");
                    writer.WriteLine($"ren \"{sourceFilePath}\" \"{Path.GetFileName(destinationFilePath)}\"");
                    writer.WriteLine($"\"{destinationFilePath}\"");
                    writer.WriteLine($"exit");
                }

                // バッチファイルを実行
                var app = new ProcessStartInfo();
                app.FileName = batchFilePath;
                app.UseShellExecute = true;

                Process.Start(app);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK);
                Console.WriteLine($"エラー: {ex.Message}");
            }
        }










        static async Task<string[]> GetLatestReleaseAssetUrl(string owner, string repo, string targetFileName)
        {
            string[] returns = new string[2];
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
            }
            catch { }
            return returns;

        }


    }
}
