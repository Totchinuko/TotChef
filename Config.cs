using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Tot;

namespace Tot
{
    internal class Config
    {
        public string DevKitPath { get; set; }
        [JsonIgnore]
        public bool IsValid => !string.IsNullOrEmpty(DevKitPath);

        private const string ConfigFileName = "Tot.json";

        public Config() 
        { 
            DevKitPath = "";
        }

        public static Config LoadConfig()
        {
            string configPath = GetConfigPath() ?? "";
            string json = "";
            if (File.Exists(configPath))
            {
                try
                {
                    json = File.ReadAllText(configPath);
                }
                catch { }
            }
            if (string.IsNullOrEmpty(json))
                return new Config();

            Config config = JsonSerializer.Deserialize<Config>(json) ?? new Config() { DevKitPath = "" };
            return config;
        }

        public void SaveConfig()
        {
            JsonSerializerOptions option = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(this, option);
            string? configPath = GetConfigPath() ?? "";
            File.WriteAllText(configPath, json);
        }

        internal static string? GetConfigPath()
        {
            string? configPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (string.IsNullOrEmpty(configPath))
                return null;
            configPath = Path.Combine(configPath, ConfigFileName);
            return configPath;
        }
    }
}
