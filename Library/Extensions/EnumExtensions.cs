namespace Library.Extensions;

public static partial class Extensions
{
    public static T GetAttribute<T>(this Enum value) where T : Attribute
    {
        var enumType = value.GetType();

        var name = Enum.GetName(enumType, value);
        if (name is null) throw new NullReferenceException("You requested an invalid enum attribute.");

        var fieldName = enumType.GetField(name);
        return fieldName is null ? throw new NullReferenceException("You requested an invalid enum attribute field.") : fieldName.GetCustomAttributes(false).OfType<T>().Single();
    }
}
