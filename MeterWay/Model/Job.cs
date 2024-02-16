using System.Collections.Generic;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using ImGuiNET;
using MeterWay.managers;

public static class Job
{
    public static Dictionary<uint, string> Ids = new Dictionary<uint, string>()
    {
        { 0, "ADV" },
        { 1, "GLA" },
        { 2, "PGL" },
        { 3, "MRD" },
        { 4, "LNC" },
        { 5, "ARC" },
        { 6, "CNJ" },
        { 7, "THM" },
        { 8, "CRP" },
        { 9, "BSM" },
        { 10, "ARM" },
        { 11, "GSM" },
        { 12, "LTW" },
        { 13, "WVR" },
        { 14, "ALC" },
        { 15, "CUL" },
        { 16, "MIN" },
        { 17, "BTN" },
        { 18, "FSH" },
        { 19, "PLD" },
        { 20, "MNK" },
        { 21, "WAR" },
        { 22, "DRG" },
        { 23, "BRD" },
        { 24, "WHM" },
        { 25, "BLM" },
        { 26, "ACN" },
        { 27, "SMN" },
        { 28, "SCH" },
        { 29, "ROG" },
        { 30, "NIN" },
        { 31, "MCH" },
        { 32, "DRK" },
        { 33, "AST" },
        { 34, "SAM" },
        { 35, "RDM" },
        { 36, "BLU" },
        { 37, "GNB" },
        { 38, "DNC" },
        { 39, "RPR" },
        { 40, "SGE" }
    };

    public static string GetName(uint id)
    {
        if (Ids.ContainsKey(id))
        {
            return Ids[id];
        }

        return "UNK";
    }

    public static nint GetIcon(uint job)
    {
        var icon = PluginManager.Instance.TextureProvider.GetIcon(job + 62000u, Dalamud.Plugin.Services.ITextureProvider.IconFlags.None);
        if (icon == null) return 0;

        return icon.ImGuiHandle;
    }
}