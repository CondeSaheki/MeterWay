using ImGuiNET;
using MeterWay.Data;
using MeterWay.Managers;
using MeterWay.Overlay;

namespace HelloWorld
{
    public class Overlay : BasicOverlay
    {
        // REQUIRED static property providing basic information about the overlay
        public static BasicOverlayInfo Info => new()
        {
            Name = "HelloWorld", // required
            Author = "MeterWay", // required
            Description = "A simple overlay.",
        };

        // Objects to hold the overlay's settings and The window where the overlay will be rendered
        private Configuration Config { get; init; }
        private IOverlayWindow Window { get; init; }

        // Object holding encounter information
        private Encounter? Data;

        // Constructor initializing the overlay
        public Overlay(IOverlayWindow overlayWindow)
        {
            Window = overlayWindow;
            Config = Load<Configuration>(Window.WindowName);
        }

        // Get the current encounter data from the manager
        public override void OnEncounterUpdate()
        {
            Data = EncounterManager.Inst.CurrentEncounter();
        }

        public override void Draw()
        {
            if (Data == null) return; // no data avaliable

            ImGui.Text($"{Data.Name}, Welcome to this MeterWay overlay!");
        }

        public override void DrawConfiguration()
        {
            ImGui.Text("Hey, I am a configuration window!\n");

            // Example checkbox
            var isEnabledValue = Config.IsEnabled;
            if (ImGui.Checkbox("Example configuration is enabled ", ref isEnabledValue))
            {
                Config.IsEnabled = isEnabledValue;
                Save(Window.WindowName, Config);
            }
        }

        // Method to remove any presistent resources on overlay
        public override void Remove()
        {
            Delete(Window.WindowName);
        }

        // DO NOT forget to dispose of any resources used
        public override void Dispose() { }
    }
}