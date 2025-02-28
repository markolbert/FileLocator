using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public record ImportedColumn<TEntity, TProp> : IImportedColumn
{
    private readonly ILogger? _logger;
    private readonly Action<TEntity, object?> _setter;

    public ImportedColumn(
        int colNum,
        string colNameInSheet,
        Expression<Func<TEntity, TProp?>> propExpr,
        ILoggerFactory? loggerFactory
    )
    {
        _logger = loggerFactory?.CreateLogger( GetType() );

        ColumnNumber = colNum;
        ColumnNameInSheet = colNameInSheet;

        var exprHelper = new ExpressionHelpers( loggerFactory );

        if( !exprHelper.TryGetPropertyInfo( propExpr, out var propInfo ) )
        {
            _logger?.UnboundProperty( GetType(), propExpr.ToString(), "Unknown" );
            PropertyName = "Unknown";
        }
        else PropertyName = propInfo!.Name;

        _setter = exprHelper.CreateObjectPropertySetter( propExpr )
         ?? throw new FileUtilityException( GetType(),
                                            "ctor",
                                            $"Could not create property setter from '{propExpr}'" );
    }

    public int ColumnNumber { get; }
    public string ColumnNameInSheet { get; }
    public string PropertyName { get; }

    public bool SetValue( ISheet sheet, object entity, ICell cell )
    {
        if( entity is TEntity castEntity )
            return SetValue( sheet, castEntity, cell );

        _logger?.UnexpectedType( typeof( TEntity ), entity.GetType() );

        return false;
    }

    private bool SetValue( ISheet sheet, TEntity entity, ICell cell )
    {
        try
        {
            switch( Type.GetTypeCode( typeof( TProp ) ) )
            {
                case TypeCode.Boolean:
                    _setter( entity, cell.BooleanCellValue );
                    break;

                case TypeCode.DateTime:
                    _setter( entity, cell.DateCellValue );
                    break;

                case TypeCode.Int32:
                    var intValue = Convert.ToInt32( cell.NumericCellValue );
                    _setter( entity, intValue );

                    break;

                case TypeCode.Double:
                    _setter( entity, cell.NumericCellValue );
                    break;

                case TypeCode.String:
                    _setter( entity, cell.StringCellValue );
                    break;

                default:
                    _logger?.UnsupportedCellType( typeof( TProp ), sheet.SheetName );
                    break;
            }

            return true;
        }
        catch( Exception ex )
        {
            _logger?.SetValueFailed( typeof( TEntity ).Name, PropertyName, ex.Message );
            return false;
        }
    }
}
