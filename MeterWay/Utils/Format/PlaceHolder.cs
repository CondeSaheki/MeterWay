using System;
using System.Linq;
using MeterWay.Data;

namespace MeterWay.Utils;

/// <summary>
/// Represents a node in a tree structure. Each node has a name, a list of children nodes,
/// a function delegate, and types for the argument and return value of the function.
/// </summary>
public class PlaceHolder
{
    public string[] FullName { get; private init; }
    public PlaceHolder[] Children { get; private init; }
    public Delegate Function { get; private init; }
    public Type? ArgumentType { get; private init; }
    public Type? ReturnType { get; private init; }

    public PlaceHolder(string[] fullName, Delegate function, PlaceHolder[]? children = null)
    {
        FullName = fullName;
        Children = children ?? [];
        Function = function;
        var parameters = function?.Method.GetParameters();
        ArgumentType = parameters != null && parameters.Length != 0 ? parameters[0].ParameterType : null;
        ReturnType = function?.Method.ReturnType;
    }

    public PlaceHolder(PlaceHolder[] children) : this([], new Action(() => { }), children) { }
    public PlaceHolder(string firstName, string[] familyName, Delegate function, PlaceHolder[]? children = null) : this([.. familyName, firstName], function, children) { }

    public string FirstName => FullName[^1];
    public string[] FamilyName => FullName[..^1];

    /// <summary>
    /// Invokes the function delegate with the given argument and returns the result.
    /// </summary>
    /// <typeparam name="TArg">The type of the argument.</typeparam>
    /// <typeparam name="TResult">The type of the return value.</typeparam>
    /// <param name="arg">The argument to pass to the function delegate.</param>
    /// <returns>The result of the function delegate invocation.</returns>
    public TResult Get<TArg, TResult>(TArg arg)
    {
        if (Function is not Func<TArg, TResult> func) throw new Exception("Node Get: Signature mismatch.");
        return func(arg);
    }

    /// <summary>
    /// Invokes the function delegate with the given argument and returns the result.
    /// </summary>
    /// <param name="arg">The argument to pass to the function delegate.</param>
    /// <returns>The result of the function delegate invocation.</returns>
    public object? Get(object arg)
    {
        var method = Function.Method;
        var parameters = new object[] { arg };
        return method.Invoke(Function.Target, parameters);
    }

    /// <summary>
    /// Find a node in the tree based on the given full name.
    /// </summary>
    /// <param name="fullName">The full name of the node to search for.</param>
    /// <param name="node">The root node to start the search from. If null, the root node of the tree will be used.</param>
    /// <returns>the node if the node is found, null otherwise.</returns>
    public static PlaceHolder? Find(string[] fullName, PlaceHolder? node = null)
    {
        if (fullName.Length == 0) return null;

        PlaceHolder currentNode = node ?? All;

        foreach (var name in fullName)
        {
            if (currentNode.Children == null) return null;
            var index = Array.FindIndex(currentNode.Children, node => name == node.FirstName);
            if (index == -1) return null;
            currentNode = currentNode.Children[index];
        }
        return currentNode;
    }

    /// <summary>
    /// The root node of the placeholder tree.
    /// </summary>
    public static PlaceHolder All => new([Encounter, Party, Player, Pet]);

    /// <summary>
    /// The encounter node of the placeholder tree.
    /// </summary>
    public static PlaceHolder Encounter => new("Encounter", [], new Func<IEncounter, IEncounter>(obj => obj),
    [
        new("Name", ["Encounter"], new Func<IEncounter, string>(obj => obj.Name)),
        new("Begin", ["Encounter"], new Func<IEncounter, DateTime?>(obj => obj.Begin)),
        new("End", ["Encounter"], new Func<IEncounter, DateTime?>(obj => obj.End)),
        new("Duration", ["Encounter"], new Func<IEncounter, TimeSpan>(obj => obj.Duration)),
        new("Partys", ["Encounter"], new Func<IEncounter, string>(obj => string.Join(", ", obj.Partys.Values.Select(party => party.ToString())))),
        ..IDataNodes<IEncounter>("Encounter"),
    ]);

    /// <summary>
    /// The party node of the placeholder tree.
    /// </summary>
    public static PlaceHolder Party => new("Party", [], new Func<IParty, IParty>(obj => obj),
    [
        new ("Players", ["Party"], new Func<IParty, string>(obj => string.Join(", ", obj.Players.Values.Select(party => party.ToString())))),
        new ("Pets", ["Party"], new Func<IParty, string>(obj => string.Join(", ", obj.Pets.Values.Select(party => party.ToString())))),
        ..IDataNodes<IParty>("Party"),
    ]);

