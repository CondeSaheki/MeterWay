using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace MeterWay.Overlay;

/// <summary>
/// Abstract class representing an overlay with basic methods.
/// </summary>
public abstract class BasicOverlay : IDisposable
{
    /// <summary>
    /// Code to be executed when the encounter data updates.
    /// </summary>
    public virtual void OnEncounterUpdate() { }

    /// <summary>
    /// Code to be executed when the encounter begin.
    /// </summary>
    public virtual void OnEncounterBegin() { }

    /// <summary>
    /// Code to be executed when the encounter end.
    /// </summary>
    public virtual void OnEncounterEnd() { }

    /// <summary>
    /// Code to be executed every time the window renders.
    /// In this method, implement your drawing code. You do NOT need to ImGui.Begin your window
    /// </summary>
    public virtual void Draw() { }

    /// <summary>
    /// Code to be executed when the window is opened.
    /// </summary>
    public virtual void OnOpen() { }

    /// <summary>
    /// Code to be executed when the window is closed.
    /// </summary>
    public virtual void OnClose() { }

    /// <summary>
    /// Code to be executed every frame, even when the window is collapsed.
    /// </summary>
    public virtual void Update() { }

    /// <summary>
    /// Code to be executed every time the configuration window renders.
    /// In this method, implement your drawing code. You do NOT need to ImGui.Begin your window
    /// </summary>
    public virtual void DrawConfiguration() { }

    /// <summary>
    /// Performs cleanup tasks of persistent data like configuration files associated with the overlay.
    /// </summary>
    // public virtual void Remove() { }

    public virtual void Dispose() { }

    /// <summary>
    /// Loads the configuration from a specified JSON file.
    /// If the file does not exist or deserialization fails, a new configuration instance is created and saved.
    /// </summary>
    /// <typeparam name="Configuration">The type of the configuration, which must implement <see cref="IBasicOverlayConfiguration"/> and have a parameterless constructor.</typeparam>
    /// <param name="fileName">The name of the file (without extension) from which to load the configuration.</param>
    /// <returns>The loaded or newly created configuration.</returns>
    public virtual Configuration Load<Configuration>(string fileName) where Configuration : IBasicOverlayConfiguration, new()
    {
        Configuration config;
        var file = GetFile($"{fileName}.json");
        if (file != null && file.Exists)
        {
            try
            {
                Dalamud.Log.Debug($"File Load, file \'{file.FullName}\': Reading");
                var fileContent = File.ReadAllText(file.FullName);
                config = JsonConvert.DeserializeObject<Configuration>(fileContent, new JsonSerializerSettings
                {
                    Converters =
                    [
                        DalamudAssemblyTypeNameForcingJsonConverter.Instance,
                    ],
                })!;

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
    /// from Dalamud/Configuration/PluginConfigurations.cs
    /// </summary>
    private class DalamudAssemblyTypeNameForcingJsonConverter : JsonConverter
    {
        public static readonly DalamudAssemblyTypeNameForcingJsonConverter Instance = new();

        private static readonly Assembly DalamudAssembly = typeof(DalamudAssemblyTypeNameForcingJsonConverter).Assembly;

        private static readonly JsonSerializer TypeNameForcingJsonConverterDefaultSerializer = new()
        {
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            TypeNameHandling = TypeNameHandling.Objects,
        };

        private DalamudAssemblyTypeNameForcingJsonConverter()
        {
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) =>
            TypeNameForcingJsonConverterDefaultSerializer.Serialize(writer, value);

        public override object? ReadJson(
            JsonReader reader,
            Type objectType,
            object? existingValue,
            JsonSerializer serializer) =>
            TypeNameForcingJsonConverterDefaultSerializer.Deserialize(reader, objectType);

        public override bool CanConvert(Type objectType) => objectType.Assembly == DalamudAssembly;
    }

    /// <summary>
    /// Save the configuration as JSON in specified file name.
    /// </summary>
    /// <typeparam name="Configuration"></typeparam>
    /// <param name="fileName"></param>
    /// <param name="config"></param>
    public static void Save<Configuration>(string fileName, Configuration config) where Configuration : IBasicOverlayConfiguration, new()
    {
        var file = GetFile($"{fileName}.json");
        if (file == null) return;

        try
        {
            Dalamud.Log.Debug($"File Save, file \'{file.FullName}\': Writing");
            var fileContent = JsonConvert.SerializeObject(config, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
            File.WriteAllText(file.FullName, fileContent);
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

// public interface IOverlayCommandHandler
// {
//     public string CommandHelpMessage(string? command);
//     public Action? OnCommand(List<string> args);
// }