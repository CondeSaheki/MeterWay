/*
    this need a complete refactor
*/

using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;

namespace MeterWay.Utils;

public class PlaceHoldersBuilder
{
    private class PlaceholderNode
    {
        public string Name { get; }
        public string FullPath { get; }
        public Dictionary<string, PlaceholderNode> Children { get; } = [];

        public PlaceholderNode(string name, string fullPath)
        {
            Name = name;
            FullPath = fullPath;
        }
    }

    private readonly string[] PlaceHolderStrings =
    [
        "Encounter.Name",
        "Encounter.Begin",
        "Encounter.End",
        "Encounter.Duration",
        "Encounter.DamageDealt",
        "Encounter.DamageDealt.Value",
        "Encounter.DamageDealt.Value.Total",
        "Encounter.DamageDealt.Value.Critical",
        "Encounter.DamageDealt.Value.Direct",
        "Encounter.DamageDealt.Value.CriticalDirect",
        "Encounter.DamageDealt.Value.Percent",
        "Encounter.DamageDealt.Value.Percent.Neutral",
        "Encounter.DamageDealt.Value.Percent.Critical",
        "Encounter.DamageDealt.Value.Percent.Direct",
        "Encounter.DamageDealt.Value.Percent.CriticalDirect",
        "Encounter.DamageDealt.Count",
        "Encounter.DamageDealt.Count.Total",
        "Encounter.DamageDealt.Count.Critical",
        "Encounter.DamageDealt.Count.Direct",
        "Encounter.DamageDealt.Count.CriticalDirect",
        "Encounter.DamageDealt.Count.Miss",
        "Encounter.DamageDealt.Count.Parry",
        "Encounter.DamageDealt.Count.Percent",
        "Encounter.DamageDealt.Count.Percent.Neutral",
        "Encounter.DamageDealt.Count.Percent.Critical",
        "Encounter.DamageDealt.Count.Percent.Direct",
        "Encounter.DamageDealt.Count.Percent.CriticalDirect",
        "Encounter.DamageDealt.Count.Percent.Miss",
        "Encounter.DamageDealt.Count.Percent.Parry",
        "Encounter.DamageReceived",
        "Encounter.DamageReceived.Value",
        "Encounter.DamageReceived.Value.Total",
        "Encounter.DamageReceived.Value.Critical",
        "Encounter.DamageReceived.Value.Direct",
        "Encounter.DamageReceived.Value.CriticalDirect",
        "Encounter.DamageReceived.Value.Percent",
        "Encounter.DamageReceived.Value.Percent.Neutral",
        "Encounter.DamageReceived.Value.Percent.Critical",
        "Encounter.DamageReceived.Value.Percent.Direct",
        "Encounter.DamageReceived.Value.Percent.CriticalDirect",
        "Encounter.DamageReceived.Count",
        "Encounter.DamageReceived.Count.Total",
        "Encounter.DamageReceived.Count.Critical",
        "Encounter.DamageReceived.Count.Direct",
        "Encounter.DamageReceived.Count.CriticalDirect",
        "Encounter.DamageReceived.Count.Miss",
        "Encounter.DamageReceived.Count.Parry",
        "Encounter.DamageReceived.Count.Percent",
        "Encounter.DamageReceived.Count.Percent.Neutral",
        "Encounter.DamageReceived.Count.Percent.Critical",
        "Encounter.DamageReceived.Count.Percent.Direct",
        "Encounter.DamageReceived.Count.Percent.CriticalDirect",
        "Encounter.DamageReceived.Count.Percent.Miss",
        "Encounter.DamageReceived.Count.Percent.Parry",
        "Encounter.HealDealt",
        "Encounter.HealDealt.Value",
        "Encounter.HealDealt.Value.Total",
        "Encounter.HealDealt.Value.Critical",
        "Encounter.HealDealt.Value.Percent",
        "Encounter.HealDealt.Value.Percent.Neutral",
        "Encounter.HealDealt.Value.Percent.Critical",
        "Encounter.HealDealt.Count",
        "Encounter.HealDealt.Count.Total",
        "Encounter.HealDealt.Count.Critical",
        "Encounter.HealDealt.Count.Percent",
        "Encounter.HealDealt.Count.Percent.Neutral",
        "Encounter.HealDealt.Count.Percent.Critical",
        "Encounter.HealReceived",
        "Encounter.HealReceived.Value",
        "Encounter.HealReceived.Value.Total",
        "Encounter.HealReceived.Value.Critical",
        "Encounter.HealReceived.Value.Percent",
        "Encounter.HealReceived.Value.Percent.Neutral",
        "Encounter.HealReceived.Value.Percent.Critical",
        "Encounter.HealReceived.Count",
        "Encounter.HealReceived.Count.Total",
        "Encounter.HealReceived.Count.Critical",
        "Encounter.HealReceived.Count.Percent",
        "Encounter.HealReceived.Count.Percent.Neutral",
        "Encounter.HealReceived.Count.Percent.Critical",
        "Encounter.PerSeconds.DamageDealt",
        "Encounter.PerSeconds.DamageReceived",
        "Encounter.PerSeconds.HealDealt",
        "Encounter.PerSeconds.HealReceived",
        "Player.Name",
        "Player.World",
        "Player.Job",
        "Player.DamageDealt",
        "Player.DamageDealt.Value",
        "Player.DamageDealt.Value.Total",
        "Player.DamageDealt.Value.Critical",
        "Player.DamageDealt.Value.Direct",
        "Player.DamageDealt.Value.CriticalDirect",
        "Player.DamageDealt.Value.Percent",
        "Player.DamageDealt.Value.Percent.Neutral",
        "Player.DamageDealt.Value.Percent.Critical",
        "Player.DamageDealt.Value.Percent.Direct",
        "Player.DamageDealt.Value.Percent.CriticalDirect",
        "Player.DamageDealt.Count",
        "Player.DamageDealt.Count.Total",
        "Player.DamageDealt.Count.Critical",
        "Player.DamageDealt.Count.Direct",
        "Player.DamageDealt.Count.CriticalDirect",
        "Player.DamageDealt.Count.Miss",
        "Player.DamageDealt.Count.Parry",
        "Player.DamageDealt.Count.Percent",
        "Player.DamageDealt.Count.Percent.Neutral",
        "Player.DamageDealt.Count.Percent.Critical",
        "Player.DamageDealt.Count.Percent.Direct",
        "Player.DamageDealt.Count.Percent.CriticalDirect",
        "Player.DamageDealt.Count.Percent.Miss",
        "Player.DamageDealt.Count.Percent.Parry",
        "Player.DamageReceived",
        "Player.DamageReceived.Value",
        "Player.DamageReceived.Value.Total",
        "Player.DamageReceived.Value.Critical",
        "Player.DamageReceived.Value.Direct",
        "Player.DamageReceived.Value.CriticalDirect",
        "Player.DamageReceived.Value.Percent",
        "Player.DamageReceived.Value.Percent.Neutral",
        "Player.DamageReceived.Value.Percent.Critical",
        "Player.DamageReceived.Value.Percent.Direct",
        "Player.DamageReceived.Value.Percent.CriticalDirect",
        "Player.DamageReceived.Count",
        "Player.DamageReceived.Count.Total",
        "Player.DamageReceived.Count.Critical",
        "Player.DamageReceived.Count.Direct",
        "Player.DamageReceived.Count.CriticalDirect",
        "Player.DamageReceived.Count.Miss",
        "Player.DamageReceived.Count.Parry",
        "Player.DamageReceived.Count.Percent",
        "Player.DamageReceived.Count.Percent.Neutral",
        "Player.DamageReceived.Count.Percent.Critical",
        "Player.DamageReceived.Count.Percent.Direct",
        "Player.DamageReceived.Count.Percent.CriticalDirect",
        "Player.DamageReceived.Count.Percent.Miss",
        "Player.DamageReceived.Count.Percent.Parry",
        "Player.HealDealt",
        "Player.HealDealt.Value",
        "Player.HealDealt.Value.Total",
        "Player.HealDealt.Value.Critical",
        "Player.HealDealt.Value.Percent",
        "Player.HealDealt.Value.Percent.Neutral",
        "Player.HealDealt.Value.Percent.Critical",
        "Player.HealDealt.Count",
        "Player.HealDealt.Count.Total",
        "Player.HealDealt.Count.Critical",
        "Player.HealDealt.Count.Percent",
        "Player.HealDealt.Count.Percent.Neutral",
        "Player.HealDealt.Count.Percent.Critical",
        "Player.HealReceived",
        "Player.HealReceived.Value",
        "Player.HealReceived.Value.Total",
        "Player.HealReceived.Value.Critical",
        "Player.HealReceived.Value.Percent",
        "Player.HealReceived.Value.Percent.Neutral",
        "Player.HealReceived.Value.Percent.Critical",
        "Player.HealReceived.Count",
        "Player.HealReceived.Count.Total",
        "Player.HealReceived.Count.Critical",
        "Player.HealReceived.Count.Percent",
        "Player.HealReceived.Count.Percent.Neutral",
        "Player.HealReceived.Count.Percent.Critical",
        "Player.PerSeconds.DamageDealt",
        "Player.PerSeconds.DamageReceived",
        "Player.PerSeconds.HealDealt",
        "Player.PerSeconds.HealReceived",
    ];

