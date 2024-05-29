using ImGuiNET;
using MeterWay.Data;
using MeterWay.Managers;
using MeterWay.Overlay;

namespace HelloWorld
{
    public class Overlay : IOverlay, IOverlayConfig
    {
        // Static properties providing basic information about the overlay
        public static string Name => "HelloWorld"; // required
        public static string Author => "MeterWay";
        public static string Description => "A simple overlay.";

        // Objects to hold the overlay's settings and The window where the overlay will be rendered
        private Configuration Config { get; init; }
        private IOverlayWindow Window { get; init; }

        // Object holding encounter information
        private Encounter? Data = new();

        // Constructor initializing the overlay
        public Overlay(IOverlayWindow overlayWindow)
        {
            Window = overlayWindow;
            Config = File.Load<Configuration>(Window.NameId);
        }

        // Get the current encounter data from the manager
        public void DataUpdate()
        {
            Data = EncounterManager.Inst.CurrentEncounter();
        }

        public void Draw()
        {
            if (Data == null) return; // no data avaliable

            ImGui.Text($"{Data.Name}, Welcome to this MeterWay overlay!");
        }

        public void DrawConfig()
        {
            ImGui.Text("Hey, I am a configuration window!\n");

            // Example checkbox
            var isEnabledValue = Config.IsEnabled;
            if (ImGui.Checkbox("Example configuration is enabled ", ref isEnabledValue))
            {
                Config.IsEnabled = isEnabledValue;
                File.Save(Name, Config);
            }
        }

        // Method to remove any presistent resources on overlay
        public void Remove()
        {
            File.Delete(Window.NameId);
        }

        // DO NOT forget to dispose of any resources
        public void Dispose() { }
    }
}