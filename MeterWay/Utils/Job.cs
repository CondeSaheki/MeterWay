using System;
using Dalamud.Plugin.Services;

namespace MeterWay.Utils;

/// <summary>
/// Represents an FFXIV job.
/// </summary>
public class Job
{
    public int Id { get; private init; }
    public string Acronym { get; private init; }
    public string Name { get; private init; }

    // ?

    public static readonly Job Unknown = new(-1, "UNK", "Unknown");
    public static readonly Job Adventurer = new(0, "ADV", "Adventurer");

    // Tank

    public static readonly Job Gladiator = new(1, "GLA", "Gladiator");
    public static readonly Job Paladin = new(19, "PLD", "Paladin");
    public static readonly Job Marauder = new(3, "MRD", "Marauder");
    public static readonly Job Warrior = new(21, "WAR", "Warrior");
    public static readonly Job DarkKnight = new(32, "DRK", "Dark Knight");
    public static readonly Job GunBreaker = new(37, "GNB", "GunBreaker");

    // Melee Dps

    public static readonly Job Pugilist = new(2, "PGL", "Pugilist");
    public static readonly Job Monk = new(20, "MNK", "Monk");
    public static readonly Job Lancer = new(4, "LNC", "Lancer");
    public static readonly Job Dragoon = new(22, "DRG", "Dragoon");
    public static readonly Job Rogue = new(29, "ROG", "Rogue");
    public static readonly Job Ninja = new(30, "NIN", "Ninja");
    public static readonly Job Samurai = new(34, "SAM", "Samurai");
    public static readonly Job Reaper = new(39, "RPR", "Reaper");
    public static readonly Job Viper = new(41, "VPR", "Viper");

    // Healer

    public static readonly Job Conjurer = new(6, "CNJ", "Conjurer");
    public static readonly Job WhiteMage = new(24, "WHM", "White Mage");
    public static readonly Job Scholar = new(28, "SCH", "Scholar");
    public static readonly Job Astrologian = new(33, "AST", "Astrologian");
    public static readonly Job Sage = new(40, "SGE", "Sage");

    // Physical Ranged Dps

    public static readonly Job Archer = new(5, "ARC", "Archer");
    public static readonly Job Bard = new(23, "BRD", "Bard");
    public static readonly Job Machinist = new(31, "MCH", "Machinist");
    public static readonly Job Dancer = new(38, "DNC", "Dancer");

    // Magical Ranged Dps

    public static readonly Job Thaumaturge = new(7, "THM", "Thaumaturge");
    public static readonly Job BlackMage = new(25, "BLM", "Black Mage");
    public static readonly Job Arcanist = new(26, "ACN", "Arcanist");
    public static readonly Job Summoner = new(27, "SMN", "Summoner");
    public static readonly Job RedMage = new(35, "RDM", "Red Mage");
    public static readonly Job Pictomancer = new(42, "PCT", "Pictomancer");
    public static readonly Job BlueMage = new(36, "BLU", "Blue Mage"); // no included in any "Job.Is..." -> Job.IsBlueMage

    // Disciples of the Hand

    public static readonly Job Carpenter = new(8, "CRP", "Carpenter");
    public static readonly Job Blacksmith = new(9, "BSM", "Blacksmith");
    public static readonly Job Armorer = new(10, "ARM", "Armorer");
    public static readonly Job Goldsmith = new(11, "GSM", "Goldsmith");
    public static readonly Job Leatherworker = new(12, "LTW", "Leatherworker");
    public static readonly Job Weaver = new(13, "WVR", "Weaver");
    public static readonly Job Alchemist = new(14, "ALC", "Alchemist");
    public static readonly Job Culinarian = new(15, "CUL", "Culinarian");

    // Disciples of the Land

    public static readonly Job Miner = new(16, "MIN", "Miner");
    public static readonly Job Botanist = new(17, "BTN", "Botanist");
    public static readonly Job Fisher = new(18, "FSH", "Fisher");

    private static readonly Job[] Jobs =
    [
        Unknown,
        Adventurer,
        Gladiator,
        Pugilist,
        Marauder,
        Lancer,
        Archer,
        Conjurer,
        Thaumaturge,
        Carpenter,
        Blacksmith,
        Armorer,
        Goldsmith,
        Leatherworker,
        Weaver,
        Alchemist,
        Culinarian,
        Miner,
        Botanist,
        Fisher,
        Paladin,
        Monk,
        Warrior,
        Dragoon,
        Bard,
        WhiteMage,
        BlackMage,
        Arcanist,
        Summoner,
        Scholar,
        Rogue,
        Ninja,
        Machinist,
        DarkKnight,
        Astrologian,
        Samurai,
        RedMage,
        BlueMage,
        GunBreaker,
        Dancer,
        Reaper,
        Sage,
        Viper,
        Pictomancer
    ];

