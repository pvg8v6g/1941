using Library.Models.Enums;

namespace Library.Models.Infantry;

public class AntiArmor : BaseUnit
{

    public override required string Name { get; init; } = "Anti-Armor";

    public override required int Cost { get; init; } = 200;

    public override required int Move { get; init; } = 1;
    
    public override required int Attack { get; init; } = 25;

    public override required int Defense { get; init; } = 40;

    public override required int TransportCost { get; init; } = 1;

    public override required Classification Classification { get; init; } = Classification.Infantry;

    public override required Weapon[] Weapons { get; init; } = [Weapon.RPG];

}
