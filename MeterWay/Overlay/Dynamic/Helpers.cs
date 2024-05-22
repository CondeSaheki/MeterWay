
using ImGuiNET;

using MeterWay.Overlay;

namespace Dynamic;

public partial class Overlay : IOverlay, IOverlayConfig
{
    private void LoadScript()
    {
        if (Config.ScriptFile == null) return;
        Script?.Dispose();
        var file = new System.IO.FileInfo(Config.ScriptFile);
        if (!file.Exists)
        {
            Config.ScriptFile = null;
            File.Save(Window.NameId, Config);
            MeterWay.Dalamud.Log.Info("Dynamic Overlay LoadScript: File does not exist!");
            Window.IsOpen = false;
            return;
        }
        Script = new(file, Registers);
        Window.IsOpen = true;
    }

    private void UnLoadScript()
    {
        Script?.Dispose();
        Script = null;
        Window.IsOpen = false;
    }

    private void ReloadScript()
    {
        if (Config.ScriptFile == null || Script == null) return;
        if (!Script.FilePath.Exists)
        {
            if (Config.ScriptFile == Script.FilePath.FullName)
            {
                Config.ScriptFile = null;
                File.Save(Window.NameId, Config);
            }
            MeterWay.Dalamud.Log.Info("Dynamic Overlay ReloadScript: File does not exist!");
            Window.IsOpen = false;
            return;
        }
        Script.Reload();
        Window.IsOpen = true;
    }

    private bool StatusScript() => Script != null && Script.Status;

    private static void ImGui_Disable(ref bool disabled)
    {
        if (disabled) return;
        ImGui.BeginDisabled();
        disabled = true;
    }

    private static void ImGui_Enable(ref bool disabled)
    {
        if (!disabled) return;
        ImGui.EndDisabled();
        disabled = false;
    }

    public static void Text()
    {
        ImGui.Text("Hello from C#!");
    }
}