using iNKORE.UI.WPF.Modern.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net.Http;
using System.Text.Json;
using Path = System.IO.Path;
using System.Reflection;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using IWshRuntimeLibrary;
using File = System.IO.File;

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
