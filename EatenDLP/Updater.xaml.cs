using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
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


        public static bool IsAdminPrivilegesRequiredForPath(string filePath)
        {
            string directoryPath = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(directoryPath))
            {
                return false; // ディレクトリパスが取得できない場合は権限不要とみなす（念のため）
            }

            string testFilePath = Path.Combine(directoryPath, ".test_permission"); // テストファイル名
            try
            {
                // ファイル書き込みを試みる
                File.WriteAllText(testFilePath, "書き込みテスト");
                File.Delete(testFilePath); // テストファイル削除
                return false; // 書き込み成功 = 管理者権限不要と判断
            }
            catch (UnauthorizedAccessException)
            {
                return true; // 書き込み失敗 (UnauthorizedAccessException) = 管理者権限が必要と判断
            }
            catch (Exception)
            {
                return true; // その他のエラーの場合は、念のため管理者権限は必要とみなす (状況に応じて変更)
            }
        }



        private bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
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





            bool isAdminRequired = IsAdminPrivilegesRequiredForPath(executionPath);

            if (isAdminRequired)
            {
                if (!IsAdministrator())
                {



                    // 管理者権限で再起動
                    ProcessStartInfo app = new ProcessStartInfo();
                    app.FileName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

                    app.Arguments = "--restarted-for-update";
                    app.Verb = "runas"; // 管理者として実行
                    app.UseShellExecute = true;


                    try
                    {
                        Process.Start(app);
                        Application.Current.Shutdown(); // 現在のインスタンスを終了
                        return; // アップデート処理を中断
                    }
                    catch (System.ComponentModel.Win32Exception ex)
                    {
                        // ユーザーが管理者昇格をキャンセルした場合など
                        MessageBox.Show("Administrative privileges are required.", "Permission Error", MessageBoxButton.OK);
                        return; // アップデート処理を中断
                    }
                }
            }

















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