    private Job(int id, string acronym, string name)
    {
        Id = id;
        Name = name;
        Acronym = acronym;
    }

    /// <summary>
    /// Retrieves the icon handle for the specified job.
    /// </summary>
    /// <param name="job">The job for which to retrieve the icon handle.</param>
    /// <returns>The icon handle if the job is special, otherwise null.</returns>
    public nint? Icon()
    {
        if (IsEspecial(this)) return null;
        if (!Dalamud.Textures.TryGetFromGameIcon((uint)Id + 62000u, out var icon)) return null;
        if (!icon.TryGetWrap(out var wrap, out _)) return null;
        return wrap.ImGuiHandle;
    }

    /// <summary>
    /// Retrieves a Job object from the Jobs array based on the provided ID.
    /// </summary>
    /// <param name="id">The ID of the Job to retrieve.</param>
    /// <returns>The Job object with the matching ID, or null if no match is found.</returns>
    public static Job? FromId(int id)
    {
        var index = Array.FindIndex(Jobs, job => job.Id == id);
        return index == -1 ? null : Jobs[index];
    }

    /// <summary>
    /// Retrieves a Job object from the Jobs array based on the provided name.
    /// </summary>
    /// <param name="name">The name of the Job to retrieve.</param>
    /// <returns>The Job object with the matching name, or null if no match is found.</returns>
    public static Job? FromName(string name)
    {
        var index = Array.FindIndex(Jobs, job => job.Name == name);
        return index == -1 ? null : Jobs[index];
    }

    /// <summary>
    /// Retrieves a Job object from the Jobs array based on the provided acronym.
    /// </summary>
    /// <param name="acronym">The acronym of the Job to retrieve.</param>
    /// <returns>The Job object with the matching acronym, or null if no match is found.</returns>
    public static Job? FromAcronym(string acronym)
    {
        var index = Array.FindIndex(Jobs, job => job.Acronym == acronym);
        return index == -1 ? null : Jobs[index];
    }

    /// <summary>
    /// Checks if the job is a tank.
    /// </summary>
    /// <param name="job">The job to check.</param>
    /// <returns>True if the job is a tank, false otherwise.</returns>
    public static bool IsTank(Job job)
    {
        return job.Id == 1 ||
            job.Id == 19 ||
            job.Id == 3 ||
            job.Id == 21 ||
            job.Id == 32 ||
            job.Id == 37;
    }

    /// <summary>
    /// Checks if the job is a melee DPS.
    /// </summary>
    /// <param name="job">The job to check.</param>
    /// <returns>True if the job is a melee DPS, false otherwise.</returns>
    public static bool IsMeleeDps(Job job)
    {
        return job.Id == 2 ||
            job.Id == 20 ||
            job.Id == 4 ||
            job.Id == 22 ||
            job.Id == 29 ||
            job.Id == 30 ||
            job.Id == 34 ||
            job.Id == 39 ||
            job.Id == 41;
    }

    /// <summary>
    /// Checks if the job is a healer.
    /// </summary>
    /// <param name="job">The job to check.</param>
    /// <returns>True if the job is a healer, false otherwise.</returns>
    public static bool IsHealer(Job job)
    {
        return job.Id == 6 ||
            job.Id == 24 ||
            job.Id == 28 ||
            job.Id == 33 ||
            job.Id == 40;
    }

    /// <summary>
    /// Checks if the job is a physical ranged DPS.
    /// </summary>
    /// <param name="job">The job to check.</param>
    /// <returns>True if the job is a physical ranged DPS, false otherwise.</returns>
    public static bool IsPhysicalRangedDps(Job job)
    {
        return job.Id == 5 ||
            job.Id == 23 ||
            job.Id == 31 ||
            job.Id == 38;
    }

    /// <summary>
    /// Checks if the job is a magical ranged DPS.
    /// </summary>
    /// <param name="job">The job to check.</param>
    /// <returns>True if the job is a magical ranged DPS, false otherwise.</returns>
    public static bool IsMagicalRangedDps(Job job)
    {
        return job.Id == 7 ||
            job.Id == 25 ||
            job.Id == 26 ||
            job.Id == 27 ||
            job.Id == 35 ||
            job.Id == 42;
    }

    /// <summary>
    /// Checks if the job is a damage dealer.
    /// </summary>
    /// <param name="job">The job to check.</param>
    /// <returns>True if the job is a damage dealer, false otherwise.</returns>
    public static bool IsDamageDealer(Job job)
    {
        return IsMeleeDps(job) ||
            IsPhysicalRangedDps(job) ||
            IsMagicalRangedDps(job);
    }

    /// <summary>
    /// Checks if the job is a support.
    /// </summary>
    /// <param name="job">The job to check.</param>
    /// <returns>True if the job is a support, false otherwise.</returns>
    public static bool IsSupport(Job job)
    {
        return IsTank(job) ||
            IsHealer(job);
    }

