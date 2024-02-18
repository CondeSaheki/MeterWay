using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Numerics;

namespace MeterWay
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        // General

        // public int Interaction { get; set; } = 1;
        // public bool SaveCombats { get; set; } = true;
        // public int Combats { get; set; } = 5;
        // public bool CombatClose { get; set; } = true;
        // public bool PvP { get; set; } = true;

        //overlay
        public bool Overlay { get; set; } = true;
        public bool OverlayClickThrough { get; set; } = false;
        public bool OverlayBackground { get; set; } = true;
        public Vector4 OverlayBackgroundColor { get; set; } = new Vector4(0f, 0f, 0f, 0.25f);
        public string OverlayFontPath { get; set; } = "Inter-Bold.ttf";
        public float OverlayFontSize { get; set; } = 15f;
        public float OverlayFontScale { get; set; } = 1f;
        public int OverlayType { get; set; } = 0;

        // Aparence


        // the below exist just to make saving less cumbersome
        [NonSerialized]
        private DalamudPluginInterface? pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.pluginInterface!.SavePluginConfig(this);
        }
    }
}
