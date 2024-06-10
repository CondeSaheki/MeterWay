using System;
using MeterWay.Data;

namespace MeterWay.Utils;

public class VariableFunction(Type? argumentType = null, Type? returnType = null, Func<object, object?>? function = null)
{
    public Type? Argument { get; private set; } = argumentType;
    public Type? Return { get; private set; } = returnType;
    public Func<object, object?>? Function { get; private set; } = function;
}

public class Node
{
    public string Name { get; set; }
    public string[] FullName { get; set; }
    public Node[] Children { get; set; }
    public VariableFunction Getter { get; set; }

    public Node(string name, string[] fullName, VariableFunction getter, Node[]? children = null)
    {
        Name = name;
        FullName = [.. fullName, name];
        Children = children ?? [];
        Getter = getter;
    }

    public Node(Node node) : this(node.Name, node.FullName, node.Getter, node.Children) { }
}

public static class PlaceHolder
{
    /// <summary>
    /// Get a node in the tree based on the given full name.
    /// </summary>
    /// <param name="fullName">The full name of the node to search for.</param>
    /// <param name="node">The root node to start the search from. If null, the root node of the tree will be used.</param>
    /// <returns>the node if the node is found, null otherwise.</returns>
    public static Node? Get(string[] fullName, Node? node = null)
    {
        if (fullName.Length == 0 || node == null) return null;

        Node currentNode = node ?? All;

        foreach (var name in fullName)
        {
            if (currentNode.Children == null) return null;
            var index = Array.FindIndex(currentNode.Children, node => name == node.Name);
            if (index == -1) return null;
            currentNode = currentNode.Children[index];
        }
        return currentNode;
    }
    
    /// <summary>
    /// Get a node in the tree based on the given full name.
    /// </summary>
    /// <param name="fullNameString">The full name of the node to search for.</param>
    /// <param name="node">The root node to start the search from. If null, the root node of the tree will be used.</param>
    /// <returns>the node if the node is found, null otherwise.</returns>
    public static Node? Get(string fullNameString, Node? node = null) => Get(fullNameString.Trim('{').TrimEnd('}').Split('.'), node);

    /// <summary>
    /// The root node of the placeholder tree.
    /// </summary>
    public static Node All => new("Root", [], new(null, null, null), [Encounter, Party, Player, Pet]);

    /// <summary>
    /// The encounter node of the placeholder tree.
    /// </summary>
    public static Node Encounter => new("Encounter", [], new(typeof(IEncounter), typeof(int), (obj) => (IEncounter)obj),
    [
        new("Name", ["Encounter"], new(typeof(IEncounter), typeof(int), (obj) => ((IEncounter)obj).Name)),
        new("Begin", ["Encounter"], new(typeof(IEncounter), typeof(int), (obj) => ((IEncounter)obj).Begin)),
        new("End", ["Encounter"], new(typeof(IEncounter), typeof(int), (obj) => ((IEncounter)obj).End)),
        new("Duration", ["Encounter"], new(typeof(IEncounter), typeof(int), (obj) => ((IEncounter)obj).Duration)),
        new("Partys", ["Encounter"], new(typeof(IEncounter), typeof(int), (obj) => ((IEncounter)obj).Partys)),
        ..IDataNodes("Encounter"),
    ]);

    /// <summary>
    /// The party node of the placeholder tree.
    /// </summary>
    public static Node Party => new("Party", [], new(typeof(IParty), typeof(int), (obj) => (IParty)obj),
    [
        new ("Players", ["Party"], new(typeof(IParty), typeof(int), (obj) => ((IParty)obj).Players)),
        new ("Pets", ["Party"], new(typeof(IParty), typeof(int), (obj) => ((IParty)obj).Pets)),
        ..IDataNodes("Party"),
    ]);

    /// <summary>
    /// The player node of the placeholder tree.
    /// </summary>
    public static Node Player => new("Player", [], new(typeof(IPlayer), typeof(int), (obj) => (IPlayer)obj),
    [
        new ("Name", ["Player"], new(typeof(IPlayer), typeof(int),  (obj) => ((IPlayer)obj).Name)),
        new ("World", ["Player"], new(typeof(IPlayer), typeof(int), (obj) => ((IPlayer)obj).World)),
        new ("Job", ["Player"], new(typeof(IPlayer), typeof(int), (obj) => ((IPlayer)obj).Job)),
        ..IDataNodes("Player"),
    ]);

    /// <summary>
    /// The pet node of the placeholder tree.
    /// </summary>
    public static Node Pet => new("Pet", [], new(typeof(IPet), typeof(int), (obj) => (IPet)obj),
    [
        new ("Name", ["Pet"], new(typeof(IPet), typeof(int), (obj) => ((IPet)obj).Name)),
        new ("Owner", ["Pet"], new(typeof(IPet), typeof(int), (obj) => ((IPet)obj).Owner)),
        ..IDataNodes("Pet"),
    ]);

