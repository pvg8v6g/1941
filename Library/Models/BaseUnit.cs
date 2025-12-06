namespace Library.Models;

public abstract class BaseUnit
{

    public abstract required string Name { get; init; }

    public abstract required int Cost { get; init; }

    public abstract required int Move { get; init; }

    public abstract required int TransportCost { get; init; }

}
