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
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

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


        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //変数設定
            string appDataPath = System.Environment.GetEnvironmentVariable("APPDATA");
            string EatenDlpFolderPath = System.IO.Path.Combine(appDataPath, "EatenDLP");
            string exePath = System.IO.Path.Combine(EatenDlpFolderPath, "yt-dlp.exe");
            string downloadUrl = "https://github.com/yt-dlp/yt-dlp/releases/download/2024.10.22/yt-dlp.exe";

            //exePathが存在するか
            if (File.Exists(exePath))
            {
                // exePath が存在する場合の処理
                Console.WriteLine($"yt-dlp.exe は存在します: {exePath}");
            }
            else
            {
                //exePathが存在しない場合の処理
                MessageBox.Show("yt-dlpが見つかりません。yt-dlpのダウンロードを行います。",
                    "エラー",
                    MessageBoxButton.OK);

                ytDlpDownload DlpDownload = new ytDlpDownload();
                DlpDownload.ShowDialog();


            }


            Quality_ComboBox.SelectedIndex = 0;
            Download_Button.IsEnabled = false;
            Location_TextBox.IsEnabled = false;
            Browse_Button.IsEnabled = false;
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







        private void Download_Button_Click(object sender, RoutedEventArgs e)
        {
            Download_Button_Enable();
        }




    }
}