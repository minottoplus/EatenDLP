﻿using System.Configuration;
using System.Data;
using System.Windows;

namespace EatenDLP
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {


    }


    public static class GlobalData
    {
        public static string Version { get; set; }
        public static string latestVersion { get; set; }

    }

}
