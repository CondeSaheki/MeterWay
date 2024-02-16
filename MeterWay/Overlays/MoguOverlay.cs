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
    private Encounter data;

    public MoguOverlay(Plugin plugin)
    {
        data = new Encounter();
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