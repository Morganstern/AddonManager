using System.Collections.Generic;
using AddonManager.Models;
using System.Data.SQLite;
using System.IO;
using IniParser.Model;
using System.Net;
using System.IO.Compression;
using System.Text;
using System;

namespace AddonManager
{
    public static class AddonHandler
    {
        

        public static List<Addon> GetAddons()
        {
            List<Addon> addons = new List<Addon>();

            SQLiteConnection dbConnection;
            dbConnection = new SQLiteConnection("Data Source=addons.sqlite;Version=3;");
            dbConnection.Open();

            string query = "SELECT * FROM ADDONS;";
            SQLiteCommand command = new SQLiteCommand(query, dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
                addons.Add(new Addon(reader["name"].ToString(), reader["version"].ToString(), reader["url"].ToString()));

            dbConnection.Close();
            return addons;
        }

        public static bool AddAddon(string name, string url)
        {
            List<Addon> currentAddons = new List<Addon>();
            currentAddons = GetAddons();

            foreach (var a in currentAddons)
                if (name == a.Name || url == a.URL)
                    return false;

            SQLiteConnection dbConnection;
            dbConnection = new SQLiteConnection("Data Source=addons.sqlite;Version=3;");
            dbConnection.Open();

            string version = "1.0.0"; //Placeholder until versioning is added
            string insert = $@"INSERT INTO ADDONS VALUES('{name}', '{version}', '{url}');";
            SQLiteCommand command = new SQLiteCommand(insert, dbConnection);
            command.ExecuteNonQuery();

            dbConnection.Close();
            return true;
        }

        public static void DownloadAddons(List<Addon> addons)
        {
            foreach (var addon in addons)
            {
                if (addon.Name == "ElvUI")
                    DownloadElvUI();
                else
                    DownloadCurse(addon);
            }
        }

        private static void DownloadCurse(Addon addon)
        {
            // Number of characters to seek past when parsing the download location of the zip file
            const int hrefPadding = 11;

            string download = "data-href";
            string content;
            char c = 'a';

            string url = $@"https://mods.curse.com/addons/wow/{addon.URL}/download";
            using (WebClient client = new WebClient())
            {
                content = client.DownloadString(url);
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

            GetAddonZip(url);
        }

        private static void DownloadElvUI()
        {
            string download = @"http://www.tukui.org/downloads/elvui-";
            string content;
            char c = 'a';

            string url = $@"http://www.tukui.org/dl.php";
            using (WebClient client = new WebClient())
            {
                content = client.DownloadString(url);
            }
            int i = content.IndexOf(download);

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

        public static void GetAddonZip(string url)
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
                if (entry.FullName.Contains(".gitlab") == false && entry.Name != "")
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
    }
}