    private PlaceholderNode RootNode { get; }

    // cursed
    private static string? SelectedPlaceHolder { get; set; } = null;

    // cursed
    private static List<PlaceholderNode> selectedNodes = [];

    public PlaceHoldersBuilder()
    {
        RootNode = new PlaceholderNode("Root", "");
        foreach (var placeholder in PlaceHolderStrings)
        {
            AddPlaceholder(RootNode, placeholder.Split('.'));
        }
    }

    public void Draw()
    {
        DrawNode(RootNode);
    }

    public string GetResult()
    {
        return SelectedPlaceHolder ?? "";
    }

    public void Clear()
    {
        SelectedPlaceHolder = string.Empty;
        selectedNodes.Clear();
    }

    private void AddPlaceholder(PlaceholderNode node, string[] parts, int index = 0)
    {
        if (index >= parts.Length) return;

        if (!node.Children.TryGetValue(parts[index], out PlaceholderNode? value))
        {
            string fullPath = string.Join('.', parts, 0, index + 1);
            value = new PlaceholderNode(parts[index], fullPath);
            node.Children[parts[index]] = value;
        }

        AddPlaceholder(value, parts, index + 1);
    }

    private void DrawNode(PlaceholderNode currentNode, int level = 0)
    {
        if (currentNode.Children.Count == 0) return;

        if (level >= 1)
        {
            ImGui.SameLine();
            ImGui.Text($" -> ");
            ImGui.SameLine();
        }

        int selectedIndex = selectedNodes.Count > level ? GetSelectedIndex(currentNode, level) : -1;

        ImGui.PushItemWidth(150);
        if (ImGui.BeginCombo($"##{level}", selectedIndex != -1 ? currentNode.Children.Keys.ToArray()[selectedIndex] : "Select"))
        {
            foreach (var pair in currentNode.Children)
            {
                bool isSelected = pair.Key == (selectedIndex != -1 ? currentNode.Children.Keys.ToArray()[selectedIndex] : "");
                if (ImGui.Selectable(pair.Key, isSelected))
                {
                    selectedIndex = Array.IndexOf([.. currentNode.Children.Keys], pair.Key);
                    if (selectedNodes.Count > level)
                    {
                        selectedNodes[level] = pair.Value;
                        selectedNodes.RemoveRange(level + 1, selectedNodes.Count - level - 1);
                    }
                    else
                    {
                        selectedNodes.Add(pair.Value);
                    }

                    SelectedPlaceHolder = pair.Value.FullPath;
                }
            }

            ImGui.EndCombo();
        }
        ImGui.PopItemWidth();

        if (selectedNodes.Count > level)
        {
            DrawNode(selectedNodes[level], level + 1);
        }
    }

