using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

// this is the base class for objects that adjust the content
// of a field based solely on its value, or on the values of
// related fields
public abstract class PropertyAdjuster<TProp> : IPropertyAdjuster<TProp>
{
    protected PropertyAdjuster(
        ILoggerFactory? loggerFactory
    )
    {
        Logger = loggerFactory?.CreateLogger( GetType() );
    }
    protected ILogger? Logger { get; }

    public Type PropertyType => typeof( TProp );

    public abstract TProp AdjustField( TProp propValue );

    object? IPropertyAdjuster.AdjustField(object? propValue)
    {
        if (propValue is TProp castValue)
            return AdjustField(castValue);

        Logger?.InvalidTypeAssignment(propValue?.GetType() ?? typeof(object), typeof(TProp));

        return null;
    }
}
