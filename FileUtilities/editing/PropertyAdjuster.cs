using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

// this is the base class for objects that adjust the content
// of a field based solely on its value, or on the values of
// related fields
public abstract class PropertyAdjuster<TProp> : IPropertyAdjuster<TProp>
{
    private readonly IEqualityComparer<TProp>? _equalityComparer;

    protected PropertyAdjuster(
        ILoggerFactory? loggerFactory,
        IEqualityComparer<TProp>? equalityComparer = null
    )
    {
        Logger = loggerFactory?.CreateLogger(GetType());

        _equalityComparer = equalityComparer;
    }

    protected ILogger? Logger { get; }

    public Type PropertyType => typeof( TProp );

    public virtual bool AdjustField( TProp? propValue, out TProp adjValue )
    {
        var initialValue = propValue;
        adjValue = AdjustFieldInternal( propValue );

        return !(_equalityComparer?.Equals( initialValue, adjValue ) ?? initialValue?.Equals( adjValue ) ?? false);
    }

    protected abstract TProp AdjustFieldInternal( TProp? propValue );

    bool IPropertyAdjuster.AdjustField(object? propValue, out object? adjValue )
    {
        adjValue = null;

        if ( propValue is TProp castValue )
        {
            if( AdjustField( castValue, out var temp ) )
            {
                adjValue = temp;
                return true;
            }

            return false;
        }

        Logger?.InvalidTypeAssignment(propValue?.GetType() ?? typeof(object), typeof(TProp));

        return false;
    }
}
