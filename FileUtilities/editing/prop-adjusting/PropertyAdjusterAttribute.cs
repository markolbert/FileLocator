using System.Reflection;
using Microsoft.Extensions.Logging;
using NPOI.SS.Formula.Functions;
#pragma warning disable CS8618, CS9264

namespace J4JSoftware.FileUtilities;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class PropertyAdjusterAttribute : Attribute
{
    private readonly ConstructorInfo _adjusterInfo;
    private readonly ConstructorInfo _filterInfo;

    public PropertyAdjusterAttribute(
        Type adjusterType,
        Type? entityFilterType = null
        )
    {
        // we have to ensure adjusterType has a public parameterless ctor, or
        // one that accepts just an ILoggerFactory? argument, and that it 
        // implements the IFieldAdjuster interface
        if( adjusterType.GetInterface( nameof( IPropertyAdjuster ) ) == null )
            throw new MissingInterfaceException( adjusterType, typeof( IPropertyAdjuster ) );

        if( entityFilterType != null && entityFilterType.GetInterface(nameof(IEntityFilter)) == null)
            throw new MissingInterfaceException(entityFilterType, typeof(IEntityFilter));

        var temp = adjusterType.CheckTypeForValidConstructors();

        if( temp.ConstructorInfo == null )
            throw new MissingConstructorException( adjusterType,
                                                   $"does not have a public parameterless constructor, or a public constructor accepting only an instance of {nameof( ILoggerFactory )}?" );

        _adjusterInfo = temp.ConstructorInfo;
        AdjusterAllowsLogging = temp.AllowsLoggingFactory;
        
        AdjusterType = adjusterType;

        if( entityFilterType == null )
            return;

        temp = entityFilterType.CheckTypeForValidConstructors();

        if (temp.ConstructorInfo == null)
            throw new MissingConstructorException(entityFilterType,
                                                  $"does not have a public parameterless constructor, or a public constructor accepting only an instance of {nameof(ILoggerFactory)}?");
        _filterInfo = temp.ConstructorInfo;
        FilterAllowsLogging = temp.AllowsLoggingFactory;

        FilterType = entityFilterType;
    }

    public Type AdjusterType { get; }
    public bool AdjusterAllowsLogging { get; set; }

    public Type? FilterType { get; }
    public bool FilterAllowsLogging { get; set; }

    public bool TryCreateAdjuster( ILoggerFactory? loggerFactory, out IPropertyAdjuster? cleaner )
    {
        cleaner = null;

        var args = AdjusterAllowsLogging ? new object?[] { loggerFactory } : Array.Empty<object?>();

        try
        {
            cleaner = (IPropertyAdjuster) _adjusterInfo.Invoke( args );
        }
        catch( Exception ex )
        {
            return false;
        }

        return true;
    }

    public bool TryCreateFilter( ILoggerFactory? loggerFactory, out IEntityFilter? filter )
    {
        filter = null;

        var args = FilterAllowsLogging ? new object?[] { loggerFactory } : Array.Empty<object?>();

        try
        {
            filter = (IEntityFilter) _filterInfo.Invoke( args );
        }
        catch( Exception ex )
        {
            return false;
        }

        return true;
    }
}
