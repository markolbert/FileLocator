namespace J4JSoftware.FileUtilities;

public interface IPropertyAdjuster
{
    Type PropertyType { get; }
    
    bool AdjustField( object? propValue, out object? adjValue );
}

public interface IPropertyAdjuster<TProp> : IPropertyAdjuster
{
    bool AdjustField( TProp propValue, out TProp adjValue );
}