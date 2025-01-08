using System.Reflection;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class PropertyAdjusterAttribute : Attribute
{
    private readonly ConstructorInfo _constructorInfo;

    public PropertyAdjusterAttribute(
        Type adjusterType
        )
    {
        // we have to ensure adjusterType has a public parameterless ctor, or
        // one that accepts just an ILoggerFactory? argument, and that it 
        // implements the IFieldAdjuster interface
        if( adjusterType.GetInterface( nameof( IPropertyAdjuster ) ) == null )
            throw new MissingInterfaceException( adjusterType, typeof( IPropertyAdjuster ) );

        ConstructorInfo? curInfo = null;

        foreach( var info in adjusterType.GetConstructors()
                                        .Where( x => x.IsPublic )
                                        .Select( x => new
                                         {
                                             ConstructorInfo = x, Parameters = x.GetParameters()
                                         } )
                                        .OrderByDescending(x=>x.Parameters.Length))
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

        if( curInfo == null )
            throw new MissingConstructorException( adjusterType,
                                                   $"does not have a public parameterless constructor, or a public constructor accepting only an instance of {nameof( ILoggerFactory )}?" );

        _constructorInfo = curInfo;
        
        AdjusterType = adjusterType;
    }

    public bool AllowsLoggingFactory { get; set; }
    public Type AdjusterType { get; }

    public bool TryCreateAdjuster( ILoggerFactory? loggerFactory, out IPropertyAdjuster? cleaner )
    {
        cleaner = null;

        var args = AllowsLoggingFactory ? new object?[] { loggerFactory } : Array.Empty<object?>();

        try
        {
            cleaner = (IPropertyAdjuster) _constructorInfo.Invoke( args );
        }
        catch( Exception ex )
        {
            return false;
        }

        return true;
    }
}
