namespace J4JSoftware.FileUtilities;

public interface IPropertyAdjuster
{
    Type PropertyType { get; }
    
    object? AdjustField( object? propValue );
}

public interface IPropertyAdjuster<TProp> : IPropertyAdjuster
{
    TProp AdjustField( TProp propValue );
}