    private int GetSelectedIndex(PlaceholderNode currentNode, int level)
    {
        int index = -1;
        int i = 0;
        foreach (var pair in currentNode.Children)
        {
            if (pair.Key == selectedNodes[level].Name)
            {
                index = i;
                break;
            }
            i++;
        }
        return index;
    }
}

/*
private readonly string[] PlaceHolders = [
        "{Encounter.Name}",
        "{Encounter.Begin}",
        "{Encounter.End}",
        "{Encounter.Duration}",
        "{Encounter.DamageDealt}",
        "{Encounter.DamageDealt.Value}",
        "{Encounter.DamageDealt.Value.Total}",
        "{Encounter.DamageDealt.Value.Critical}",
        "{Encounter.DamageDealt.Value.Direct}",
        "{Encounter.DamageDealt.Value.CriticalDirect}",
        "{Encounter.DamageDealt.Value.Percent}",
        "{Encounter.DamageDealt.Value.Percent.Neutral}",
        "{Encounter.DamageDealt.Value.Percent.Critical}",
        "{Encounter.DamageDealt.Value.Percent.Direct}",
        "{Encounter.DamageDealt.Value.Percent.CriticalDirect}",
        "{Encounter.DamageDealt.Count}",
        "{Encounter.DamageDealt.Count.Total}",
        "{Encounter.DamageDealt.Count.Critical}",
        "{Encounter.DamageDealt.Count.Direct}",
        "{Encounter.DamageDealt.Count.CriticalDirect}",
        "{Encounter.DamageDealt.Count.Miss}",
        "{Encounter.DamageDealt.Count.Parry}",
        "{Encounter.DamageDealt.Count.Percent}",
        "{Encounter.DamageDealt.Count.Percent.Neutral}",
        "{Encounter.DamageDealt.Count.Percent.Critical}",
        "{Encounter.DamageDealt.Count.Percent.Direct}",
        "{Encounter.DamageDealt.Count.Percent.CriticalDirect}",
        "{Encounter.DamageDealt.Count.Percent.Miss}",
        "{Encounter.DamageDealt.Count.Percent.Parry}",
        "{Encounter.DamageReceived}",
        "{Encounter.DamageReceived.Value}",
        "{Encounter.DamageReceived.Value.Total}",
        "{Encounter.DamageReceived.Value.Critical}",
        "{Encounter.DamageReceived.Value.Direct}",
        "{Encounter.DamageReceived.Value.CriticalDirect}",
        "{Encounter.DamageReceived.Value.Percent}",
        "{Encounter.DamageReceived.Value.Percent.Neutral}",
        "{Encounter.DamageReceived.Value.Percent.Critical}",
        "{Encounter.DamageReceived.Value.Percent.Direct}",
        "{Encounter.DamageReceived.Value.Percent.CriticalDirect}",
        "{Encounter.DamageReceived.Count}",
        "{Encounter.DamageReceived.Count.Total}",
        "{Encounter.DamageReceived.Count.Critical}",
        "{Encounter.DamageReceived.Count.Direct}",
        "{Encounter.DamageReceived.Count.CriticalDirect}",
        "{Encounter.DamageReceived.Count.Miss}",
        "{Encounter.DamageReceived.Count.Parry}",
        "{Encounter.DamageReceived.Count.Percent}",
        "{Encounter.DamageReceived.Count.Percent.Neutral}",
        "{Encounter.DamageReceived.Count.Percent.Critical}",
        "{Encounter.DamageReceived.Count.Percent.Direct}",
        "{Encounter.DamageReceived.Count.Percent.CriticalDirect}",
        "{Encounter.DamageReceived.Count.Percent.Miss}",
        "{Encounter.DamageReceived.Count.Percent.Parry}",
        "{Encounter.HealDealt}",
        "{Encounter.HealDealt.Value}",
        "{Encounter.HealDealt.Value.Total}",
        "{Encounter.HealDealt.Value.Critical}",
        "{Encounter.HealDealt.Value.Percent}",
        "{Encounter.HealDealt.Value.Percent.Neutral}",
        "{Encounter.HealDealt.Value.Percent.Critical}",
        "{Encounter.HealDealt.Count}",
        "{Encounter.HealDealt.Count.Total}",
        "{Encounter.HealDealt.Count.Critical}",
        "{Encounter.HealDealt.Count.Percent}",
        "{Encounter.HealDealt.Count.Percent.Neutral}",
        "{Encounter.HealDealt.Count.Percent.Critical}",
        "{Encounter.HealReceived}",
        "{Encounter.HealReceived.Value}",
        "{Encounter.HealReceived.Value.Total}",
        "{Encounter.HealReceived.Value.Critical}",
        "{Encounter.HealReceived.Value.Percent}",
        "{Encounter.HealReceived.Value.Percent.Neutral}",
        "{Encounter.HealReceived.Value.Percent.Critical}",
        "{Encounter.HealReceived.Count}",
        "{Encounter.HealReceived.Count.Total}",
        "{Encounter.HealReceived.Count.Critical}",
        "{Encounter.HealReceived.Count.Percent}",
        "{Encounter.HealReceived.Count.Percent.Neutral}",
        "{Encounter.HealReceived.Count.Percent.Critical}",
        "{Player.Name}",
        "{Player.World}",
        "{Player.Job}",
        "{Player.DamageDealt}",
        "{Player.DamageDealt.Value}",
        "{Player.DamageDealt.Value.Total}",
        "{Player.DamageDealt.Value.Critical}",
        "{Player.DamageDealt.Value.Direct}",
        "{Player.DamageDealt.Value.CriticalDirect}",
        "{Player.DamageDealt.Value.Percent}",
        "{Player.DamageDealt.Value.Percent.Neutral}",
        "{Player.DamageDealt.Value.Percent.Critical}",
        "{Player.DamageDealt.Value.Percent.Direct}",
        "{Player.DamageDealt.Value.Percent.CriticalDirect}",
        "{Player.DamageDealt.Count}",
        "{Player.DamageDealt.Count.Total}",
        "{Player.DamageDealt.Count.Critical}",
        "{Player.DamageDealt.Count.Direct}",
        "{Player.DamageDealt.Count.CriticalDirect}",
        "{Player.DamageDealt.Count.Miss}",
        "{Player.DamageDealt.Count.Parry}",
        "{Player.DamageDealt.Count.Percent}",
        "{Player.DamageDealt.Count.Percent.Neutral}",
        "{Player.DamageDealt.Count.Percent.Critical}",
        "{Player.DamageDealt.Count.Percent.Direct}",
        "{Player.DamageDealt.Count.Percent.CriticalDirect}",
        "{Player.DamageDealt.Count.Percent.Miss}",
        "{Player.DamageDealt.Count.Percent.Parry}",
        "{Player.DamageReceived}",
        "{Player.DamageReceived.Value}",
        "{Player.DamageReceived.Value.Total}",
        "{Player.DamageReceived.Value.Critical}",
        "{Player.DamageReceived.Value.Direct}",
        "{Player.DamageReceived.Value.CriticalDirect}",
        "{Player.DamageReceived.Value.Percent}",
        "{Player.DamageReceived.Value.Percent.Neutral}",
        "{Player.DamageReceived.Value.Percent.Critical}",
        "{Player.DamageReceived.Value.Percent.Direct}",
        "{Player.DamageReceived.Value.Percent.CriticalDirect}",
        "{Player.DamageReceived.Count}",
        "{Player.DamageReceived.Count.Total}",
        "{Player.DamageReceived.Count.Critical}",
        "{Player.DamageReceived.Count.Direct}",
        "{Player.DamageReceived.Count.CriticalDirect}",
        "{Player.DamageReceived.Count.Miss}",
        "{Player.DamageReceived.Count.Parry}",
        "{Player.DamageReceived.Count.Percent}",
        "{Player.DamageReceived.Count.Percent.Neutral}",
        "{Player.DamageReceived.Count.Percent.Critical}",
        "{Player.DamageReceived.Count.Percent.Direct}",
        "{Player.DamageReceived.Count.Percent.CriticalDirect}",
        "{Player.DamageReceived.Count.Percent.Miss}",
        "{Player.DamageReceived.Count.Percent.Parry}",
        "{Player.HealDealt}",
        "{Player.HealDealt.Value}",
        "{Player.HealDealt.Value.Total}",
        "{Player.HealDealt.Value.Critical}",
        "{Player.HealDealt.Value.Percent}",
        "{Player.HealDealt.Value.Percent.Neutral}",
        "{Player.HealDealt.Value.Percent.Critical}",
        "{Player.HealDealt.Count}",
        "{Player.HealDealt.Count.Total}",
        "{Player.HealDealt.Count.Critical}",
        "{Player.HealDealt.Count.Percent}",
        "{Player.HealDealt.Count.Percent.Neutral}",
        "{Player.HealDealt.Count.Percent.Critical}",
        "{Player.HealReceived}",
        "{Player.HealReceived.Value}",
        "{Player.HealReceived.Value.Total}",
        "{Player.HealReceived.Value.Critical}",
        "{Player.HealReceived.Value.Percent}",
        "{Player.HealReceived.Value.Percent.Neutral}",
        "{Player.HealReceived.Value.Percent.Critical}",
        "{Player.HealReceived.Count}",
        "{Player.HealReceived.Count.Total}",
        "{Player.HealReceived.Count.Critical}",
        "{Player.HealReceived.Count.Percent}",
        "{Player.HealReceived.Count.Percent.Neutral}",
        "{Player.HealReceived.Count.Percent.Critical}",
        "{Encounter.PerSeconds.DamageDealt}",
        "{Encounter.PerSeconds.DamageReceived}",
        "{Encounter.PerSeconds.HealDealt}",
        "{Encounter.PerSeconds.HealReceived}",
        "{Player.PerSeconds.DamageDealt}",
        "{Player.PerSeconds.DamageReceived}",
        "{Player.PerSeconds.HealDealt}",
        "{Player.PerSeconds.HealReceived}",
    ];
*/