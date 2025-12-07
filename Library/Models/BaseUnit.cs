using Library.Enumerations;
using Library.Models.Enums;

namespace Library.Models;

public abstract class BaseUnit
{

    public abstract required string Name { get; init; }

    public abstract required int Cost { get; init; }

    public abstract required int Move { get; init; }

    public abstract required int Attack { get; init; }

    public abstract required int Defense { get; init; }

    public abstract required int TransportCost { get; init; }

    public abstract required Classification Classification { get; init; }

    public abstract required Weapon[] Weapons { get; init; }

    public required Faction Faction { get; init; }

}
