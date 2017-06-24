using AddonManager.Models;
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

        public static IniData GetConfig()
        {
            var parser = new FileIniDataParser();
            return parser.ReadFile("config.ini");
        }
    }
}
