using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace J4JSoftware.FileUtilities;

public class WorksheetTableReader<TEntity, TContext> : IWorksheetTableReader<TEntity, TContext>
    where TEntity : class, new()
    where TContext : WorksheetImportContext
{
    private readonly Dictionary<int, IImportedColumn> _columns = [];

    public WorksheetTableReader(
        ILoggerFactory? loggerFactory = null
    )
    {
        LoggerFactory = loggerFactory;
        Logger = LoggerFactory?.CreateLogger( GetType() );
    }

    protected ILoggerFactory? LoggerFactory { get; }
    protected ILogger? Logger { get; }

    public Type ImportedType => typeof( TEntity );
    public IRecordFilter<TEntity>? Filter { get; set; }
    public IEntityAdjuster<TEntity>? EntityAdjuster { get; set; }

    public HashSet<int> GetReplacementIds() => EntityAdjuster?.GetReplacementIds() ?? [];

    public IEnumerable<TEntity> GetData( TContext context )
    {
        if( !InitializeInternal( context, out var sheet ) )
            yield break;

        for( var rowNum = 1; rowNum <= sheet!.LastRowNum; rowNum++ )
        {
            var row = sheet.GetRow( rowNum );
            var entity = new TEntity();

            foreach( var kvp in _columns )
            {
                var cell = row.GetCell( kvp.Key );
                if( cell == null )
                    continue;

                if( kvp.Value.SetValue( sheet, entity, cell ) )
                    continue;

                Logger?.FailedToSetCellValue( kvp.Key, rowNum );
            }

            if( !EntityAdjuster?.AdjustEntity( entity ) ?? false )
                yield break;

            if( Filter == null || Filter.Include( entity ) )
                yield return entity;
        }

        CompleteImport();
    }

    public IAsyncEnumerable<TEntity> GetDataAsync( TContext context, CancellationToken ctx ) =>
        GetData( context ).ToAsyncEnumerable();

    private bool InitializeInternal( WorksheetImportContext context, out ISheet? sheet )
    {
        sheet = null;

        if( context.ImportStream == null )
        {
            Logger?.UndefinedStream();
            return false;
        }

        IWorkbook workbook;

        try
        {
            workbook = new XSSFWorkbook( context.ImportStream );
        }
        catch( Exception ex )
        {
            Logger?.StreamUnreadable( ex.Message );
            return true;
        }

        try
        {
            sheet = workbook.GetSheet( context.SheetName );

            return sheet != null
             && ValidateColumns( context, sheet )
             && Initialize();
        }
        catch( Exception ex )
        {
            Logger?.MissingSheetWithMessage( context.SheetName, ex.Message );
            return false;
        }
    }

    private bool ValidateColumns( WorksheetImportContext context, ISheet sheet )
    {
        var headerRow = sheet.GetRow( 0 );

        if( headerRow == null )
        {
            Logger?.MissingSheetHeaderRow( sheet.SheetName );
            return false;
        }

        if( _columns.Count == 0 )
        {
            Logger?.NoColumnMappings( sheet.SheetName );
            return false;
        }

        var matchedColumns = 0;

        for( var colNum = 0; colNum < headerRow.LastCellNum; colNum++ )
        {
            var cell = headerRow.GetCell( colNum );

            if( cell == null || !_columns.TryGetValue( colNum, out var column ) )
                continue;

            if( column.ColumnNameInSheet.Equals( cell.StringCellValue, StringComparison.OrdinalIgnoreCase ) )
                matchedColumns++;
            else
            {
                Logger?.BadHeaderName( context.SheetName, colNum, column.ColumnNameInSheet, cell.StringCellValue );
                return false;
            }
        }

        if( matchedColumns == _columns.Count )
            return true;

        Logger?.HeaderCountMismatch( _columns.Count, matchedColumns );
        return false;
    }

    #region column mappings

    public void AddMapping(
        int colNum,
        string colNameInSheet,
        Expression<Func<TEntity, double>> propExpr
    ) =>
        AddMappingInternal( colNum,
                            new ImportedColumn<TEntity, double>( colNum, colNameInSheet, propExpr, LoggerFactory ) );

    public void AddMapping(
        int colNum,
        string colNameInSheet,
        Expression<Func<TEntity, int>> propExpr
    ) =>
        AddMappingInternal( colNum,
                            new ImportedColumn<TEntity, int>( colNum, colNameInSheet, propExpr, LoggerFactory ) );

    public void AddMapping(
        int colNum,
        string colNameInSheet,
        Expression<Func<TEntity, bool>> propExpr
    ) =>
        AddMappingInternal( colNum,
                            new ImportedColumn<TEntity, bool>( colNum, colNameInSheet, propExpr, LoggerFactory ) );

    public void AddMapping(
        int colNum,
        string colNameInSheet,
        Expression<Func<TEntity, DateTime>> propExpr
    ) =>
        AddMappingInternal( colNum,
                            new ImportedColumn<TEntity, DateTime>( colNum, colNameInSheet, propExpr, LoggerFactory ) );

    public void AddMapping(
        int colNum,
        string colNameInSheet,
        Expression<Func<TEntity, string?>> propExpr
    ) =>
        AddMappingInternal( colNum,
                            new ImportedColumn<TEntity, string?>( colNum, colNameInSheet, propExpr, LoggerFactory ) );

    private void AddMappingInternal(
        int colNum,
        IImportedColumn mapping
    )
    {
        if( colNum < 0 )
        {
            Logger?.InvalidPropertyValue( "column number", colNum );
            return;
        }

        if( _columns.TryGetValue( colNum, out _ ) )
        {
            Logger?.ReplacedDuplicate( "import column mapping for column", colNum.ToString() );
            _columns[ colNum ] = mapping;
        }
        else _columns.Add( colNum, mapping );
    }

    #endregion

    protected virtual bool Initialize() => true;

    protected virtual void CompleteImport()
    {
        // save whatever changes/updates were recorded
        EntityAdjuster?.SaveAdjustmentRecords();
    }

    public void Dispose()
    {
    }

    bool ITableReader.TryGetData(
        ImportContext context,
        out IEnumerable<object> data
    )
    {
        if( context is TContext castContext )
        {
            data = GetData( castContext );
            return true;
        }

        data = new List<object>();

        Logger?.InvalidTypeAssignment( context.GetType(), typeof( TContext ) );

        return false;
    }

    IAsyncEnumerable<object> ITableReader.GetObjectDataAsync( ImportContext context, CancellationToken ctx )
    {
        if( context is TContext castContext )
            return GetDataAsync( castContext, ctx );

        Logger?.UnexpectedType( typeof( TContext ), context.GetType() );
        return AsyncEnumerable.Empty<object>();
    }

    bool ITableReader.SetAdjuster( IEntityAdjuster? adjuster )
    {
        if( adjuster == null )
        {
            EntityAdjuster = null;
            return true;
        }

        if( adjuster is not IEntityAdjuster<TEntity> castAdjuster )
        {
            Logger?.InvalidTypeAssignment( adjuster.GetType(), typeof( IEntityAdjuster<TEntity> ) );
            return false;
        }

        EntityAdjuster = castAdjuster;
        return true;
    }

    bool ITableReader.SetFilter( IRecordFilter? filter )
    {
        if( filter == null )
        {
            Filter = null;
            return true;
        }

        if( filter is not IRecordFilter<TEntity> castFilter )
        {
            Logger?.InvalidTypeAssignment( filter.GetType(), typeof( IRecordFilter<TEntity> ) );
            return false;
        }

        Filter = castFilter;
        return true;
    }
}
