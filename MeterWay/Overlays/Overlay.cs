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

public interface IOverlay
{
    public string Name { get; }

    private Plugin plugin { get; init; }

    public IOverlay(Plugin plugin)
    {
        this.plugin = plugin;
    }


    public void Draw();
    public void Dispose();
}