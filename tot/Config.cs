using System.Text.Json;
using System.Text.Json.Serialization;
using tot_lib;

namespace Tot;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(Config))]
public partial class ConfigJsonContext : JsonSerializerContext
{
}

public class Config : ITotService
{
    private const string ConfigFileName = "Tot.json";

    public Config()
    {
        DevKitPath = "";
    }

    public string DevKitPath { get; set; }

    public bool AutoBumpBuild { get; set; }

    public string GitAuthorName { get; set; } = "Tot Chef";

    public string GitAuthorEmail { get; set; } = "no@email.com";

    public string DefaultCliEditor { get; set; } = "nano";

    [JsonIgnore] public bool IsValid => !string.IsNullOrEmpty(DevKitPath);

    public static Config LoadConfig()
    {
        var configPath = GetConfigPath() ?? "";
        var json = "";
        if (File.Exists(configPath))
            try
            {
                json = File.ReadAllText(configPath);
            }
            catch
            {
                return new Config();
            }

        if (string.IsNullOrEmpty(json))
            return new Config();

        var config = JsonSerializer.Deserialize(json, ConfigJsonContext.Default.Config) ??
                     new Config { DevKitPath = "" };
        return config;
    }

    public void SaveConfig()
    {
        var json = JsonSerializer.Serialize(this, ConfigJsonContext.Default.Config);
        var configPath = GetConfigPath() ?? "";
        File.WriteAllText(configPath, json);
    }

    internal static string? GetConfigPath()
    {
        var configPath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
        if (string.IsNullOrEmpty(configPath))
            return null;
        configPath = Path.Combine(configPath, ConfigFileName);
        return configPath;
    }

    public void SetValue(string key, string value)
    {
        switch (key)
        {
            case "DevKitPath":
                DevKitPath = value;
                break;
            case "AutoBumpBuild":
                if (!bool.TryParse(value, out var b))
                    throw new CommandException(CommandCode.MissingArgument, "Invalid value for AutoBumpBuild");
                AutoBumpBuild = b;
                break;
            case "GitAuthorName":
                GitAuthorName = value;
                break;
            case "GitAuthorEmail":
                GitAuthorEmail = value;
                break;
            case "DefaultCliEditor":
                DefaultCliEditor = value;
                break;
            default:
                throw new CommandException(CommandCode.MissingArgument, $"Invalid key: {key}");
        }
    }

    public IEnumerable<string> GetKeyList()
    {
        return
        [
            "DevKitPath",
            "AutoBumpBuild",
            "GitAuthorName",
            "GitAuthorEmail",
            "DefaultCliEditor"
        ];
    }

    public string GetValue(string key)
    {
        switch (key)
        {
            case "DevKitPath":
                return DevKitPath;
            case "AutoBumpBuild":
                return AutoBumpBuild.ToString();
            case "GitAuthorName":
                return GitAuthorName;
            case "GitAuthorEmail":
                return GitAuthorEmail;
            case "DefaultCliEditor":
                return DefaultCliEditor;
            default:
                throw new CommandException(CommandCode.MissingArgument, $"Invalid key: {key}");
        }
    }
}