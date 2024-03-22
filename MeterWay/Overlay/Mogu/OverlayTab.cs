using System.Linq;
using Dalamud.Interface.ImGuiFontChooserDialog;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

using MeterWay.Overlay;
using MeterWay.Utils;
using MeterWay.Windows;

namespace Mogu;

public partial class Overlay : IOverlay, IOverlayTab
{
    public void DrawTab()
    {
        using var bar = ImRaii.TabBar("Overlay Settings Tabs");
        if (!bar) return;

        DrawGeneralTab();
        DrawAppearanceTab();
        DrawFontsTab();
        DrawJobColorsTab();
    }

    private void DrawGeneralTab()
    {
        using var tab = ImRaii.TabItem("General");
        if (!tab) return;

        ImGui.Spacing();

        var clickThroughValue = Config.ClickThrough;
        if (ImGui.Checkbox("Click Through", ref clickThroughValue))
        {
            Config.ClickThrough = clickThroughValue;
            File.Save(Name, Config);
            if (clickThroughValue) Window.Flags = OverlayWindow.defaultflags | ImGuiWindowFlags.NoInputs;
            else Window.Flags = OverlayWindow.defaultflags;
        }

        var frameCalcValue = Config.FrameCalc;
        if (ImGui.Checkbox("Calculate per frame", ref frameCalcValue))
        {
            Config.FrameCalc = frameCalcValue;
            File.Save(Name, Config);
        }
    }

