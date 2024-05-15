using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Text;
using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace J4JSoftware.FileUtilities;

public class TableCreator<TEntity>(
    IEnumerable<TEntity> entities,
    IWorkbookCreator workbookCreator,
    StyleConfiguration styleConfig,
    ILoggerFactory? loggerFactory
)
    : ITableCreator<TEntity>, ITableCreatorInternal<TEntity>
    where TEntity : class
{
    private readonly ILogger? _logger = workbookCreator.LoggerFactory?.CreateLogger<TableCreator<TEntity>>();
    private readonly List<IExportableColumn> _columns = [];
    private readonly List<TitleRow> _titleRows = [];
    private readonly Dictionary<string, NamedRangeScope> _namedRanges = new( StringComparer.OrdinalIgnoreCase );

    private int _firstDataRow;

    public Type EntityType => typeof( TEntity );
    public IStyleSets StyleSets { get; } = new StyleSets( styleConfig, loggerFactory );
    public IWorkbookCreator WorkbookCreator { get; } = workbookCreator;
    public ILoggerFactory? LoggerFactory { get; } = workbookCreator.LoggerFactory;

    public ISheet? Sheet { get; private set; }
    public string SheetName { get; set; } = string.Empty;
    public ReadOnlyCollection<IExportableColumn> Columns => _columns.AsReadOnly();

    public List<TEntity> Data { get; } = entities.ToList();
    public int NumDataRows => Data.Count;
    public int NumColumnsDefined => _columns.Count;

    public int FreezeColumn { get; private set; }

    #region internal interface methods

    #region internal column adders

    IExportableColumn<TEntity, string?> ITableCreatorInternal<TEntity>.AddColumn(
        Expression<Func<TEntity, string?>> propExpr
    )
    {
        var retVal = new ExportableColumn<TEntity, string?>( this,
                                                            propExpr,
                                                            StyleSets.DefaultBase,
                                                            WorkbookCreator.LoggerFactory );

        _columns.Add( retVal );

        return retVal;
    }

    IExportableColumn<TEntity, int> ITableCreatorInternal<TEntity>.AddColumn(
        Expression<Func<TEntity, int>> propExpr
    )
    {
        var retVal = new ExportableColumn<TEntity, int>(this,
                                                            propExpr,
                                                            StyleSets.DefaultInteger,
                                                            WorkbookCreator.LoggerFactory);

        _columns.Add(retVal);

        return retVal;
    }

    IExportableColumn<TEntity, double> ITableCreatorInternal<TEntity>.AddColumn(
        Expression<Func<TEntity, double>> propExpr
    )
    {
        var retVal = new ExportableColumn<TEntity, double>(this,
                                                        propExpr,
                                                        StyleSets.DefaultDouble,
                                                        WorkbookCreator.LoggerFactory);

        _columns.Add(retVal);

        return retVal;
    }

    IExportableColumn<TEntity, DateTime?> ITableCreatorInternal<TEntity>.AddColumn(
        Expression<Func<TEntity, DateTime?>> propExpr
    )
    {
        var retVal = new ExportableColumn<TEntity, DateTime?>(this,
                                                            propExpr,
                                                            StyleSets.DefaultDate,
                                                            WorkbookCreator.LoggerFactory);

        _columns.Add(retVal);

        return retVal;
    }

    IExportableColumn<TEntity, bool> ITableCreatorInternal<TEntity>.AddColumn(
        Expression<Func<TEntity, bool>> propExpr
    )
    {
        var retVal = new ExportableColumn<TEntity, bool>(this,
                                                           propExpr,
                                                           StyleSets.DefaultBoolean,
                                                           WorkbookCreator.LoggerFactory);

        _columns.Add(retVal);

        return retVal;
    }

    #endregion

    #region internal vector adders

    IExportableColumn<TEntity, double> ITableCreatorInternal<TEntity>.AddVector(
        Expression<Func<TEntity, List<double>>> propExpr
    )
    {
        var retVal = new ExportableVector<TEntity, double>(this,
                                                                 propExpr,
                                                                 StyleSets.DefaultDouble,
                                                                 WorkbookCreator.LoggerFactory);

        _columns.Add(retVal);

        return retVal;
    }

    IExportableColumn<TEntity, int> ITableCreatorInternal<TEntity>.AddVector(
        Expression<Func<TEntity, List<int>>> propExpr
    )
    {
        var retVal = new ExportableVector<TEntity, int>(this,
                                                           propExpr,
                                                           StyleSets.DefaultInteger,
                                                           WorkbookCreator.LoggerFactory);

        _columns.Add(retVal);

        return retVal;
    }

    IExportableColumn<TEntity, string> ITableCreatorInternal<TEntity>.AddVector(
        Expression<Func<TEntity, List<string>>> propExpr
    )
    {
        var retVal = new ExportableVector<TEntity, string>(this,
                                                           propExpr,
                                                           StyleSets.DefaultBase,
                                                           WorkbookCreator.LoggerFactory);

        _columns.Add(retVal);

        return retVal;
    }

    #endregion

    void ITableCreatorInternal.SetFreezeColumn( int colNum ) => FreezeColumn = colNum;
    void ITableCreatorInternal.AddTitle( string text, StyleSetBase styleSet ) => _titleRows.Add( new TitleRow( text, this, styleSet ) );

    void ITableCreatorInternal.AddNamedRangeScope( string rangeName, NamedRangeScope namedRange )
    {
        if( _namedRanges.TryGetValue( rangeName, out _ ) )
            _namedRanges[ rangeName ] = namedRange;
        else _namedRanges.Add( rangeName, namedRange );
    }

#endregion

    public void Export( IWorkbook workbook, bool workbookIsNew )
    {
        Sheet = workbookIsNew ? workbook.CreateSheet( SheetName ) : workbook.ClearSheet( SheetName );

        // if any columns are set to wrap text, all columns should wrap text and set vertical
        // alignment to top
        if( _columns.Any( c => c.StyleSet.WrapText ) )
        {
            foreach( var column in _columns )
            {
                column.StyleSet = column.StyleSet with { WrapText = true, VerticalAlignment = VerticalAlignment.Top };
            }
        }

        BuildWorksheet( workbook );

        if( workbookIsNew )
            CreateNamedRanges();
        else UpdateNamedRanges( workbook );
    }

    private void BuildWorksheet( IWorkbook workbook )
    {
        // should never happen, but...
        if( Sheet == null )
            return;

        var numTitles = _titleRows.ExportTitlesToSheet( workbook );

        _firstDataRow = AddHeaders( workbook, numTitles );
        FreezeColumn = FreezeColumn < 0 ? 0 : FreezeColumn;

        // leave room for title lines (we add them at the end so that
        // we can easily size columns)
        Sheet.CreateFreezePane( FreezeColumn, _firstDataRow );

        var startingCol = 0;

        foreach( var column in _columns )
        {
            column.PopulateSheet( workbook, _firstDataRow, startingCol );
            startingCol += column.ColumnsNeeded;
        }

        // have to evaluate the sheet so that formula values get set
        // and autosizing works as expected
        var evaluator = new XSSFFormulaEvaluator( workbook );
        evaluator.EvaluateAll();

        SizeColumns();
    }

    private int AddHeaders( IWorkbook workbook, int numTitleRows )
    {
        // shouldn't happen, but...
        if( Sheet == null )
            return 0;

        if( _columns.All( c => c.HeaderCreators.Count == 0 ) )
            return numTitleRows;

        var numHeaderRows = _columns.Max( c => c.HeaderCreators.Count != 0
                                         ? c.HeaderCreators.Max( hc => hc.NumRows )
                                         : 0 )
          + numTitleRows;

        var startCol = 0;

        foreach( var column in _columns )
        {
            if( column.HeaderCreators.Count == 0 )
            {
                startCol += column.ColumnsNeeded;
                continue;
            }

            var colHeaderRow = numHeaderRows - column.HeaderCreators.Sum( hc => hc.NumRows );

            foreach( var creator in column.HeaderCreators )
            {
                creator.PopulateSheet(workbook, colHeaderRow, startCol);
                colHeaderRow += creator.NumRows;
            }

            startCol += column.ColumnsNeeded;
        }

        return numHeaderRows;
    }

    private void CreateNamedRanges()
    {
        // should never happen, but...
        if( Sheet == null )
            return;

        // create the sheet-scoped ranges first
        foreach( var kvp in _namedRanges.Where( nr => !nr.Value.WorkbookScoped ) )
        {
            var nr = Sheet.Workbook.CreateName();
            nr.NameName = kvp.Key;
            nr.SheetIndex = Sheet.Workbook.GetSheetIndex( Sheet );
            nr.RefersToFormula = CreateRangeFormula( kvp.Value );
        }

        // create the workbook-scoped ranges next, avoiding duplicate names
        var existingNames = Sheet.Workbook.GetAllNames().Select( x => x.NameName ).ToList();

        foreach( var kvp in _namedRanges.Where( nr => nr.Value.WorkbookScoped ) )
        {
            if( existingNames.Any( x => x.Equals( kvp.Key, StringComparison.OrdinalIgnoreCase ) ) )
                continue;

            var nr = Sheet.Workbook.CreateName();
            nr.NameName = kvp.Key;
            nr.RefersToFormula = CreateRangeFormula( kvp.Value );
        }
    }

    private void UpdateNamedRanges( IWorkbook workbook )
    {
        // should never happen, but...
        if( Sheet == null )
            return;

        foreach( var kvp in _namedRanges )
        {
            var nr = workbook.GetName( kvp.Key );

            // skip missing names (which can happen if we change the name of a range)
            if( nr == null )
            {
                _logger?.DuplicateNamedRange( kvp.Key );
                continue;
            }

            nr.RefersToFormula = CreateRangeFormula( kvp.Value );
        }
    }

    private string? CreateRangeFormula( NamedRangeScope nrScope )
    {
        // don't create invalid ranges
        if( nrScope.Columns.Min() < 0 || nrScope.Columns.Max() >= _columns.Count )
            return null;

        var sortedColumns = nrScope.Columns.Where( x => x <= 16384 ).OrderBy( x => x ).ToList();
        var sb = new StringBuilder();
        var prevCol = -2;
        string? colText = null;
        var lastCol = sortedColumns.Max( x => x );

        foreach( var col in sortedColumns )
        {
            if( col > prevCol + 1 || col == lastCol )
            {
                // discontinuous range or last column
                prevCol = col;

                if( !string.IsNullOrEmpty( colText ) )
                    sb.Append( $"${colText}${Data.Count + 1}" );

                if( sb.Length > 0 )
                    sb.Append( ';' );

                colText = NpoiExtensions.ConvertColumnNumberToText( col );
                sb.Append( $"{SheetName}!${colText}${_firstDataRow + 1}" );
            }
        }

        sb.Append( $":${colText}${Data.Count + _firstDataRow}" );

        return sb.ToString();
    }

    private void SizeColumns()
    {
        // should never happen, but...
        if( Sheet == null )
            return;

        var startingCol = 0;

        foreach( var column in _columns )
        {
            if( column.StyleSet.AutoSize )
            {
                AutosizeColumns(column, startingCol);
                startingCol += column.ColumnsNeeded;

                continue;
            }

            if( column.StyleSet.MaxWidth > 0 )
                SetColumnWidths(column, startingCol);

            startingCol += column.ColumnsNeeded;
        }
    }

    private void AutosizeColumns( IExportableColumn column, int startingCol )
    {
        // should never happen, but...
        if (Sheet == null)
            return;

        for (var colIdx = 0; colIdx < column.ColumnsNeeded; colIdx++)
        {
            Sheet.AutoSizeColumn(startingCol + colIdx);

            // set max width if needed
            var colWidth = Sheet.GetColumnWidth(startingCol + colIdx);

            if (column.StyleSet is { MaxWidth: > 0 } && colWidth > column.StyleSet.MaxWidth * 256)
                Sheet.SetColumnWidth(startingCol + colIdx, column.StyleSet.MaxWidth * 256);
        }
    }

    private void SetColumnWidths( IExportableColumn column, int startingCol )
    {
        // should never happen, but...
        if (Sheet == null)
            return;

        for (var colIdx = 0; colIdx < column.ColumnsNeeded; colIdx++)
        {
            Sheet.SetColumnWidth( startingCol + colIdx, column.StyleSet.MaxWidth * 256 );
        }
    }

    List<object> ITableCreator.GetData() => Data.Cast<object>().ToList();
}
