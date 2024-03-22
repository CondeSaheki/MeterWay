
using ImGuiNET;

using MeterWay.Overlay;

namespace Dynamic;

public partial class Overlay : IOverlay, IOverlayTab
{
    public void LoadScript()
    {
        Script?.Dispose();
        if(Config.ScriptName == null || Config.Scripts.Count == 0) return;
        if (Config.Scripts.TryGetValue(Config.ScriptName, out string? path))
        {
            Script = new LuaScript(Config.ScriptName, path, Registers);
        }
    }
    
    public static void Text()
    {
        ImGui.Text("Hello from C#!");
    }
}