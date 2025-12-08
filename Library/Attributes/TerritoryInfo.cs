using Library.Enumerations;

namespace Library.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class TerritoryInfo(string name, Faction startingFaction) : Attribute
{
    public string Name { get; set; } = name;

    public Faction StartingFaction { get; set; } = startingFaction;
}