    private void DrawAppearanceTab()
    {
        using var tab = ImRaii.TabItem("Appearance");
        if (!tab) return;

        ImGui.Spacing();

        if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.None))
        {
            var BackgroundValue = Config.Background;
            if (ImGui.Checkbox("Background", ref BackgroundValue))
            {
                Config.Background = BackgroundValue;
                File.Save(Name, Config);
            }

            if (Config.Background)
            {
                var OverlayBackgroundColorValue = ImGui.ColorConvertU32ToFloat4(Config.BackgroundColor);
                if (ImGui.ColorEdit4("Background Color", ref OverlayBackgroundColorValue,
                    ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.OptionsDefault)) // ImGuiColorEditFlags.NoLabel
                {
                    Config.BackgroundColor = ImGui.ColorConvertFloat4ToU32(OverlayBackgroundColorValue);
                    File.Save(Name, Config);
                }
            }
        }
        if (ImGui.CollapsingHeader("Header", ImGuiTreeNodeFlags.None))
        {
            var HeaderValue = Config.Header;
            if (ImGui.Checkbox("Header", ref HeaderValue))
            {
                Config.Header = HeaderValue;
                File.Save(Name, Config);
            }
            if (Config.Header)
            {
                var HeaderAlignmentValue = (int)Config.HeaderAlignment;
                if (DrawAlingmentButtons(ref HeaderAlignmentValue))
                {
                    Config.HeaderAlignment = (Canvas.HorizontalAlign)HeaderAlignmentValue;
                    File.Save(Name, Config);
                }

                var HeaderBackgroundValue = Config.HeaderBackground;
                if (ImGui.Checkbox("Header Background", ref HeaderBackgroundValue))
                {
                    Config.HeaderBackground = HeaderBackgroundValue;
                    File.Save(Name, Config);
                }
                if (Config.HeaderBackground)
                {
                    var HeaderBackgroundColorValue = ImGui.ColorConvertU32ToFloat4(Config.HeaderBackgroundColor);
                    if (ImGui.ColorEdit4("Header Background Color", ref HeaderBackgroundColorValue,
                        ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.OptionsDefault)) // ImGuiColorEditFlags.NoLabel
                    {
                        Config.HeaderBackgroundColor = ImGui.ColorConvertFloat4ToU32(HeaderBackgroundColorValue);
                        File.Save(Name, Config);
                    }
                }
            }
        }
        if (ImGui.CollapsingHeader("Player", ImGuiTreeNodeFlags.None))
        {
            var JobIconsValue = Config.PlayerJobIcon;
            if (ImGui.Checkbox("Job Icon", ref JobIconsValue))
            {
                Config.PlayerJobIcon = JobIconsValue;
                File.Save(Name, Config);
            }

            var BarValue = Config.Bar;
            if (ImGui.Checkbox("Bar", ref BarValue))
            {
                Config.Bar = BarValue;
                File.Save(Name, Config);
            }
            if (Config.Bar)
            {
                var BarColorJobValue = Config.BarColorJob;
                if (ImGui.Checkbox("job colors", ref BarColorJobValue))
                {
                    Config.BarColorJob = BarColorJobValue;
                    File.Save(Name, Config);
                }
                if (!Config.BarColorJob)
                {
                    var BarColorValue = ImGui.ColorConvertU32ToFloat4(Config.BarColor);
                    if (ImGui.ColorEdit4("Color", ref BarColorValue,
                        ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.OptionsDefault)) // ImGuiColorEditFlags.NoLabel
                    {
                        Config.BarColor = ImGui.ColorConvertFloat4ToU32(BarColorValue);
                        File.Save(Name, Config);
                    }
                }
            }
        }
    }

    private void DrawFontsTab()
    {
        using var tab = ImRaii.TabItem("Fonts");
        if (!tab) return;

        ImGui.Spacing();
        ImGui.Text("General font, used in all overlay texts");
        ImGui.Spacing();

        if (ImGui.Button("Choose"))
        {
            SingleFontChooserDialog chooser = new(MeterWay.Dalamud.PluginInterface.UiBuilder, false, null)
            {
                Title = "Font Chooser",
                PreviewText = "0.123456789 abcdefghijklmnopqrstuvxyzw",
                //SelectedFont = new SingleFontSpec { FontId = DalamudDefaultFontAndFamilyId.Instance } // load id from config
                // FontFamilyExcludeFilter = x => x is DalamudDefaultFontAndFamilyId; // exclude especific fonts from being selected
                // FontId = original.FontId;
                // SizePx = original.SizePx;
            };
            chooser.ResultTask.ContinueWith(chooserTask =>
            {
                if (chooserTask.IsCompletedSuccessfully)
                {
                    FontMogu?.Dispose();
                    FontMogu = chooserTask.Result.CreateFontHandle(FontAtlas);
                    // TODO save config
                }
                MeterWay.Dalamud.PluginInterface.UiBuilder.Draw -= chooser.Draw;
                chooser.Dispose();
            });
            MeterWay.Dalamud.PluginInterface.UiBuilder.Draw += chooser.Draw;
        }
        ImGui.SameLine();
        if (ImGui.Button("Default"))
        {
            FontMogu?.Dispose();
            FontMogu = FontAtlas.NewDelegateFontHandle(e => e.OnPreBuild(tk => tk.AddDalamudDefaultFont(20)));
        }

        var FontColorValue = ImGui.ColorConvertU32ToFloat4(Config.MoguFontColor);
        if (ImGui.ColorEdit4("Color", ref FontColorValue,
            ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.OptionsDefault))
        {
            Config.MoguFontColor = ImGui.ColorConvertFloat4ToU32(FontColorValue);
            File.Save(Name, Config);
        }
        // ImGui.Spacing();
        // ImGui.Text("Font Tester");
        // ImGui.Spacing();
        // FontMogu.Push();
        // string text = "abcdefghijklmnopqrstuvxyzw";
        // ImGui.InputText("##text", ref text, 200);
        // FontMogu.Pop();
    }

    private void DrawJobColorsTab()
    {
        using var tab = ImRaii.TabItem("Job Colors");
        if (!tab) return;

        ImGui.Spacing();
        ImGui.Text("Job Colors");
        ImGui.Spacing();

        ImGui.BeginTable("##ColorTable", 4, ImGuiTableFlags.SizingFixedFit); // ImGuiTableFlags.BordersV | ImGuiTableFlags.BordersOuter | ImGuiTableFlags.RowBg
        for (var i = 0; i != Config.JobColors.Length; ++i)
        {
            ImGui.TableNextColumn();
            var ColorValue = ImGui.ColorConvertU32ToFloat4(Config.JobColors[i].Color);
            if (ImGui.ColorEdit4(Config.JobColors[i].Job.ToString(), ref ColorValue,
                ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.OptionsDefault)) // ImGuiColorEditFlags.NoLabel
            {
                Config.JobColors[i].Color = ImGui.ColorConvertFloat4ToU32(ColorValue);
                File.Save(Name, Config);
            }
        }
        ImGui.EndTable();
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
        var DefaultColorValue = ImGui.ColorConvertU32ToFloat4(Config.JobDefaultColor);
        if (ImGui.ColorEdit4("Job Default Color", ref DefaultColorValue,
            ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.OptionsDefault)) // ImGuiColorEditFlags.NoLabel
        {
            Config.JobDefaultColor = ImGui.ColorConvertFloat4ToU32(DefaultColorValue);
            File.Save(Name, Config);
        }
    }
}