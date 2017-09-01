using System.Collections.Generic;
using AddonManager.Models;
using System.Data.SQLite;
using System.IO;
using IniParser.Model;
using System.Net;
using System.IO.Compression;
using System.Text;
using System;
using System.Windows;
using System.Text.RegularExpressions;

namespace AddonManager
{
    public class AddonHandler : IDisposable
    {
        private SQLiteConnection dbConnection { get; set; }

        public AddonHandler()
        {
            if (!File.Exists("addons.sqlite"))
            {
                SQLiteConnection.CreateFile("addons.sqlite");
                dbConnection = new SQLiteConnection("Data Source=addons.sqlite;Version=3;");
                dbConnection.Open();
                string sql = "create table addons (name varchar(255), version varchar(255), url varchar(255));";
                SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
                command.ExecuteNonQuery();
            }
            else
            {
                dbConnection = new SQLiteConnection("Data Source=addons.sqlite;Version=3;");
                dbConnection.Open();
            }
        }

        public void CloseHandler()
        {
            dbConnection.Close();
        }

        public List<Addon> GetAddons()
        {
            List<Addon> addons = new List<Addon>();
            string query = "SELECT * FROM ADDONS;";
            SQLiteCommand command = new SQLiteCommand(query, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
                addons.Add(new Addon(reader["name"].ToString(), reader["version"].ToString(), reader["url"].ToString()));
            return addons;
        }

        public bool AddAddon(string name, string url)
        {
            List<Addon> currentAddons = new List<Addon>();
            currentAddons = GetAddons();

            foreach (var a in currentAddons)
                if (name == a.Name || url == a.URL)
                    return false;

            string version = "0.0.0"; //Placeholder until first update run
            string insert = $@"INSERT INTO ADDONS VALUES('{name}', '{version}', '{url}');";
            SQLiteCommand command = new SQLiteCommand(insert, dbConnection);
            command.ExecuteNonQuery();

            return true;
        }

        public void RemoveAddon(string selectedAddon)
        {
            string query = $"DELETE FROM addons WHERE name = \"{selectedAddon}\"";
            SQLiteCommand command = new SQLiteCommand(query, dbConnection);
            command.ExecuteNonQuery();
        }

        public void DownloadAddons(List<Addon> addons)
        {
            foreach (var addon in addons)
            {
                if (addon.Name == "ElvUI")
                    DownloadElvUI(addon);
                else
                    DownloadCurse(addon);
            }
            MessageBox.Show("Updates completed successfully!");
        }

        private void DownloadCurse(Addon addon)
        {
            // Number of characters to seek past when parsing the download location of the zip file
            const int hrefPadding = 11;

            string download = "data-href";
            string content;
            char c = 'a';

            string url = $@"https://mods.curse.com/addons/wow/{addon.URL}/download";
            try
            {
                using (WebClient client = new WebClient())
                {
                    content = client.DownloadString(url);
                }
            }
            catch
            {
                MessageBox.Show($"{addon.Name} failed to download, continuing", "Addon Manager");
                return;
            }

            int i = content.IndexOf(download) + hrefPadding;

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

            string pattern = @"(\d+\.\d+\.\d+)";
            Regex re = new Regex(pattern);
            MatchCollection matches = re.Matches(url);
            string version;
            if (matches.Count > 0)
                version = matches[0].ToString();
            else
                version = String.Empty;

            if (SkipUpdate(addon, version))
                return;

            GetAddonZip(url, addon.Name);
        }

        private void DownloadElvUI(Addon addon)
        {
            string ziptest = @"https://www.tukui.org/downloads/elvui-";
            string download = @"/downloads/elvui-";
            string content;
            char c = 'a';

            string url = $@"https://www.tukui.org/welcome.php";
            try
            {
                using (WebClient client = new WebClient())
                {
                    content = client.DownloadString(url);
                }
            }
            catch
            {
                MessageBox.Show($"{addon.Name} failed to download, continuing", "Addon Manager");
                return;
            }

            int i = content.IndexOf(download) + download.Length;

            StringBuilder sb = new StringBuilder();
            while (true)
            {
                c = content[i];
                if (c == '"')
                    break;
                sb.Append(c);
                i++;
            }
            url = ziptest + sb.ToString();

            string version = url.Replace(download, String.Empty).Replace(".zip", String.Empty);
            if (SkipUpdate(addon, version))
                return;

            GetAddonZip(url, addon.Name);
        }

        public void GetAddonZip(string url, string name)
        {
            IniData config = ConfigHandler.GetConfig();
            string wowpath = config["DIRECTORY"]["WowInstall"].Replace("\"", "");

            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile(url, "addon.zip");
                }
            }
            catch
            {
                MessageBox.Show($"The zip file for {name} failed to download, continuing", "Addon Manager");
                return;
            }

            ZipArchive zip = ZipFile.Open("addon.zip", ZipArchiveMode.Read);
            foreach (ZipArchiveEntry entry in zip.Entries)
            {
                if (entry.FullName.Contains(".git") == false && entry.Name != "")
                {
                    DirectoryInfo dir = new DirectoryInfo(entry.ToString());
                    string testPath = Path.GetDirectoryName(Path.Combine(wowpath, dir.ToString()));
                    if (!Directory.Exists(testPath))
                    {
                        Directory.CreateDirectory(testPath);
                    }

                    entry.ExtractToFile(Path.Combine(wowpath, entry.FullName), true);
                }
            }
            zip.Dispose();

            File.Delete("addon.zip");
        }

        private bool SkipUpdate(Addon addon, string dl_version)
        {
            if (addon.Version != dl_version)
            {
                string sql = $"UPDATE addons SET version = \"{dl_version}\" WHERE name = \"{addon.Name}\"";
                SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
                command.ExecuteNonQuery();
                return false;
            }
            else
            {
                return true;
            }
        }

        public void Dispose()
        {
            CloseHandler();
        }
    }
}
