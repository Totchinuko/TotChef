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
    public string DevKitPath { get; set; } = "";

    public string GitBinary { get; set; } = "git";

    public bool AutoBumpBuild { get; set; }

    public string DefaultCliEditor { get; set; } = "nano";

    [JsonIgnore] public bool IsValid => !string.IsNullOrEmpty(DevKitPath);

    public static Config LoadConfig()
    {
        var configPath = GetConfigPath();
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

        return JsonSerializer.Deserialize(json, ConfigJsonContext.Default.Config) ?? new Config();
    }

    public void SaveConfig()
    {
        var json = JsonSerializer.Serialize(this, ConfigJsonContext.Default.Config);
        var configPath = GetConfigPath();
        var directory = Path.GetDirectoryName(configPath);
        if(directory is null)
            throw new DirectoryNotFoundException(directory);
        Directory.CreateDirectory(directory);
        File.WriteAllText(configPath, json);
    }

    private static string GetConfigPath()
    {
        var directory = typeof(Config).GetStandardFolder(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(directory.FullName, Constants.ConfigFileName);
    }

    public void SetValue(string key, string value)
    {
        switch (key)
        {
            case nameof(DevKitPath):
                DevKitPath = value;
                break;
            case nameof(AutoBumpBuild):
                if (!bool.TryParse(value, out var b))
                    throw new Exception("Invalid value for AutoBumpBuild");
                AutoBumpBuild = b;
                break;
            case nameof(DefaultCliEditor):
                DefaultCliEditor = value;
                break;
            case nameof(GitBinary):
                GitBinary = value;
                break;
            default:
                throw new Exception($"Invalid key: {key}");
        }
    }

    public IEnumerable<string> GetKeyList()
    {
        return
        [
            nameof(DevKitPath),
            nameof(AutoBumpBuild),
            nameof(DefaultCliEditor),
            nameof(GitBinary)
        ];
    }

    public string GetValue(string key)
    {
        return key switch
        {
            nameof(DevKitPath) => DevKitPath,
            nameof(AutoBumpBuild) => AutoBumpBuild.ToString(),
            nameof(DefaultCliEditor) => DefaultCliEditor,
            nameof(GitBinary) => GitBinary,
            _ => throw new Exception($"Invalid key: {key}")
        };
    }
}