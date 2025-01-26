using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            ytDlpUpdate DlpUpdate = new ytDlpUpdate();
            DlpUpdate.ShowDialog();
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
    }
}
