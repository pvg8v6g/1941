using Library.Models.Enums;

namespace Library.Models.Structures;

public class AntiAirGun : BaseUnit
{
    public override required string Name { get; init; } = "Anti-Air Gun";

    public override required int Cost { get; init; } = 800;

    public override required int Move { get; init; } = 1;

    public override required int Attack { get; init; } = 0;

    public override required int Defense { get; init; } = 35;

    public override required int TransportCost { get; init; } = 1;

    public override required Classification Classification { get; init; } = Classification.Structure;

    public override required Weapon[] Weapons { get; init; } = [Weapon.Flak];
}
