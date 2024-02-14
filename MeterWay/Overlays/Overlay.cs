using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Collections.Generic;
using Dalamud.Game.Gui.PartyFinder.Types;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System.Globalization;

namespace Meterway.Overlays;

public class Overlay : IOverlay
{
    public string Name { get; }

    protected Plugin plugin { get; init; }

    public Overlay(Plugin plugin, string name)
    {
        this.plugin = plugin;
        this.Name = name;
    }

    public virtual void Draw()
    {
        throw new NotImplementedException();
    }

    public virtual void Dispose()
    {
        throw new NotImplementedException();
    }
}

public interface IOverlay
{
    public string Name { get; }

    public void Draw();
    public void Dispose();
}
