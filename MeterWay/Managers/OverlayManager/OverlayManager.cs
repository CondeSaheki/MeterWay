using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface.Windowing;

using MeterWay.Managers;

namespace MeterWay.Overlay;

[Serializable]
public class OverlayWindowSpec
{
    public required string Name { get; init; }
    public required string Autor { get; init; }
    public required uint Id { get; init; }
    public required string Comment { get; init; }
    public required bool IsEnabled { get; init; }
}

public class OverlayManager : IDisposable
{
    public Dictionary<uint, OverlayWindow> Windows { get; private set; } = [];
    public Type[] Types { get; private init; }

    private WindowSystem WindowSystem { get; init; } = new();
    private uint UniqueId;

    public OverlayManager(Type[] overlayTypes)
    {
#if DEBUG
        Check(overlayTypes);
#endif

        Types = overlayTypes;

        Load();

        UniqueId = Windows.Count != 0 ? Windows.Keys.Max() + 1 : 1;
        Dalamud.PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
    }

    public void Add(Type overlayType)
    {
        try
        {
            uint id = UniqueId++;
            OverlayWindow overlay = new(overlayType, id);
            Windows.Add(id, overlay);
            WindowSystem.AddWindow(overlay);
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"OverlayManager Add, name \'{overlayType.Name}\':\n{ex}");
            Dispose();
        }
    }

    public void Remove(uint id)
    {
        try
        {
            if (!Windows.TryGetValue(id, out var window)) throw new Exception($"Not found");
            WindowSystem?.RemoveWindow(window);
            window.Remove();
            window.Dispose();
            if (!Windows.Remove(id)) throw new Exception($"Not successfully removed");
            window = null;
        }
        catch (Exception ex)
        {
            Dalamud.Log.Error($"OverlayManager Remove, id \'{id}\':\n{ex}");
            Dispose();
        }
    }

    public void Dispose()
    {
        foreach (var id in Windows.Keys) Remove(id);
        if (WindowSystem != null) Dalamud.PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        WindowSystem?.RemoveAllWindows();
        Windows?.Clear();
    }

    public static BasicOverlayInfo GetInfo(Type type)
    {
        return (BasicOverlayInfo?)type.GetProperty("Info")?.GetValue(null) ?? throw new Exception("Missing Basic Overlay Information");
    }

    public static OverlayWindowSpec GetSpec(OverlayWindow window)
    {
        return new OverlayWindowSpec()
        {
            Name = window.Info.Name,
            Autor = window.Info.Author,
            Id = window.Id,
            Comment = window.Comment,
            IsEnabled = window.IsEnabled(),
        };
    }

    private void Load()
    {
        var infos = Types.Select(GetInfo).ToArray();
        foreach (var spec in ConfigurationManager.Inst.Configuration.Overlays)
        {
            var index = Array.FindIndex(infos, info => info.Name == spec.Name && info.Author == spec.Autor);
            if (index == -1)
            {
                Dalamud.Log.Warning($"OverlayManager Load, name \'{spec.Name}\', autor \'{spec.Autor}\', id \'{spec.Id}\': Not found.");
                continue;
            }

            try
            {
                var obj = new OverlayWindow(Types[index], spec);
                WindowSystem.AddWindow(obj);
                Windows.Add(spec.Id, obj);
            }
            catch (Exception ex)
            {
                Dalamud.Log.Error($"OverlayManager Load, name \'{spec.Name}\', autor \'{spec.Autor}\', id \'{spec.Id}\':\n{ex}.");
            }
        }
    }

    private static void Check(IEnumerable<Type> types)
    {
        if (types == null || !types.Any()) throw new Exception("OverlayManager Check: Type list is empty.");
        if (types.Distinct().Count() != types.Count()) throw new Exception("OverlayManager Check: Type list contains duplicate types.");

        var infos = new List<BasicOverlayInfo>();
        foreach (var type in types)
        {
            if (!typeof(BasicOverlay).IsAssignableFrom(type)) throw new Exception($"OverlayManager Check: {type.Name} does not implement {typeof(BasicOverlay).Name}.");
            var property = type.GetProperty("Info") ?? throw new Exception($"OverlayManager Check: {type.Name} does not implement property 'Info'.");
            if (property.PropertyType != typeof(BasicOverlayInfo)) throw new Exception($"OverlayManager Check: {type.Name} property 'Info' is not of type 'BasicOverlayInfo'.");
            var value = (BasicOverlayInfo)(property.GetValue(null) ?? throw new Exception($"OverlayManager Check: Property 'Info' in {type.Name} is null."));

            infos.Add(value);
        }

        var uniqueOverlays = new HashSet<(string Name, string Author)>();
        foreach (var info in infos)
        {
            if (!uniqueOverlays.Add((info.Name, info.Author))) throw new Exception($"OverlayManager Check: Name: \'{info.Name}\', Author: \'{info.Author}\', is not unique.");
        }
    }
}