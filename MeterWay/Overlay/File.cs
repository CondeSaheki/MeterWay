
using System;
using System.IO;
using Newtonsoft.Json;

namespace MeterWay.Overlay;

static class File
{
    public static Configuration Load<Configuration>(string fileName) where Configuration : IConfiguration, new()
    {
        Configuration config;
        var file = GetFile($"{fileName}.json");
        if (file != null && file.Exists)
        {
            try
            {
                Dalamud.Log.Debug($"Reading: {file.FullName}");
                var fileContent = System.IO.File.ReadAllText(file.FullName);
                config = JsonConvert.DeserializeObject<Configuration>(fileContent)!;
                if (config != null) return config;
            }
            catch (Exception ex)
            {
                Dalamud.Log.Warning($"Error loading configuration from \'{file.FullName}\':\n{ex}");
            }
        }
        config = new();
        Save(fileName, config);
        return config;
    }

    public static void Save<Configuration>(string fileName, Configuration config) where Configuration : IConfiguration, new()
    {
        var file = GetFile($"{fileName}.json");
        if (file == null) return;

        try
        {
            Dalamud.Log.Debug($"Writing: {file.FullName}");
            var fileContent = JsonConvert.SerializeObject(config, Formatting.Indented);
            System.IO.File.WriteAllText(file.FullName, fileContent);
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"Could not write file {file.FullName}:\n{ex}");
        }
    }

    public static void Delete(string fileName)
    {
        var file = GetFile($"{fileName}.json");
        if (file == null || !file.Exists)
        {
            Dalamud.Log.Warning($"File '{fileName}.json' not found.");
            return;
        }

        try
        {
            file.Delete();
            Dalamud.Log.Debug($"Deleted: {file.FullName}");
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"Error deleting file '{file.FullName}':\n{ex}");
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
            Dalamud.Log.Error($"Could not create directory: {Path.FullName}:\n{ex}");
            return null;
        }

        return new FileInfo(System.IO.Path.Combine(Path.FullName, fileName));
    }
}