using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json;
using System.IO;

namespace NBandcc
{
    class Config
    {
        public string ServerUrl { get; set; }
        public string FfmpagePath { get; set; }
        public  int Perlen { get; set; }
        public  int ConnTimeOut { get; set; }
        public int WriteTimeOut { get; set; }
       
      
    }
    class ConfigHelper
    {
        public static Config mConfig;
        public static string path = "config.json";
        public static void DefaultConfig()
        {
            mConfig = new Config();
            mConfig.ServerUrl = "http://111.53.74.132:5002";
            mConfig.FfmpagePath = "../ffmpeg-4.1.3/";
            mConfig.Perlen = 10;
            mConfig.ConnTimeOut = 10;
            mConfig.WriteTimeOut = 180;
            Save();
        }
        public static void Save()
        {
            if (mConfig == null) return;
            string json = JsonConvert.SerializeObject(mConfig, Formatting.Indented);
            File.WriteAllText(path, json);
        }
        public static void ReadConfig()
        {
            if (!File.Exists(path))
            {
                DefaultConfig();
                return;
            }
            try
            {
                string txt = File.ReadAllText(path);
                if (!string.IsNullOrEmpty(txt))
                {
                    mConfig = JsonConvert.DeserializeObject<Config>(txt);
                }
                if (mConfig == null) DefaultConfig();
            }
            catch (Exception e)
            {
                DefaultConfig();
                return;
            }
        }
    }
}
