using IniParser;
using IniParser.Model;
using System;
using System.IO;
using System.Windows.Forms;

namespace AddonManager
{
    public static class ConfigHandler
    {
        public static void CreateConfig()
        {
            string defaultPath = @"C:\Program Files (x86)\World of Warcraft\Interface\AddOns\";
            if (Directory.Exists(defaultPath))
                WriteConfigFile(defaultPath);

            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select your World of Warcraft folder";
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    string path = fbd.SelectedPath + "\\Interface\\Addons\\";
                    WriteConfigFile(path);
                }
            }
        }

        private static void WriteConfigFile(string path)
        {
            string lines = $"[DIRECTORY]\r\nWowInstall=\"{path}\"";
            StreamWriter file = new StreamWriter("config.ini");
            file.WriteLine(lines);
            file.Close();
        }

        public static void SetConfig(IniData config)
        {
            var parser = new FileIniDataParser();
            parser.WriteFile("config.ini", config);
        }

        public static IniData GetConfig()
        {
            var parser = new FileIniDataParser();
            return parser.ReadFile("config.ini");
        }
    }
}
