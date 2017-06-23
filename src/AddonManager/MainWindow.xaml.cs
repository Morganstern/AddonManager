using IniParser.Model;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Windows;
using System.IO.Compression;
using System.IO;

namespace AddonManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> addons = new List<string>(ConfigHandler.GetAddons());

        public MainWindow()
        {
            InitializeComponent();
            Startup();
        }

        private void Startup()
        {
            lstBox.ItemsSource = addons;
        }

        private void DownloadAddons()
        {
            
            string download = "data-href";
            string content;
            char c = 'a';

            foreach (var addon in addons)
            {
                string url = $@"https://mods.curse.com/addons/wow/{addon}/download";
                using (WebClient client = new WebClient())
                {
                    content = client.DownloadString(url);
                }
                int i = content.IndexOf(download) + 11;

                StringBuilder sb = new StringBuilder();
                while (true)
                {
                    c = content[i];
                    if (c == '"')
                        break;
                    sb.Append(c);
                    i++;
                }
                url = sb.ToString();

                GetAddonZip(url);
            }
        }

        private void GetAddonZip(string url)
        {
            IniData config = ConfigHandler.GetConfig();
            string wowpath = config["DIRECTORY"]["WowInstall"].Replace("\"", "");

            using (var client = new WebClient())
            {
                client.DownloadFile(url, "addon.zip");
            }

            ZipArchive zip = ZipFile.Open("addon.zip", ZipArchiveMode.Read);
            foreach (ZipArchiveEntry entry in zip.Entries)
            {
                DirectoryInfo dir = new DirectoryInfo(entry.ToString());
                string testPath = Path.GetDirectoryName(Path.Combine(wowpath, dir.ToString()));
                if (!Directory.Exists(testPath))
                {
                    Directory.CreateDirectory(testPath);
                }

                entry.ExtractToFile(Path.Combine(wowpath, entry.FullName), true);
            }
            zip.Dispose();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            DownloadAddons();           
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
