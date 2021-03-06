﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddonManager.Models
{
    public class Addon
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string URL { get; set; }
        public string TopLevelDirs { get; set; }

        public Addon(string _name, string _version, string _url, string _topLevelDirs)
        {
            Name = _name;
            Version = _version;
            URL = _url;
            TopLevelDirs = _topLevelDirs;
        }
    }
}
