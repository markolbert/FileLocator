using System.Reflection;
using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public class ImportedColumn<TEntity> : IImportedColumn<TEntity>
{
    private readonly Action<TEntity, object?> _setter;
    private readonly INpoiConverter _converter;
    private readonly ILogger? _logger;

    public ImportedColumn(
        string colNameInSheet, 
        PropertyInfo propInfo,
        INpoiConverter converter,
        ILoggerFactory? loggerFactory
    )
    {
        _logger = loggerFactory?.CreateLogger<ImportedColumn<TEntity>>();

        ColumnNameInSheet = colNameInSheet;
        PropertyName = propInfo.Name;
        PropertyType = propInfo.PropertyType;
        _converter = converter;

        _setter = ( e, v ) => propInfo.SetValue( e, v );
    }

    public string ColumnNameInSheet { get; }
    public int ColumnNumber { get; set; } = -1;
    public string PropertyName { get; }
    public Type PropertyType { get; }

    public bool SetValue( ISheet sheet, TEntity entity, ICell cell )
    {
        if( ColumnNumber < 0 )
        {
            _logger?.InvalidNpoiMapping( PropertyName, typeof( TEntity ).Name, ColumnNameInSheet, PropertyType.Name );
            return false;
        }

        try
        {
            _setter( entity, _converter.ConvertValue( cell ) );

            return true;
        }
        catch( Exception ex )
        {
            _logger?.SetValueFailed( typeof( TEntity ).Name, PropertyName, ex.Message );
            return false;
        }
    }

    bool IImportedColumn.SetValue( ISheet sheet, object entity, ICell cell )
    {
        if (entity is TEntity castEntity)
            return SetValue(sheet, castEntity, cell);

        _logger?.UnexpectedType(typeof(TEntity), entity.GetType());

        return false;
    }
}
