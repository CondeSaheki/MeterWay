
using System;
using System.IO;
using Newtonsoft.Json;

namespace MeterWay.Overlay;

/// <summary>
/// Provides methods for loading, saving, and deleting configuration files.
/// </summary>
public static class File
{
    /// <summary>
    /// Loads the configuration from a specified JSON file.
    /// If the file does not exist or deserialization fails, a new configuration instance is created and saved.
    /// </summary>
    /// <typeparam name="Configuration">The type of the configuration, which must implement <see cref="IConfiguration"/> and have a parameterless constructor.</typeparam>
    /// <param name="fileName">The name of the file (without extension) from which to load the configuration.</param>
    /// <returns>The loaded or newly created configuration.</returns>
    public static Configuration Load<Configuration>(string fileName) where Configuration : IConfiguration, new()
    {
        Configuration config;
        var file = GetFile($"{fileName}.json");
        if (file != null && file.Exists)
        {
            try
            {
                Dalamud.Log.Debug($"File Load, file \'{file.FullName}\': Reading");
                var fileContent = System.IO.File.ReadAllText(file.FullName);
                config = JsonConvert.DeserializeObject<Configuration>(fileContent)!;
                if (config != null) return config;
            }
            catch (Exception ex)
            {
                Dalamud.Log.Warning($"File Load, file \'{file.FullName}\':\n{ex}");
            }
        }
        config = new();
        Save(fileName, config);
        return config;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="Configuration"></typeparam>
    /// <param name="fileName"></param>
    /// <param name="config"></param>
    public static void Save<Configuration>(string fileName, Configuration config) where Configuration : IConfiguration, new()
    {
        var file = GetFile($"{fileName}.json");
        if (file == null) return;

        try
        {
            Dalamud.Log.Debug($"File Save, file \'{file.FullName}\': Writing");
            var fileContent = JsonConvert.SerializeObject(config, Formatting.Indented);
            System.IO.File.WriteAllText(file.FullName, fileContent);
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"File Save, file \'{file.FullName}\':\n{ex}");
        }
    }

    /// <summary>
    /// Deletes the specified JSON file.
    /// </summary>
    /// <param name="fileName">The name of the file (without extension) to delete.</param>
    public static void Delete(string fileName)
    {
        var file = GetFile($"{fileName}.json");
        if (file == null || !file.Exists)
        {
            Dalamud.Log.Warning($"File Delete, file \'{fileName}.json\': Not found.");
            return;
        }

        try
        {
            file.Delete();
            Dalamud.Log.Debug($"File Delete, file \'{fileName}.json\': Done");
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"File Delete, file \'{fileName}.json\':\n{ex}");
        }
    }

    private static FileInfo? GetFile(string fileName)
    {
        var Path = new DirectoryInfo(Dalamud.PluginInterface.GetPluginConfigDirectory());

        if (Path.Exists) return new FileInfo(System.IO.Path.Combine(Path.FullName, fileName));

        try
        {
            Path.Create();
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"GetFile, path \'{Path.FullName}\':\n{ex}");
            return null;
        }

        return new FileInfo(System.IO.Path.Combine(Path.FullName, fileName));
    }
}