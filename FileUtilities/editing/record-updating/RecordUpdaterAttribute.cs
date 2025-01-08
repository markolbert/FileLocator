using Microsoft.Extensions.Logging;
using System.Reflection;

namespace J4JSoftware.FileUtilities;

[ AttributeUsage( AttributeTargets.Class ) ]
public class RecordUpdaterAttribute : Attribute
{
    private readonly ConstructorInfo _constructorInfo;

    public RecordUpdaterAttribute(
        Type recordUpdaterType
        )
    {
        // we have to ensure adjusterType has a public parameterless ctor, or
        // one that accepts just an ILoggerFactory? argument, and that it
        // implements the IRecordUpdater interface
        if( recordUpdaterType.GetInterface( nameof( IRecordUpdater ) ) == null )
            throw new MissingInterfaceException( recordUpdaterType, typeof( IRecordUpdater ) );

        ConstructorInfo? curInfo = null;

        foreach( var info in recordUpdaterType.GetConstructors()
                                              .Where( x => x.IsPublic )
                                              .Select( x => new
                                               {
                                                   ConstructorInfo = x, Parameters = x.GetParameters()
                                               } )
                                              .OrderByDescending( x => x.Parameters.Length ) )
        {
            switch( info.Parameters.Length )
            {
                case 0:
                    curInfo = info.ConstructorInfo;
                    break;

                case 1:
                    if( info.Parameters[ 0 ].ParameterType == typeof( ILoggerFactory ) )
                    {
                        curInfo = info.ConstructorInfo;
                        AllowsLoggingFactory = true;
                    }

                    break;

                default:
                    continue;
            }

            if( curInfo != null )
                break;
        }

        if (curInfo == null)
            throw new MissingConstructorException(recordUpdaterType,
                                                  $"does not have a public parameterless constructor, or a public constructor accepting only an instance of {nameof(ILoggerFactory)}?");

        _constructorInfo = curInfo;

        RecordUpdaterType = recordUpdaterType;
    }

    public bool AllowsLoggingFactory { get; set; }
    public Type RecordUpdaterType { get; }

    public bool TryCreateUpdater(ILoggerFactory? loggerFactory, out IRecordUpdater? updater)
    {
        updater = null;

        var args = AllowsLoggingFactory ? new object?[] { loggerFactory } : Array.Empty<object?>();

        try
        {
            updater = (IRecordUpdater) _constructorInfo.Invoke( args );
        }
        catch (Exception ex)
        {
            return false;
        }

        return true;
    }
}
