using Library.Attributes;

namespace Library.Enumerations;

public enum Faction
{
    [FactionColor("#69001b")]
    Russia,

    [FactionColor("#5f6575")]
    Germany,

    [FactionColor("#29846f")]
    Italy,

    [FactionColor("#4169e1")]
    UnitedKingdom,

    [FactionColor("#ff0047")]
    Japan,

    [FactionColor("#acab0a")]
    China,

    [FactionColor("#834d96")]
    Australia,

    [FactionColor("#438f43")]
    UnitedStates,

    [FactionColor("#e2acc1")]
    Neutral
}
