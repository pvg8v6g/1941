namespace Library.Models.Infantry;

public class Conscript : BaseUnit
{

    public override required string Name { get; init; } = "Conscript";

    public override required int Cost { get; init; }

    public override required int Move { get; init; }

    public override required int TransportCost { get; init; }

}