    /// <summary>
    /// The player node of the placeholder tree.
    /// </summary>
    public static PlaceHolder Player => new("Player", [], new Func<IPlayer, IPlayer>(obj => obj),
    [
        new ("Name", ["Player"], new Func<IPlayer, string>(obj => obj.Name)),
        new ("World", ["Player"], new Func<IPlayer, uint?>(obj => obj.World)), // TODO: return World Object
        new ("Job", ["Player"], new Func<IPlayer, string>(obj => obj.Job.Name)), // TODO: return Job Object
        ..IDataNodes<IPlayer>("Player"),
    ]);

    /// <summary>
    /// The pet node of the placeholder tree.
    /// </summary>
    public static PlaceHolder Pet => new("Pet", [], new Func<IPet, IPet>(obj => obj),
    [
        new ("Name", ["Pet"], new Func<IPet, string>(obj => obj.Name)),
        new ("Owner", ["Pet"], new Func<IPet, IPlayer?>(obj => obj.Owner)),
        ..IDataNodes<IPet>("Pet"),
    ]);

    private static PlaceHolder[] IDataNodes<T>(string owner) where T : IData =>
    [
        new("DamageDealt", [owner], new Func<T, string>(obj => obj.DamageDealt.ToString()),
        [
            new("Value", [owner, "DamageDealt"], new Func<T, string>(obj => obj.DamageDealt.Value.ToString()),
            [
                new("Total", [owner, "DamageDealt", "Value"],  new Func<T, uint>(obj => obj.DamageDealt.Value.Total)),
                new("Critical", [owner, "DamageDealt", "Value"],  new Func<T, uint>(obj => obj.DamageDealt.Value.Critical)),
                new("Direct", [owner, "DamageDealt", "Value"],  new Func<T, uint>(obj => obj.DamageDealt.Value.Direct)),
                new("CriticalDirect", [owner, "DamageDealt", "Value"],  new Func<T, uint>(obj => obj.DamageDealt.Value.CriticalDirect)),
                new("Percent", [owner, "DamageDealt", "Value"],  new Func<T, string>(obj => obj.DamageDealt.Value.Percent.ToString()),
                [
                    new("Neutral", [owner, "DamageDealt", "Value", "Percent"], new Func<T, float>(obj => obj.DamageDealt.Value.Percent.Neutral)),
                    new("Critical", [owner, "DamageDealt", "Value", "Percent"], new Func<T, float>(obj => obj.DamageDealt.Value.Percent.Critical)),
                    new("Direct", [owner, "DamageDealt", "Value", "Percent"], new Func<T, float>(obj => obj.DamageDealt.Value.Percent.Direct)),
                    new("CriticalDirect", [owner, "DamageDealt", "Value", "Percent"], new Func<T, float>(obj => obj.DamageDealt.Value.Percent.CriticalDirect))
                ])
            ]),
            new("Count", [owner, "DamageDealt"], new Func<T, string>(obj => obj.DamageDealt.Count.ToString()),
            [
                new("Total", [owner, "DamageDealt", "Count"], new Func<T, uint>(obj => obj.DamageDealt.Count.Total)),
                new("Critical", [owner, "DamageDealt", " Count"], new Func<T, uint>(obj => obj.DamageDealt.Count.Critical)),
                new("Direct", [owner, "DamageDealt", "   Count"], new Func<T, uint>(obj => obj.DamageDealt.Count.Direct)),
                new("CriticalDirect", [owner, "DamageDealt", "   Count"], new Func<T, uint>(obj => obj.DamageDealt.Count.CriticalDirect)),
                new("Miss", [owner, "DamageDealt", " Count"], new Func<T, uint>(obj => obj.DamageDealt.Count.Miss)),
                new("Parry", [owner, "DamageDealt", "Count"], new Func<T, uint>(obj => obj.DamageDealt.Count.Parry)),
                new("Percent", [owner, "DamageDealt", "Count"], new Func<T, string>(obj => obj.DamageDealt.Count.Percent.ToString()),
                [
                    new("Neutral", [owner, "DamageDealt", "Count", "Percent"], new Func<T, float>(obj => obj.DamageDealt.Count.Percent.Neutral)),
                    new("Critical", [owner, "DamageDealt", "Count", "Percent"], new Func<T, float>(obj => obj.DamageDealt.Count.Percent.Critical)),
                    new("Direct", [owner, "DamageDealt", "Count", "Percent"], new Func<T, float>(obj => obj.DamageDealt.Count.Percent.Direct)),
                    new("CriticalDirect", [owner, "DamageDealt", "Count", "Percent"], new Func<T, float>(obj => obj.DamageDealt.Count.Percent.CriticalDirect)),
                    new("Miss", [owner, "DamageDealt", "Count", "Percent"], new Func<T, float>(obj => obj.DamageDealt.Count.Percent.Miss)),
                    new("Parry", [owner, "DamageDealt", "Count", "Percent"], new Func<T, float>(obj => obj.DamageDealt.Count.Percent.Parry))
                ])
            ])
        ]),
        new ("DamageReceived", [owner], new Func<T, string>(obj => obj.DamageReceived.ToString()),
        [
            new ("Value", [owner, "DamageReceived"], new Func<T, string>(obj => obj.DamageReceived.Value.ToString()),
            [
                new ("Total", [owner, "DamageReceived", "Value"], new Func<T, uint>(obj => obj.DamageReceived.Value.Total)),
                new ("Critical", [owner, "DamageReceived", "Value"], new Func<T, uint>(obj => obj.DamageReceived.Value.Critical)),
                new ("Direct", [owner, "DamageReceived", "Value"], new Func<T, uint>(obj => obj.DamageReceived.Value.Direct)),
                new ("CriticalDirect", [owner, "DamageReceived", "Value"], new Func<T, uint>(obj => obj.DamageReceived.Value.CriticalDirect)),
                new ("Percent", [owner, "DamageReceived", "Value"], new Func<T, string>(obj => obj.DamageReceived.Value.Percent.ToString()),
                [
                    new ("Neutral", [owner, "DamageReceived", "Value", "Percent"], new Func<T, float>(obj => obj.DamageReceived.Value.Percent.Neutral)),
                    new ("Critical", [owner, "DamageReceived", "Value", "Percent"], new Func<T, float>(obj => obj.DamageReceived.Value.Percent.Critical)),
                    new ("Direct", [owner, "DamageReceived", "Value", "Percent"], new Func<T, float>(obj => obj.DamageReceived.Value.Percent.Direct)),
                    new ("CriticalDirect", [owner, "DamageReceived", "Value", "Percent"], new Func<T, float>(obj => obj.DamageReceived.Value.Percent.CriticalDirect))
                ])
            ]),
            new ("Count", [owner, "DamageReceived"], new Func<T, string>(obj => obj.DamageReceived.Count.ToString()),
            [
                new ("Total", [owner, "DamageReceived", "Count"], new Func<T, uint>(obj => obj.DamageReceived.Count.Total)),
                new ("Critical", [owner, "DamageReceived", "Count"], new Func<T, uint>(obj => obj.DamageReceived.Count.Critical)),
                new ("Direct", [owner, "DamageReceived", "Count"], new Func<T, uint>(obj => obj.DamageReceived.Count.Direct)),
                new ("CriticalDirect", [owner, "DamageReceived", "Count"], new Func<T, uint>(obj => obj.DamageReceived.Count.CriticalDirect)),
                new ("Miss", [owner, "DamageReceived", "Count"], new Func<T, uint>(obj => obj.DamageReceived.Count.Miss)),
                new ("Parry", [owner, "DamageReceived", "Count"], new Func<T, uint>(obj => obj.DamageReceived.Count.Parry)),
                new ("Percent", [owner, "DamageReceived", "Count"], new Func<T, string>(obj => obj.DamageReceived.Count.Percent.ToString()),
                [
                    new ("Neutral", [owner, "DamageReceived", "Count", "Percent"], new Func<T, float>(obj => obj.DamageReceived.Count.Percent.Neutral)),
                    new ("Critical", [owner, "DamageReceived", "Count", "Percent"], new Func<T, float>(obj => obj.DamageReceived.Count.Percent.Critical)),
                    new ("Direct", [owner, "DamageReceived", "Count", "Percent"], new Func<T, float>(obj => obj.DamageReceived.Count.Percent.Direct)),
                    new ("CriticalDirect", [owner, "DamageReceived", "Count", "Percent"], new Func<T, float>(obj => obj.DamageReceived.Count.Percent.CriticalDirect)),
                    new ("Miss", [owner, "DamageReceived", "Count", "Percent"], new Func<T, float>(obj => obj.DamageReceived.Count.Percent.Miss)),
                    new ("Parry", [owner, "DamageReceived", "Count", "Percent"], new Func<T, float>(obj => obj.DamageReceived.Count.Percent.Parry))
                ])
            ])
        ]),
        new ("HealDealt", [owner], new Func<T, string>(obj => obj.HealDealt.ToString()),
        [
            new ("Value", [owner, "HealDealt"], new Func<T, string>(obj => obj.HealDealt.Value.ToString()),
            [
                new ("Total", [owner, "HealDealt", "Value"], new Func<T, uint>(obj => obj.HealDealt.Value.Total)),
                new ("Critical", [owner, "HealDealt", "Value"], new Func<T, uint>(obj => obj.HealDealt.Value.Critical)),
                new ("Percent", [owner, "HealDealt", "Value"], new Func<T, string>(obj => obj.HealDealt.Value.Percent.ToString()),
                [
                    new ("Neutral", [owner, "HealDealt", "Value", "Percent"], new Func<T, float>(obj => obj.HealDealt.Value.Percent.Neutral)),
                    new ("Critical", [owner, "HealDealt", "Value", "Percent"], new Func<T, float>(obj => obj.HealDealt.Value.Percent.Critical))
                ])
            ]),
            new ("Count", [owner, "HealDealt"], new Func<T, string>(obj => obj.HealDealt.Count.ToString()),
            [
                new ("Total", [owner, "HealDealt", "Count"], new Func<T, uint>(obj => obj.HealDealt.Count.Total)),
                new ("Critical", [owner, "HealDealt", "Count"], new Func<T, uint>(obj => obj.HealDealt.Count.Critical)),
                new ("Percent", [owner, "HealDealt", "Count"], new Func<T, string>(obj => obj.HealDealt.Count.Percent.ToString()),
                [
                    new ("Neutral", [owner, "HealDealt", "Count", "Percent"], new Func<T, float>(obj => obj.HealDealt.Count.Percent.Neutral)),
                    new ("Critical", [owner, "HealDealt", "Count", "Percent"], new Func<T, float>(obj => obj.HealDealt.Count.Percent.Critical))
                ])
            ])
        ]),
        new ("HealReceived", [owner], new Func<T, string>(obj => obj.HealReceived.ToString()),
        [
            new ("Value", [owner, "HealReceived"], new Func<T, string>(obj => obj.HealReceived.Value.ToString()),
            [
                new ("Total", [owner, "HealReceived", "Value"], new Func<T, uint>(obj => obj.HealReceived.Value.Total)),
                new ("Critical", [owner, "HealReceived", "Value"], new Func<T, uint>(obj => obj.HealReceived.Value.Critical)),
                new ("Percent", [owner, "HealReceived", "Value"], new Func<T, string>(obj => obj.HealReceived.Value.Percent.ToString()),
                [
                    new ("Neutral", [owner, "HealReceived", "Value", "Percent"], new Func<T, float>(obj => obj.HealReceived.Value.Percent.Neutral)),
                    new ("Critical", [owner, "HealReceived", "Value", "Percent"], new Func<T, float>(obj => obj.HealReceived.Value.Percent.Critical))
                ])
            ]),
            new ("Count", [owner, "HealReceived"], new Func<T, string>(obj => obj.HealReceived.Count.ToString()),
            [
                new ("Total", [owner, "HealReceived", "Count"], new Func<T, uint>(obj => obj.HealReceived.Count.Total)),
                new ("Critical", [owner, "HealReceived", "Count"], new Func<T, uint>(obj => obj.HealReceived.Count.Critical)),
                new ("Percent", [owner, "HealReceived", "Count"], new Func<T, string>(obj => obj.HealReceived.Count.Percent.ToString()),
                [
                    new ("Neutral", [owner, "HealReceived", "Count", "Percent"], new Func<T, float>(obj => obj.HealReceived.Count.Percent.Neutral)),
                    new ("Critical", [owner, "HealReceived", "Count", "Percent"], new Func<T, float>(obj => obj.HealReceived.Count.Percent.Critical))
                ])
            ])
        ]),
        new ("PerSeconds", [owner], new Func<T, string>(obj => obj.PerSeconds.ToString()),
        [

            /*
                These are double to diferentiate between Percent and PerSeconds.
                Format currently does not allow to set custom humanizations for each placeholder in an string
                this feature will be added eventualy.
            */

            new ("DamageDealt", [owner, "PerSeconds"], new Func<T, double>(obj => (double)obj.PerSeconds.DamageDealt)),
            new ("DamageReceived", [owner, "PerSeconds"], new Func<T, double>(obj => (double)obj.PerSeconds.DamageReceived)),
            new ("HealDealt", [owner, "PerSeconds"], new Func<T, double>(obj => (double)obj.PerSeconds.HealDealt)),
            new ("HealReceived", [owner, "PerSeconds"], new Func<T, double>(obj => (double)obj.PerSeconds.HealReceived))
        ])
    ];
}
