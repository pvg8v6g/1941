using Library.Enumerations;

namespace Library.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class FactionColor(string color) : Attribute
{
    public string Color { get; set; } = color;
}