    /// <summary>
    /// Checks if the job is a melee.
    /// </summary>
    /// <param name="job">The job to check.</param>
    /// <returns>True if the job is a melee, false otherwise.</returns>
    public static bool IsMelee(Job job)
    {
        return IsMeleeDps(job) ||
            IsTank(job);
    }

    /// <summary>
    /// Checks if the job is a ranged.
    /// </summary>
    /// <param name="job">The job to check.</param>
    /// <returns>True if the job is a ranged, false otherwise.</returns>
    public static bool IsRanged(Job job)
    {
        return IsHealer(job) ||
            IsPhysicalRangedDps(job) ||
            IsMagicalRangedDps(job);
    }

    /// <summary>
    /// Checks if the job is a caster.
    /// </summary>
    /// <param name="job">The job to check.</param>
    /// <returns>True if the job is a caster, false otherwise.</returns>
    public static bool IsCaster(Job job)
    {
        return IsHealer(job) ||
            IsMagicalRangedDps(job);
    }

    /// <summary>
    /// Checks if the job is magical.
    /// </summary>
    /// <param name="job">The job to check.</param>
    /// <returns>True if the job is magical, false otherwise.</returns>
    public static bool IsMagical(Job job) => IsCaster(job);

    /// <summary>
    /// Checks if the job is physical.
    /// </summary>
    /// <param name="job">The job to check.</param>
    /// <returns>True if the job is physical, false otherwise.</returns>
    public static bool IsPhysical(Job job)
    {
        return IsPhysicalRangedDps(job) ||
            IsMeleeDps(job) ||
            IsTank(job);
    }

    /// <summary>
    /// Checks if the job is a Disciple of the Hand.
    /// </summary>
    /// <param name="job">The job to check.</param>
    /// <returns>True if the job is a Disciple of the Hand, false otherwise.</returns>
    public static bool IsDiscipleOfTheHand(Job job)
    {
        return job.Id == 8 ||
            job.Id == 9 ||
            job.Id == 10 ||
            job.Id == 11 ||
            job.Id == 12 ||
            job.Id == 13 ||
            job.Id == 14 ||
            job.Id == 15;
    }

    /// <summary>
    /// Checks if the job is a Disciple of the Hand.
    /// </summary>
    /// <param name="job">The job to check.</param>
    /// <returns>True if the job is a Disciple of the Hand, false otherwise.</returns>
    public static bool IsCrafter(Job job) => IsDiscipleOfTheHand(job);

    /// <summary>
    /// Checks if the job is a Disciple of the Land.
    /// </summary>
    /// <param name="job">The job to check.</param>
    /// <returns>True if the job is a Disciple of the Land, false otherwise.</returns>
    public static bool IsDiscipleOfTheLand(Job job)
    {
        return job.Id == 16 ||
            job.Id == 17 ||
            job.Id == 18;
    }

    /// <summary>
    /// Checks if the job is a gatherer.
    /// </summary>
    /// <param name="job">The job to check.</param>
    /// <returns>True if the job is a gatherer, false otherwise.</returns>
    public static bool IsGatherer(Job job) => IsDiscipleOfTheLand(job);

    /// <summary>
    /// Checks if the job is special.
    /// </summary>
    /// <param name="job">The job to check.</param>
    /// <returns>True if the job is special, false otherwise.</returns>
    public static bool IsEspecial(Job job)
    {
        return job.Id == -1 ||
            job.Id == 0;
    }

    /// <summary>
    /// Checks if the job is a Blue Mage.
    /// </summary>
    /// <param name="job">The job to check.</param>
    /// <returns>True if the job is a Blue Mage, false otherwise.</returns>
    public static bool IsBlueMage(Job job)
    {
        return job.Id == 36;
    }

    /// <summary>
    /// Checks if the job is an initial job.
    /// </summary>
    /// <param name="job">The job to check.</param>
    /// <returns>True if the job is an initial job, false otherwise.</returns>
    public static bool IsInitial(Job job)
    {
        return job.Id == 1 ||
        job.Id == 3 ||
        job.Id == 2 ||
        job.Id == 4 ||
        job.Id == 29 ||
        job.Id == 6 ||
        job.Id == 5 ||
        job.Id == 7 ||
        job.Id == 26;
    }

    public static bool operator ==(Job left, Job right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Id == right.Id;
    }

    public static bool operator !=(Job left, Job right) => !(left == right);

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj is null || obj.GetType() != GetType()) return false;

        var other = (Job)obj;
        return Id == other.Id;
    }

    public override int GetHashCode() => HashCode.Combine(Id);

    public override string ToString() => $"Id: {Id}, Acronym: {Acronym}, Name: {Name}";
}
