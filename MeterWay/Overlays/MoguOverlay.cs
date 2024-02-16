using System;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using System.Collections.Generic;
using MeterWay.Utils;

namespace MeterWay.Overlays;

public class MoguOverlay : IMeterwayOverlay
{
    public string Name => "MoguOverlay";
    private Plugin plugin;
    private Encounter data;


    public MoguOverlay(Plugin plugin)
    {
        this.plugin = plugin;
        data = new Encounter(this.plugin);
    }

    public void DataProcess(Encounter data)
    {
        this.data = data;
    }

    public void Draw()
    {

    }

    public void Dispose() { }


}