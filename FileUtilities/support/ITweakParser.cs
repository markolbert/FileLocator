namespace J4JSoftware.FileUtilities;

public interface ITweakParser
{
    object? GetParsedValue( string text );
}

public interface ITweakParser<out T> : ITweakParser
{
    T Parse( string text );
}