    private static Node[] IDataNodes(string owner) =>
    [
        new("DamageDealt", [owner], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageDealt),
        [
            new("Value", [owner, "DamageDealt"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageDealt.Value),
            [
                new("Total", [owner, "DamageDealt", "Value"],  new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageDealt.Value.Total)),
                new("Critical", [owner, "DamageDealt", "Value"],  new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageDealt.Value.Critical)),
                new("Direct", [owner, "DamageDealt", "Value"],  new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageDealt.Value.Direct)),
                new("CriticalDirect", [owner, "DamageDealt", "Value"],  new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageDealt.Value.CriticalDirect)),
                new("Percent", [owner, "DamageDealt", "Value"],  new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageDealt.Value.Percent),
                [
                    new("Neutral", [owner, "DamageDealt", "Value", "Percent"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageDealt.Value.Percent.Neutral)),
                    new("Critical", [owner, "DamageDealt", "Value", "Percent"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageDealt.Value.Percent.Critical)),
                    new("Direct", [owner, "DamageDealt", "Value", "Percent"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageDealt.Value.Percent.Direct)),
                    new("CriticalDirect", [owner, "DamageDealt", "Value", "Percent"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageDealt.Value.Percent.CriticalDirect))
                ])
            ]),
            new("Count", [owner, "DamageDealt"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageDealt.Count),
            [
                new("Total", [owner, "DamageDealt", "Count"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageDealt.Count.Total)),
                new("Critical", [owner, "DamageDealt", " Count"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageDealt.Count.Critical)),
                new("Direct", [owner, "DamageDealt", "   Count"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageDealt.Count.Direct)),
                new("CriticalDirect", [owner, "DamageDealt", "   Count"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageDealt.Count.CriticalDirect)),
                new("Miss", [owner, "DamageDealt", " Count"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageDealt.Count.Miss)),
                new("Parry", [owner, "DamageDealt", "Count"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageDealt.Count.Parry)),
                new("Percent", [owner, "DamageDealt", "Count"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageDealt.Count.Percent),
                [
                    new("Neutral", [owner, "DamageDealt", "Count", "Percent"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageDealt.Count.Percent.Neutral)),
                    new("Critical", [owner, "DamageDealt", "Count", "Percent"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageDealt.Count.Percent.Critical)),
                    new("Direct", [owner, "DamageDealt", "Count", "Percent"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageDealt.Count.Percent.Direct)),
                    new("CriticalDirect", [owner, "DamageDealt", "Count", "Percent"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageDealt.Count.Percent.CriticalDirect)),
                    new("Miss", [owner, "DamageDealt", "Count", "Percent"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageDealt.Count.Percent.Miss)),
                    new("Parry", [owner, "DamageDealt", "Count", "Percent"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageDealt.Count.Percent.Parry))
                ])
            ])
        ]),
        new ("DamageReceived", [owner], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageReceived),
        [
            new ("Value", [owner, "DamageReceived"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageReceived.Value),
            [
                new ("Total", [owner, "DamageReceived", "Value"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageReceived.Value.Total)),
                new ("Critical", [owner, "DamageReceived", "Value"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageReceived.Value.Critical)),
                new ("Direct", [owner, "DamageReceived", "Value"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageReceived.Value.Direct)),
                new ("CriticalDirect", [owner, "DamageReceived", "Value"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageReceived.Value.CriticalDirect)),
                new ("Percent", [owner, "DamageReceived", "Value"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageReceived.Value.Percent),
                [
                    new ("Neutral", [owner, "DamageReceived", "Value", "Percent"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageReceived.Value.Percent.Neutral)),
                    new ("Critical", [owner, "DamageReceived", "Value", "Percent"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageReceived.Value.Percent.Critical)),
                    new ("Direct", [owner, "DamageReceived", "Value", "Percent"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageReceived.Value.Percent.Direct)),
                    new ("CriticalDirect", [owner, "DamageReceived", "Value", "Percent"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageReceived.Value.Percent.CriticalDirect))
                ])
            ]),
            new ("Count", [owner, "DamageReceived"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageReceived.Count),
            [
                new ("Total", [owner, "DamageReceived", "Count"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageReceived.Count.Total)),
                new ("Critical", [owner, "DamageReceived", "Count"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageReceived.Count.Critical)),
                new ("Direct", [owner, "DamageReceived", "Count"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageReceived.Count.Direct)),
                new ("CriticalDirect", [owner, "DamageReceived", "Count"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageReceived.Count.CriticalDirect)),
                new ("Miss", [owner, "DamageReceived", "Count"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageReceived.Count.Miss)),
                new ("Parry", [owner, "DamageReceived", "Count"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageReceived.Count.Parry)),
                new ("Percent", [owner, "DamageReceived", "Count"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageReceived.Count.Percent),
                [
                    new ("Neutral", [owner, "DamageReceived", "Count", "Percent"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageReceived.Count.Percent.Neutral)),
                    new ("Critical", [owner, "DamageReceived", "Count", "Percent"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageReceived.Count.Percent.Critical)),
                    new ("Direct", [owner, "DamageReceived", "Count", "Percent"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageReceived.Count.Percent.Direct)),
                    new ("CriticalDirect", [owner, "DamageReceived", "Count", "Percent"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageReceived.Count.Percent.CriticalDirect)),
                    new ("Miss", [owner, "DamageReceived", "Count", "Percent"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageReceived.Count.Percent.Miss)),
                    new ("Parry", [owner, "DamageReceived", "Count", "Percent"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).DamageReceived.Count.Percent.Parry))
                ])
            ])
        ]),
        new ("HealDealt", [owner], new(typeof(IData), typeof(int), (obj) => ((IData)obj).HealDealt),
        [
            new ("Value", [owner, "HealDealt"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).HealDealt.Value),
            [
                new ("Total", [owner, "HealDealt", "Value"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).HealDealt.Value.Total)),
                new ("Critical", [owner, "HealDealt", "Value"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).HealDealt.Value.Critical)),
                new ("Percent", [owner, "HealDealt", "Value"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).HealDealt.Value.Percent),
                [
                    new ("Neutral", [owner, "HealDealt", "Value", "Percent"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).HealDealt.Value.Percent.Neutral)),
                    new ("Critical", [owner, "HealDealt", "Value", "Percent"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).HealDealt.Value.Percent.Critical))
                ])
            ]),
            new ("Count", [owner, "HealDealt"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).HealDealt.Count),
            [
                new ("Total", [owner, "HealDealt", "Count"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).HealDealt.Count.Total)),
                new ("Critical", [owner, "HealDealt", "Count"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).HealDealt.Count.Critical)),
                new ("Percent", [owner, "HealDealt", "Count"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).HealDealt.Count.Percent),
                [
                    new ("Neutral", [owner, "HealDealt", "Count", "Percent"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).HealDealt.Count.Percent.Neutral)),
                    new ("Critical", [owner, "HealDealt", "Count", "Percent"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).HealDealt.Count.Percent.Critical))
                ])
            ])
        ]),
        new ("HealReceived", [owner], new(typeof(IData), typeof(int), (obj) => ((IData)obj).HealReceived),
        [
            new ("Value", [owner, "HealReceived"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).HealReceived.Value),
            [
                new ("Total", [owner, "HealReceived", "Value"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).HealReceived.Value.Total)),
                new ("Critical", [owner, "HealReceived", "Value"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).HealReceived.Value.Critical)),
                new ("Percent", [owner, "HealReceived", "Value"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).HealReceived.Value.Percent),
                [
                    new ("Neutral", [owner, "HealReceived", "Value", "Percent"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).HealReceived.Value.Percent.Neutral)),
                    new ("Critical", [owner, "HealReceived", "Value", "Percent"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).HealReceived.Value.Percent.Critical))
                ])
            ]),
            new ("Count", [owner, "HealReceived"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).HealReceived.Count),
            [
                new ("Total", [owner, "HealReceived", "Count"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).HealReceived.Count.Total)),
                new ("Critical", [owner, "HealReceived", "Count"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).HealReceived.Count.Critical)),
                new ("Percent", [owner, "HealReceived", "Count"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).HealReceived.Count.Percent),
                [
                    new ("Neutral", [owner, "HealReceived", "Count", "Percent"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).HealReceived.Count.Percent.Neutral)),
                    new ("Critical", [owner, "HealReceived", "Count", "Percent"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).HealReceived.Count.Percent.Critical))
                ])
            ])
        ]),
        new ("PerSeconds", [owner], new(typeof(IData), typeof(int), (obj) => ((IData)obj).PerSeconds),
        [
            new ("DamageDealt", [owner, "PerSeconds"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).PerSeconds.DamageDealt)),
            new ("DamageReceived", [owner, "PerSeconds"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).PerSeconds.DamageReceived)),
            new ("HealDealt", [owner, "PerSeconds"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).PerSeconds.HealDealt)),
            new ("HealReceived", [owner, "PerSeconds"], new(typeof(IData), typeof(int), (obj) => ((IData)obj).PerSeconds.HealReceived))
        ])
    ];
}
