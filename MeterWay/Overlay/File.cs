
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