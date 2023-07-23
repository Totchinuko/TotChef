using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace TotChef
{
    internal class Config
    {
        public string DevKitPath { get; set; }
        [JsonIgnore]
        public bool IsValid => !string.IsNullOrEmpty(DevKitPath);

        private static string ConfigFileName => "TotChef.json";

        public Config() 
        { 
            DevKitPath = "";
        }

        public KitchenClerk MakeClerk(string modFolder)
        {
            return new KitchenClerk(DevKitPath, modFolder);
        }

        public static Config LoadConfig()
        {
            string configPath = GetConfigPath();
            string json = "";
            if (File.Exists(configPath))
            {
                json = File.ReadAllText(configPath);
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
            string configPath = GetConfigPath();
            Console.WriteLine(configPath);
            File.WriteAllText(configPath, json);
        }

        private static string GetConfigPath()
        {
            string? configPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (configPath == null || !Directory.Exists(configPath))
            {
                throw new Exception("Application path cannot be accessed");
            }
            configPath = Path.Combine(configPath, ConfigFileName);
            return configPath;
        }
    }
}
