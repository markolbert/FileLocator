using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public interface INpoiConverter
{
    Type TargetType { get; }
    object? ConvertValue( ICell cell );
}

public interface INpoiConverter<out T> : INpoiConverter
{
    T Convert(ICell cell);
}