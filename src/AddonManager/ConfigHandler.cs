using IniParser;
using IniParser.Model;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace AddonManager
{
    public static class ConfigHandler
    {
        public static void SetConfig(IniData config)
        {
            var parser = new FileIniDataParser();
            parser.WriteFile("config.ini", config);
        }

        public static void SetAddons(List<string> addons)
        {
            //const string filepath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\AddonManager\\addons.txt";
            const string filepath = "addons.txt";
            StreamWriter file = new StreamWriter(filepath);
            foreach (var a in addons)
                file.WriteLine(a);
            file.Close();
        }

        public static IniData GetConfig()
        {
            var parser = new FileIniDataParser();
            return parser.ReadFile("config.ini");
        }

        public static List<string> GetAddons()
        {
            //const string filepath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\AddonManager\\addons.txt";

            // Plumbing for moving datafile from txt to sqlite
            /*
            SQLiteConnection m_dbConnection;
            m_dbConnection = new SQLiteConnection("Data Source=addons.sqlite;Version=3;");
            m_dbConnection.Open();
            m_dbConnection.Close();
            */

            const string filepath = "addons.txt";
            List<string> addons = new List<string>();

            var lines = File.ReadAllLines(filepath);
            foreach (var line in lines)
                addons.Add(line);
            return addons;


        }
    }
}
