using System.Linq.Expressions;
using J4JSoftware.Utilities;
using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public class ExportableVector<TEntity, TProp> : ExportableColumnBase, IExportableColumn<TEntity, TProp>
    where TEntity : class
{
    private readonly Func<TEntity, List<TProp>> _getter;

    public ExportableVector(
        ITableCreator<TEntity> tableCreator,
        Expression<Func<TEntity, List<TProp>>> propExpr,
        StyleSetBase styleSet,
        ILoggerFactory? loggerFactory
    )
        : base( typeof( TEntity ), typeof( TProp ), tableCreator, styleSet, loggerFactory )
    {
        _getter = propExpr.Compile();

        var exprHelper = new ExpressionHelpers( LoggerFactory );

        if( !exprHelper.TryGetPropertyInfo( propExpr, out var propInfo ) )
        {
            Logger?.UnboundProperty( GetType(), propExpr.ToString(), "Unknown" );
            BoundProperty = "Unknown";
        }
        else BoundProperty = propInfo!.Name;

        ColumnsNeeded = TableCreator.Data.Count == 0 ? 0 : TableCreator.Data.Max( r => _getter( r ).Count );
    }

    public ITableCreator<TEntity> TableCreator => (ITableCreator<TEntity>) Creator;

    public override int ColumnsNeeded { get; }
    public List<Aggregator<TEntity, TProp>> Aggregators { get; } = [];

    public override bool PopulateSheet( IWorkbook workbook, int startingRow, int startingCol )
    {
        if( TableCreator.Sheet == null )
        {
            Logger?.UndefinedSheet();
            return false;
        }

        // if nothing to export, just return
        if( TableCreator.Data.Count == 0 )
            return true;

        for( var row = 0; row < TableCreator.Data.Count; row++ )
        {
            var vectorData = _getter( TableCreator.Data[ row ] );

            for( var col = 0; col < vectorData.Count; col++ )
            {
                if( col >= vectorData.Count )
                {
                    Logger?.TruncatedDataVector( row, col );
                    return false;
                }

                CreateCell( workbook, row + startingRow, startingCol + col, vectorData[ col ] );
            }
        }

        var sumAgg = Aggregators.FirstOrDefault( a => a.AggregateFunction == AggregateFunction.Sum );
        var aggRows = sumAgg == null ? 0 : 1;

        sumAgg?.PopulateSheet( workbook, startingRow, startingCol );

        foreach( var agg in Aggregators.Where( a => a.AggregateFunction != AggregateFunction.Sum ) )
        {
            agg.PopulateSheet( workbook, startingRow + aggRows, startingCol );
            aggRows++;
        }

        return true;
    }
}
