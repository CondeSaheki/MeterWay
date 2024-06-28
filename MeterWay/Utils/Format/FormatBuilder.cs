using System;
using System.Numerics;
using System.Threading.Tasks;
using ImGuiNET;

namespace MeterWay.Utils.Format;

public class FormatBuilderDialog
{
    private const string Label = "Format Builder Dialog";
    private readonly Vector2 Size = new(1280, 360);
    private const ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoCollapse;

    private TaskCompletionSource TaskSource { get; init; } = new();
    private bool FirstDraw { get; set; } = true;

    private Action<Format> Setter { get; init; }
    private string Original { get; init; }
    private Format Format { get; set; }

    public FormatBuilderDialog(Format currentFormat, Action<Format> setter)
    {
        Setter = setter;
        Original = currentFormat.Original;
        Format = currentFormat;

        Dalamud.PluginInterface.UiBuilder.Draw += Draw;
        TaskSource.Task.ContinueWith(t =>
        {
            _ = t.Exception;
            Dalamud.PluginInterface.UiBuilder.Draw -= Draw;
        });
    }

    private void Draw()
    {
        if (FirstDraw)
        {
            Vector2 center = ImGui.GetMainViewport().GetCenter();
            ImGui.SetNextWindowPos(center, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
            ImGui.SetNextWindowSize(Size, ImGuiCond.Always);
        }
        ImGui.SetNextWindowSizeConstraints(new Vector2(320f, 180), new Vector2(float.MaxValue));

        bool p_open2 = true;
        if (!ImGui.Begin(Label, ref p_open2, WindowFlags) || !p_open2)
        {
            ImGui.End();
            TaskSource.SetCanceled();
            return;
        }

        ImGui.Spacing();
        ImGui.Text("Enter your text below:");
        ImGui.Spacing();

        var currentStringValue = Format.Original;
        Vector2 boxSize = new(ImGui.GetContentRegionAvail().X - 16, Math.Min(ImGui.CalcTextSize(currentStringValue).Y + 24, 16 * 3 + 24));
        Vector2 textBoxSize = new(Math.Max(ImGui.CalcTextSize(currentStringValue).X, ImGui.GetContentRegionAvail().X - 16) + 16, ImGui.CalcTextSize(currentStringValue).Y + 24);
        ImGui.BeginChild("##text box", boxSize, false, ImGuiWindowFlags.HorizontalScrollbar);
        if (ImGui.InputTextMultiline("##BigTextInput", ref currentStringValue, 2048, textBoxSize, ImGuiInputTextFlags.NoHorizontalScroll)
             && currentStringValue != Format.Original)
        {
            // PlaceHolders.NodeSearch();

            // Modify FormatInfo add + remove + update + validate 
            // remove or add placeholders in current string value
            Format = new(currentStringValue);
        }
        ImGui.EndChild();

        ImGui.Spacing();

        if (ImGui.Button("Clear")) Format = new(string.Empty);
        ImGui.SameLine();
        if (ImGui.Button("Reset")) Format = new(Original);
        ImGui.SameLine();
        if (ImGui.Button("Save"))
        {
            Setter(Format);
            TaskSource.SetResult();
        }
        ImGui.SameLine();
        FormatHelpers.PlaceHolderChoser([], (newValue) =>
        {
            Format = new($"{Format.Original}{{{string.Join('.', newValue.FullName)}}}");
        }, $"Add");

        // ImGui.Spacing();
        // ImGui.Separator();
        // ImGui.Spacing();
        // ImGui.Text("Format Builder");
        // ImGui.Spacing();
        // for (int index = 0; index < Format.PlaceHolders.Length; ++index)
        // {
        //     var item = Format.PlaceHolders[index];

        //     var formatchoser = new FormatChoser($"Change Placeholder {index + 1}", item.FullName, (newValue) =>
        //     {
        //         Format.PlaceHolders[index].FullName = newValue.FullName;
        //     });
        //     formatchoser.Draw();

        //     //ImGui.Selectable(item);

        //     if (ImGui.Selectable($"Format {index + 1}", false, ImGuiSelectableFlags.AllowDoubleClick))
        //     {
        //         // Handle selection logic here if needed
        //     }

        //     if (ImGui.IsItemActive() && !ImGui.IsItemHovered())
        //     {
        //         int move = index + (ImGui.GetMouseDragDelta(0).Y < 0.0f ? -1 : 1);
        //         if (move >= 0 && move < Format.PlaceHolders.Length)
        //         {
        //             Format.PlaceHolders[index] = Format.PlaceHolders[move];
        //             Format.PlaceHolders[move] = item;
        //             ImGui.ResetMouseDragDelta();
        //         }
        //     }
        // }

        ImGui.End();
        FirstDraw = false;
    }
}