using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public static class NpoiExportExtensions
{
    private static ITableCreatorInternal<TEntity> CastToInternal<TEntity>( this ITableCreator<TEntity> instance )
        where TEntity : class
    {
        if( instance is ITableCreatorInternal<TEntity> retVal )
            return retVal;

        throw new FileUtilityException( typeof( NpoiExportExtensions ),
                                    nameof( CastToInternal ),
                                    $"{nameof( instance )} is not an instance of {typeof( ITableCreatorInternal )}" );
    }

    public static ITableCreator<TEntity> SheetName<TEntity>( this ITableCreator<TEntity> tableCreator, string name )
        where TEntity : class
    {
        tableCreator.SheetName = name;

        return tableCreator;
    }

    public static IExportableColumn<TEntity, TProp> Style<TEntity, TProp>(
        this IExportableColumn<TEntity, TProp> column,
        StyleSetBase styleSet
    )
        where TEntity : class
    {
        column.StyleSet = styleSet;
        return column;
    }
    
    #region properties

    public static IExportableColumn<TEntity, string?> AddColumn<TEntity>(
        this ITableCreator<TEntity> tableCreator,
        Expression<Func<TEntity, string?>> propExpr
    )
        where TEntity : class
    {
        return tableCreator.CastToInternal().AddColumn( propExpr );
    }

    public static IExportableColumn<TEntity, int> AddColumn<TEntity>(
        this ITableCreator<TEntity> tableCreator,
        Expression<Func<TEntity, int>> propExpr
    )
        where TEntity : class
    {
        return tableCreator.CastToInternal().AddColumn(propExpr);
    }

    public static IExportableColumn<TEntity, double> AddColumn<TEntity>(
        this ITableCreator<TEntity> tableCreator,
        Expression<Func<TEntity, double>> propExpr
    )
        where TEntity : class
    {
        return tableCreator.CastToInternal().AddColumn(propExpr);
    }

    public static IExportableColumn<TEntity, DateTime?> AddColumn<TEntity>(
        this ITableCreator<TEntity> tableCreator,
        Expression<Func<TEntity, DateTime?>> propExpr
    )
        where TEntity : class
    {
        return tableCreator.CastToInternal().AddColumn(propExpr);
    }

    public static IExportableColumn<TEntity, DateTime> AddColumn<TEntity>(
        this ITableCreator<TEntity> tableCreator,
        Expression<Func<TEntity, DateTime>> propExpr
    )
        where TEntity : class
    {
        return tableCreator.CastToInternal().AddColumn(propExpr);
    }

    public static IExportableColumn<TEntity, bool> AddColumn<TEntity>(
        this ITableCreator<TEntity> tableCreator,
        Expression<Func<TEntity, bool>> propExpr
    )
        where TEntity : class
    {
        return tableCreator.CastToInternal().AddColumn(propExpr);
    }

    #endregion

    #region vector properties

    public static IExportableColumn<TEntity, double> AddVector<TEntity>(
        this ITableCreator<TEntity> tableCreator,
        Expression<Func<TEntity, List<double>>> propExpr
    )
        where TEntity : class
    {
        return tableCreator.CastToInternal().AddVector( propExpr );
    }

    public static IExportableColumn<TEntity, int> AddVector<TEntity>(
        this ITableCreator<TEntity> tableCreator,
        Expression<Func<TEntity, List<int>>> propExpr
    )
        where TEntity : class
    {
        return tableCreator.CastToInternal().AddVector(propExpr);
    }

    public static IExportableColumn<TEntity, string> AddVector<TEntity>(
        this ITableCreator<TEntity> tableCreator,
        Expression<Func<TEntity, List<string>>> propExpr
    )
        where TEntity : class
    {
        return tableCreator.CastToInternal().AddVector(propExpr);
    }

    #endregion

    #region headers

    public static IExportableColumn<TEntity, TProp> PropertyHeader<TEntity, TProp>(
        this IExportableColumn<TEntity, TProp> exportable,
        string propertyName = "Unknown"
    )
        where TEntity : class
    {
        exportable.HeaderCreators.Add( new HeaderFromProperty( exportable,
                                                               exportable.TableCreator.StyleSets.DefaultHeader,
                                                               boundName: propertyName ) );

        return exportable;
    }

    public static IExportableColumn<TEntity, TProp> TextHeader<TEntity, TProp>(
        this IExportableColumn<TEntity, TProp> exportable,
        string text
    )
        where TEntity : class
    {
        exportable.HeaderCreators.Add( new HeaderFromText( text,
                                                           exportable.TableCreator,
                                                           exportable.TableCreator.StyleSets.DefaultHeader ) );
        return exportable;
    }

    public static IExportableColumn<TEntity, TProp> Header<TEntity, TProp>(
        this IExportableColumn<TEntity, TProp> exportable,
        IHeaderCreator headerCreator
    )
        where TEntity : class
    {
        exportable.HeaderCreators.Add( headerCreator );
        return exportable;
    }

    public static IExportableColumn<TEntity, TProp> YearsHeader<TEntity, TProp>(
        this IExportableColumn<TEntity, TProp> exportable,
        int firstYear,
        int lastYear
    )
        where TEntity : class
    {
        if( firstYear > lastYear )
            ( firstYear, lastYear ) = ( lastYear, firstYear );

        exportable.HeaderCreators.Add( new YearsHeaderCreator( firstYear,
                                                               lastYear,
                                                               exportable.TableCreator,
                                                               exportable.TableCreator.StyleSets
                                                                         .DefaultUngroupedIntegerHeader ) );

        return exportable;
    }

    public static IExportableColumn<TEntity, TProp> CountHeader<TEntity, TProp>(
        this IExportableColumn<TEntity, TProp> exportable,
        int firstCol,
        int lastCol
    )
        where TEntity : class
    {
        if (firstCol > lastCol)
            (firstCol, lastCol) = (lastCol, firstCol);

        exportable.HeaderCreators.Add( new CountHeaderCreator( firstCol,
                                                               lastCol,
                                                               exportable.TableCreator,
                                                               exportable.TableCreator.StyleSets
                                                                         .DefaultUngroupedIntegerHeader ) );

        return exportable;
    }

    public static IExportableColumn<TEntity, TProp> Header<TEntity, TProp>(
        this IExportableColumn<TEntity, TProp> exportable,
        VectorHeaderCreator headerCreator
    )
        where TEntity : class
    {
        exportable.HeaderCreators.Add( headerCreator );
        return exportable;
    }

    public static IExportableColumn<TEntity, TProp> SpanningHeader<TEntity, TProp>(
        this IExportableColumn<TEntity, TProp> exportable,
        string text,
        int columnsToSpan
    )
        where TEntity : class
    {
        columnsToSpan = columnsToSpan <= 0 ? 1 : columnsToSpan;

        exportable.HeaderCreators.Add( new SpanningHeader( text,
                                                           columnsToSpan,
                                                           exportable.TableCreator,
                                                           exportable.TableCreator.StyleSets.DefaultHeader ) );

        return exportable;
    }

    #endregion

    #region named ranges

    public static ITableCreator<TEntity> AddWorkbookNamedRange<TEntity>(
        this ITableCreator<TEntity> tableCreator,
        string rangeName,
        ILogger? logger = null,
        params int[] columns
    )
        where TEntity : class
    {
        if( string.IsNullOrEmpty( rangeName ) )
        {
            logger?.MissingRangeName();
            return tableCreator;
        }

        if( columns.Length == 0 && tableCreator.NumColumnsDefined >= 1 )
            columns = [tableCreator.NumColumnsDefined - 1];

        if( columns.Length == 0 )
        {
            logger?.UndefinedNamedRange( rangeName );
            return tableCreator;
        }

        tableCreator.CastToInternal().AddNamedRangeScope( rangeName, new NamedRangeScope( true, columns.ToList() ) );

        return tableCreator;
    }

    public static IExportableColumn<TEntity, TProp> AddWorkbookNamedRange<TEntity, TProp>(
        this IExportableColumn<TEntity, TProp> exportable,
        string rangeName,
        ILogger? logger = null,
        params int[] columns
    )
        where TEntity : class
    {
        exportable.TableCreator.AddWorkbookNamedRange( rangeName, logger, columns );

        return exportable;
    }

    public static ITableCreator<TEntity> AddWorksheetNamedRange<TEntity>(
        this ITableCreator<TEntity> tableCreator,
        string rangeName,
        ILogger? logger = null,
        params int[] columns
    )
        where TEntity : class
    {
        if( string.IsNullOrEmpty( rangeName ) )
        {
            logger?.MissingRangeName();
            return tableCreator;
        }

        if( columns.Length == 0 && tableCreator.NumColumnsDefined >= 1 )
            columns = [tableCreator.NumColumnsDefined - 1];

        if( columns.Length == 0 )
        {
            logger?.UndefinedNamedRange( rangeName );
            return tableCreator;
        }

        tableCreator.CastToInternal().AddNamedRangeScope( rangeName, new NamedRangeScope( false, columns.ToList() ) );

        return tableCreator;
    }

    public static IExportableColumn<TEntity, TProp> AddWorksheetNamedRange<TEntity, TProp>(
        this IExportableColumn<TEntity, TProp> exportable,
        string rangeName,
        ILogger? logger = null,
        params int[] columns
    )
        where TEntity : class
    {
        exportable.TableCreator.AddWorksheetNamedRange( rangeName, logger, columns );

        return exportable;
    }

    #endregion

    public static IExportableColumn<TEntity, TProp> AutoSize<TEntity, TProp>(
        this IExportableColumn<TEntity, TProp> exportable,
        int maxWidth = 0,
        bool autoSize = true,
        ILogger? logger = null
    )
        where TEntity : class
    {
        if( !autoSize && maxWidth > 0 )
            logger?.SuperfluousMaxColumnWidth();

        exportable.StyleSet = exportable.StyleSet with { AutoSize = autoSize, MaxWidth = maxWidth};

        return exportable;
    }

    public static IExportableColumn<TEntity, TProp> WrapText<TEntity, TProp>(
        this IExportableColumn<TEntity, TProp> exportable,
        int maxWidth,
        ILogger? logger = null
    )
        where TEntity : class
    {
        if( maxWidth <= 0 )
            logger?.SuperfluousMaxColumnWidth();
        else exportable.StyleSet = exportable.StyleSet with { MaxWidth = maxWidth, WrapText = true};

        return exportable;
    }

    public static IExportableColumn<TEntity, TProp> Aggregator<TEntity, TProp>(
        this IExportableColumn<TEntity, TProp> exportable,
        AggregateFunction aggFunc,
        StyleSetBase? labelStyle = null
    )
        where TEntity : class
    {
        exportable.Aggregators.Add( new Aggregator<TEntity, TProp>( aggFunc, exportable, labelStyle ) );
        return exportable;
    }

    #region freeze column

    public static ITableCreator<TEntity> FreezeColumn<TEntity>(
        this ITableCreator<TEntity> tableCreator,
        int freezeColumn
    )
        where TEntity : class
    {
        tableCreator.CastToInternal().SetFreezeColumn( freezeColumn < 0 ? 0 : freezeColumn );
        return tableCreator;
    }

    public static IExportableColumn<TEntity, TProp> FreezeColumn<TEntity, TProp>(
        this IExportableColumn<TEntity, TProp> exportable,
        int freezeColumn
    )
        where TEntity : class
    {
        exportable.TableCreator.CastToInternal().SetFreezeColumn( freezeColumn < 0 ? 0 : freezeColumn );
        return exportable;
    }

    #endregion

    #region add title row

    public static ITableCreator<TEntity> AddTitleRow<TEntity>(
        this ITableCreator<TEntity> tableCreator,
        string text
    )
        where TEntity : class
    {
        tableCreator.CastToInternal().AddTitle( text, tableCreator.StyleSets.DefaultTitle );
        return tableCreator;
    }

    public static IExportableColumn<TEntity, TProp> AddTitleRow<TEntity, TProp>(
        this IExportableColumn<TEntity, TProp> exportable,
        string text
    )
        where TEntity : class
    {
        exportable.TableCreator.CastToInternal().AddTitle( text, exportable.TableCreator.StyleSets.DefaultTitle );
        return exportable;
    }

    #endregion

    public static IWorkbookCreator SheetOrder( this IWorkbookCreator workbookCreator, params ISheetCreator[] sheets )
    {
        if( workbookCreator is not IWorkbookCreatorInternal internalCreator )
            return workbookCreator;

        internalCreator.SheetSequence = sheets.Select( s => s.SheetName ).ToArray();
        return workbookCreator;
    }

    public static IWorkbookCreator SheetOrder(this IWorkbookCreator workbookCreator, params string[] sheetNames)
    {
        if (workbookCreator is not IWorkbookCreatorInternal internalCreator)
            return workbookCreator;

        internalCreator.SheetSequence = sheetNames;
        return workbookCreator;
    }

}
