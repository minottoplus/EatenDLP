using IWshRuntimeLibrary;
using System.Diagnostics;
using System.IO;
using System.Windows;
using File = System.IO.File;
using Path = System.IO.Path;

namespace EatenDLP
{
    /// <summary>
    /// about.xaml の相互作用ロジック
    /// </summary>
    public partial class about : Window
    {
        public about()
        {
            InitializeComponent();
            Loaded += about_Loaded;
        }


        private void about_Loaded(object sender, RoutedEventArgs e)
        {
            versionText.Text = $"Version: {GlobalData.Version}";
        }




        private void DlpUpdate_Click(object sender, RoutedEventArgs e)
        {
            ytDlpUpdate DlpUpdate = new ytDlpUpdate();
            DlpUpdate.ShowDialog();
        }
        private void Download_Click(object sender, RoutedEventArgs e)
        {
            ytDlpDownload DlpDownload = new ytDlpDownload();
            DlpDownload.ShowDialog();
        }

        private void Github_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo pi = new ProcessStartInfo()
            {
                FileName = "https://github.com/minottoplus/EatenDLP",
                UseShellExecute = true,
            };

            Process.Start(pi);
        }






        private async void Update_Click(object sender, RoutedEventArgs e)
        {
            Updater UpdateWin = new Updater();
            UpdateWin.ShowDialog();

        }

        private void Shortcut_Click(object sender, RoutedEventArgs e)
        {

            string executionPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            string shortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs", "EatenDLP.lnk");

            CreateShortcut(executionPath, shortcutPath);
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
    }
}
