using System.Linq.Expressions;
using J4JSoftware.Utilities;
using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public class ExportableColumn<TEntity, TProp>(
    ITableCreator<TEntity> tableCreator,
    Expression<Func<TEntity, TProp?>> propExpr,
    StyleSetBase styleSet,
    ILoggerFactory? loggerFactory
)
    : ExportableColumnBase( typeof( TEntity ),
                            typeof( TProp ),
                            GetBoundPropertyName( propExpr ),
                            tableCreator,
                            styleSet,
                            loggerFactory ),
        IExportableColumn<TEntity, TProp>
    where TEntity : class
{
    private static string GetBoundPropertyName( Expression<Func<TEntity, TProp?>> propExpr )
    {
        try
        {
            return propExpr.GetPropertyInfo().Name;
        }
        catch( Exception )
        {
            return "Unknown";
        }
    }

    private readonly Func<TEntity, TProp?> _getter = propExpr.Compile();

    public ITableCreator<TEntity> TableCreator => (ITableCreator<TEntity>) Creator;

    public override int ColumnsNeeded => 1;
    public List<Aggregator<TEntity, TProp>> Aggregators { get; } = [];

    public override bool PopulateSheet( IWorkbook workbook, int startingRow, int startingCol )
    {
        if( TableCreator.Sheet == null )
        {
            Logger?.UndefinedSheet();
            return false;
        }

        for( var row = 0; row < TableCreator.Data.Count; row++ )
        {
            CreateCell( workbook, row + startingRow, startingCol, _getter( TableCreator.Data[ row ] ) );
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
