using System;
using System.IO;
using Newtonsoft.Json;

namespace MeterWay.Overlay;

public abstract class MeterWayOverlay
{
    public static string Name() => string.Empty;
    public abstract void Draw();
    public abstract void Dispose();
    public abstract void DataProcess();
    public virtual void DrawConfigurationTab() { }
    public virtual bool HasConfigurationTab => false;

    public static Configuration Load<Configuration>() where Configuration : MeterWayOverlayConfiguration, new()
    {
        Configuration config;
        var file = GetFile(Name() + ".json");
        if (file != null && file.Exists)
        {
            try
            {
                var fileContent = File.ReadAllText(file.FullName);
                config = JsonConvert.DeserializeObject<Configuration>(fileContent)!;
                if (config != null) return config;     
            }
            catch (Exception ex)
            {
                Dalamud.Log.Warning($"Error loading configuration from \'{file.FullName}\':\n{ex}");
            }
        }
        config = new();
        Save(config);
        return config;
    }

    public static void Save<Configuration>(Configuration config) where Configuration : MeterWayOverlayConfiguration, new()
    {
        var file = GetFile(Name() + ".json");
        if (file == null) return;

        try
        {
            var fileContent = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(file.FullName, fileContent);
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"Could not write file {file.FullName}:\n{ex}");
        }
    }

    internal static FileInfo? GetFile(string fileName)
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

public class MeterWayOverlayConfiguration : Attribute { }