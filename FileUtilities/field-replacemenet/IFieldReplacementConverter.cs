namespace J4JSoftware.FileUtilities;

public interface IFieldReplacementConverter
{
    object? GetParsedValue( string text );
}

public interface IFieldReplacementConverter<out T> : IFieldReplacementConverter
{
    T Parse( string text );
}
