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
using System.Linq;

namespace AddonManager
{
    public class AddonHandler : IDisposable
    {
        private SQLiteConnection dbConnection { get; set; }
        string wowpath;

        public AddonHandler()
        {
            // Set wowpath
            IniData config = ConfigHandler.GetConfig();
            wowpath = config["DIRECTORY"]["WowInstall"].Replace("\"", "");

            // Create SQLite database if it does not exist
            if (!File.Exists("addons.sqlite"))
            {
                SQLiteConnection.CreateFile("addons.sqlite");
                dbConnection = new SQLiteConnection("Data Source=addons.sqlite;Version=3;");
                dbConnection.Open();
                string sql = "create table addons (name varchar(255), version varchar(255), url varchar(255), topLevelDirs varchar(2048));";
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
                addons.Add(new Addon(reader["name"].ToString(), reader["version"].ToString(), reader["url"].ToString(), reader["topLevelDirs"].ToString()));
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
            string insert = $@"INSERT INTO ADDONS VALUES('{name}', '{version}', '{url}', '');";
            SQLiteCommand command = new SQLiteCommand(insert, dbConnection);
            command.ExecuteNonQuery();

            return true;
        }

        public void RemoveAddon(string selectedAddon)
        {
            UninstallAddon(selectedAddon);
            string query = $"DELETE FROM addons WHERE name = \"{selectedAddon}\"";
            SQLiteCommand command = new SQLiteCommand(query, dbConnection);
            command.ExecuteNonQuery();
        }

        private void UninstallAddon(string selectedAddon)
        {
            List<Addon> Addons = new List<Addon>();
            Addons = GetAddons();

            foreach (var a in Addons)
            {
                if (a.Name == selectedAddon)
                {
                    List<string> dirs = a.TopLevelDirs.Split(',').ToList();
                    foreach (var dir in dirs)
                        Directory.Delete(Path.Combine(wowpath, dir), true);
                }
            }
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
            // Get path to WoW
            

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

            List<string> dirs = new List<string>();

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

                    // Log top level dirs
                    string topLevelDir = entry.FullName.Replace(entry.Name, String.Empty);

                    // Cut off sub-folders
                    int i = topLevelDir.IndexOf('/');
                    topLevelDir = topLevelDir.Substring(0, i);

                    if (!dirs.Contains(topLevelDir))
                        dirs.Add(topLevelDir);
                }
            }
            zip.Dispose();
            File.Delete("addon.zip");
            LogDirs(name, dirs);
        }

        private void LogDirs(string name, List<string> dirs)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var dir in dirs)
            {
                sb.Append(dir);
                sb.Append(',');
            }
            string dirList = sb.ToString().Substring(0, sb.Length - 1);
            string sql = $"UPDATE addons SET topLevelDirs = \"{dirList}\" WHERE name = \"{name}\"";
            SQLiteCommand command = new SQLiteCommand(sql, dbConnection);
            command.ExecuteNonQuery();
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
