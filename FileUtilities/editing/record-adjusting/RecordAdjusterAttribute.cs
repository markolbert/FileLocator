using Microsoft.Extensions.Logging;
using System.Reflection;

namespace J4JSoftware.FileUtilities;

[ AttributeUsage( AttributeTargets.Class ) ]
public class RecordAdjusterAttribute : Attribute
{
    private readonly ConstructorInfo _constructorInfo;

    public RecordAdjusterAttribute(
        Type recordUpdaterType
        )
    {
        // we have to ensure adjusterType has a public parameterless ctor, or
        // one that accepts just an ILoggerFactory? argument, and that it
        // implements the IRecordUpdater interface
        if( recordUpdaterType.GetInterface( nameof( IRecordAdjuster ) ) == null )
            throw new MissingInterfaceException( recordUpdaterType, typeof( IRecordAdjuster ) );

        var temp = recordUpdaterType.CheckTypeForValidConstructors();

        if (temp.ConstructorInfo== null)
            throw new MissingConstructorException(recordUpdaterType,
                                                  $"does not have a public parameterless constructor, or a public constructor accepting only an instance of {nameof(ILoggerFactory)}?");

        _constructorInfo = temp.ConstructorInfo;
        AllowsLoggingFactory = temp.AllowsLoggingFactory;

        RecordAdjusterType = recordUpdaterType;
    }

    public bool AllowsLoggingFactory { get; set; }
    public Type RecordAdjusterType { get; }

    public bool TryCreateAdjuster(ILoggerFactory? loggerFactory, out IRecordAdjuster? updater)
    {
        updater = null;

        var args = AllowsLoggingFactory ? new object?[] { loggerFactory } : Array.Empty<object?>();

        try
        {
            updater = (IRecordAdjuster) _constructorInfo.Invoke( args );
        }
        catch (Exception ex)
        {
            return false;
        }

        return true;
    }
}
