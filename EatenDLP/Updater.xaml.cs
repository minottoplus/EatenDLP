using System.Diagnostics;
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
            string executionPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            string directoryPath = System.IO.Path.GetDirectoryName(executionPath);
            string exePath = Path.Combine(directoryPath, "EatenDLP.tmp"); // 保存先のパス




            // ファイルダウンロード
            using (WebClient client = new WebClient())
            {
                await client.DownloadFileTaskAsync(new Uri(downloadUrl), exePath);
            }


            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = exePath,
                UseShellExecute = false,
                CreateNoWindow = false
            };

            Process.Start(psi);

            
            this.Close();
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
