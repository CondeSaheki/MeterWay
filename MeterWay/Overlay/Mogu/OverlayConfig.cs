using System;
using Dalamud.Interface.FontIdentifier;
using Dalamud.Interface.ImGuiFontChooserDialog;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

using MeterWay.Overlay;
using MeterWay.Utils;

namespace Mogu;

public partial class Overlay : IOverlay, IOverlayConfig
{
    public void DrawConfig()
    {
        using var bar = ImRaii.TabBar("Overlay Settings Tabs");
        if (!bar) return;

        DrawGeneralTab();
        DrawAppearanceTab();
        DrawFontsTab();
        DrawJobColorsTab();
        DrawVisibilityTab();
        DrawAboutTab();
    }

    private void DrawVisibilityTab()
    {
        using var tab = ImRaii.TabItem("Visibility");
        if (!tab) return;

        ImGui.Spacing();

        bool alwaysValue = Config.Always;
        if (ImGui.Checkbox("Always", ref alwaysValue))
        {
            Config.Always = alwaysValue;
            File.Save($"{Window.Name}{Window.Id}", Config);
        }

        if (alwaysValue) ImGui.BeginDisabled();

        bool combatValue = Config.Combat;
        if (ImGui.Checkbox("Combat", ref combatValue))
        {
            Config.Combat = combatValue;
            File.Save($"{Window.Name}{Window.Id}", Config);
        }

        bool delayValue = Config.Delay;
        if (ImGui.Checkbox("Dismmis after", ref delayValue))
        {
            Config.Delay = delayValue;
            File.Save($"{Window.Name}{Window.Id}", Config);
        }

        ImGui.SameLine();
        ImGui.PushItemWidth(70);
        float delayDurationValue = (float)Config.DelayDuration.TotalSeconds;
        if (ImGui.DragFloat("##DelaySecond", ref delayDurationValue, 0.1f, 0.1f, 3600f))
        {
            Config.DelayDuration = TimeSpan.FromSeconds(delayDurationValue);
            File.Save($"{Window.Name}{Window.Id}", Config);
        }
        ImGui.PopItemWidth();
        ImGui.SameLine();
        ImGui.Text("Seconds");

        if (alwaysValue) ImGui.EndDisabled();
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
            File.Save($"{Window.Name}{Window.Id}", Config);
            if (clickThroughValue) Window.Flags = OverlayWindow.defaultflags | ImGuiWindowFlags.NoInputs;
            else Window.Flags = OverlayWindow.defaultflags;
        }

        var frameCalcValue = Config.FrameCalc;
        if (ImGui.Checkbox("Calculate per frame", ref frameCalcValue))
        {
            Config.FrameCalc = frameCalcValue;
            File.Save($"{Window.Name}{Window.Id}", Config);
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
                File.Save($"{Window.Name}{Window.Id}", Config);
            }

            if (Config.Background)
            {
                var OverlayBackgroundColorValue = ImGui.ColorConvertU32ToFloat4(Config.BackgroundColor);
                if (ImGui.ColorEdit4("Background Color", ref OverlayBackgroundColorValue,
                    ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.OptionsDefault)) // ImGuiColorEditFlags.NoLabel
                {
                    Config.BackgroundColor = ImGui.ColorConvertFloat4ToU32(OverlayBackgroundColorValue);
                    File.Save($"{Window.Name}{Window.Id}", Config);
                }
            }
        }
        if (ImGui.CollapsingHeader("Header", ImGuiTreeNodeFlags.None))
        {
            var HeaderValue = Config.Header;
            if (ImGui.Checkbox("Header", ref HeaderValue))
            {
                Config.Header = HeaderValue;
                File.Save($"{Window.Name}{Window.Id}", Config);
            }
            if (Config.Header)
            {
                var HeaderAlignmentValue = (int)Config.HeaderAlignment;
                if (DrawAlingmentButtons(ref HeaderAlignmentValue))
                {
                    Config.HeaderAlignment = (Canvas.HorizontalAlign)HeaderAlignmentValue;
                    File.Save($"{Window.Name}{Window.Id}", Config);
                }

                var HeaderBackgroundValue = Config.HeaderBackground;
                if (ImGui.Checkbox("Header Background", ref HeaderBackgroundValue))
                {
                    Config.HeaderBackground = HeaderBackgroundValue;
                    File.Save($"{Window.Name}{Window.Id}", Config);
                }
                if (Config.HeaderBackground)
                {
                    var HeaderBackgroundColorValue = ImGui.ColorConvertU32ToFloat4(Config.HeaderBackgroundColor);
                    if (ImGui.ColorEdit4("Header Background Color", ref HeaderBackgroundColorValue,
                        ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.OptionsDefault)) // ImGuiColorEditFlags.NoLabel
                    {
                        Config.HeaderBackgroundColor = ImGui.ColorConvertFloat4ToU32(HeaderBackgroundColorValue);
                        File.Save($"{Window.Name}{Window.Id}", Config);
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
                File.Save($"{Window.Name}{Window.Id}", Config);
            }

            var BarValue = Config.Bar;
            if (ImGui.Checkbox("Bar", ref BarValue))
            {
                Config.Bar = BarValue;
                File.Save($"{Window.Name}{Window.Id}", Config);
            }
            if (Config.Bar)
            {
                var BarColorJobValue = Config.BarColorJob;
                if (ImGui.Checkbox("job colors", ref BarColorJobValue))
                {
                    Config.BarColorJob = BarColorJobValue;
                    File.Save($"{Window.Name}{Window.Id}", Config);
                }
                if (!Config.BarColorJob)
                {
                    var BarColorValue = ImGui.ColorConvertU32ToFloat4(Config.BarColor);
                    if (ImGui.ColorEdit4("Color", ref BarColorValue,
                        ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.OptionsDefault)) // ImGuiColorEditFlags.NoLabel
                    {
                        Config.BarColor = ImGui.ColorConvertFloat4ToU32(BarColorValue);
                        File.Save($"{Window.Name}{Window.Id}", Config);
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
                SelectedFont = Config.MoguFontSpec != null ? (SingleFontSpec)Config.MoguFontSpec : new SingleFontSpec { FontId = DalamudDefaultFontAndFamilyId.Instance },
            };
            chooser.ResultTask.ContinueWith(chooserTask =>
            {
                _ = chooserTask.Exception; // not needed ?
                if (chooserTask.IsCompletedSuccessfully)
                {
                    FontMogu?.Dispose();
                    FontMogu = chooserTask.Result.CreateFontHandle(FontAtlas);

                    Config.MoguFontSpec = chooser.SelectedFont;
                    File.Save($"{Window.Name}{Window.Id}", Config);
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
            File.Save($"{Window.Name}{Window.Id}", Config);
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
                File.Save($"{Window.Name}{Window.Id}", Config);
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
            File.Save($"{Window.Name}{Window.Id}", Config);
        }
    }

    private void DrawAboutTab()
    {
        using var tab = ImRaii.TabItem("About");
        if (!tab) return;
        
        DrawAbout();
    }

    private void DrawAbout()
    {
        ImGui.Text($"Description");
        ImGui.TextWrapped($"{Description}");
        ImGui.Text($"Autor");
        ImGui.TextWrapped($"{Autor}");
    }

}