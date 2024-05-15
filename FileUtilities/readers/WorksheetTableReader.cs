using System.Collections;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace J4JSoftware.FileUtilities;

public class WorksheetTableReader<TEntity> : IWorksheetTableReader<TEntity>
    where TEntity : class, new()
{
    private readonly Dictionary<int, IImportedColumn> _columns = [];
    private readonly IRecordFilter<TEntity>? _filter;
    private readonly IKeyedEntityUpdater<TEntity>? _entityUpdater;

    private IWorkbookFileInfo? _source;

    public WorksheetTableReader(
        IRecordFilter<TEntity>? filter = null,
        IKeyedEntityUpdater<TEntity>? entityUpdater = null,
        ILoggerFactory? loggerFactory = null
    )
    {
        LoggerFactory = loggerFactory;
        Logger = LoggerFactory?.CreateLogger( GetType() );

        _filter = filter;
        _entityUpdater = entityUpdater;
    }

    protected ILoggerFactory? LoggerFactory { get; }
    protected ILogger? Logger { get; }

    public Type ImportedType => typeof( TEntity );

    public IWorkbookFileInfo? Source
    {
        get => _source;

        set
        {
            _source = value;

            if( _entityUpdater != null )
                _entityUpdater.Source = _source;
        }
    }

    public IEnumerator<TEntity> GetEnumerator()
    {
        if (!InitializeInternal(out var sheet))
            yield break;

        for (var rowNum = 1; rowNum <= sheet!.LastRowNum; rowNum++)
        {
            var row = sheet.GetRow(rowNum);
            var entity = new TEntity();

            foreach (var kvp in _columns)
            {
                var cell = row.GetCell(kvp.Key);
                if (cell == null)
                    continue;

                if (kvp.Value.SetValue(sheet, entity, cell))
                    continue;

                Logger?.FailedToSetCellValue(kvp.Key, rowNum);
            }

            _entityUpdater?.ProcessEntityFields(entity);

            if( _filter == null || _filter.Include( entity ) )
                yield return entity;
        }

        CompleteImport();
    }

    private bool InitializeInternal( out ISheet? sheet )
    {
        sheet = null;

        if( Source == null )
        {
            Logger?.UndefinedImportSource();
            return false;
        }

        if( !File.Exists( Source.FilePath ) )
        {
            Logger?.FileNotFound( Source.FilePath );
            return false;
        }

        var isXlsx = Path.GetExtension( Source.FilePath ).Equals( ".xlsx", StringComparison.OrdinalIgnoreCase );

        IWorkbook workbook;

        try
        {
            using var fs = new FileStream( Source.FilePath, FileMode.Open, FileAccess.Read );
            workbook = isXlsx ? new XSSFWorkbook( fs ) : new HSSFWorkbook( fs );
        }
        catch( Exception ex )
        {
            Logger?.FileUnreadable( Source.FilePath, ex.Message );
            return true;
        }

        try
        {
            sheet = workbook.GetSheet( Source.SheetName );

            if( _entityUpdater != null )
                _entityUpdater.Source = Source;

            return sheet != null && ValidateColumns( sheet ) && ( _entityUpdater?.Initialize() ?? true ) && Initialize();
        }
        catch( Exception ex )
        {
            Logger?.MissingSheetWithMessage( Source.SheetName, ex.Message );
            return false;
        }
    }

    private bool ValidateColumns( ISheet sheet )
    {
        var headerRow = sheet.GetRow( 0 );

        if( headerRow == null )
        {
            Logger?.MissingSheetHeaderRow(sheet.SheetName);
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
                Logger?.BadHeaderName( Source!.SheetName, colNum, column.ColumnNameInSheet, cell.StringCellValue );
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
        _entityUpdater?.UpdateRecorder.SaveChanges();
    }

    public void Dispose()
    {
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    bool ITableReader.SetSource(object source)
    {
        if (source is IWorkbookFileInfo castSource)
        {
            Source = castSource;
            return true;
        }

        Logger?.UnexpectedType(typeof(IFileContext), source.GetType());
        return false;
    }
}
