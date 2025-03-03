using System.ComponentModel;
using System.Reflection;
using Microsoft.Extensions.Logging;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace J4JSoftware.FileUtilities;

public class WorksheetTableReader<TEntity, TContext> : IWorksheetTableReader<TEntity, TContext>
    where TEntity : class, new()
    where TContext : WorksheetImportContext
{
    private readonly List<IImportedColumn> _columns = [];

    public WorksheetTableReader(
        IEnumerable<INpoiConverter>? converters = null,
        ILoggerFactory? loggerFactory = null
    )
    {
        LoggerFactory = loggerFactory;
        Logger = LoggerFactory?.CreateLogger( GetType() );

        CreateColumnMappings(converters, loggerFactory);
    }

    private void CreateColumnMappings(IEnumerable<INpoiConverter>? converters, ILoggerFactory? loggerFactory )
    {
        var builtInConverters = new Dictionary<Type, INpoiConverter>();

        foreach (var builtIn in converters ?? [])
        {
            if (builtInConverters.TryGetValue(builtIn.TargetType, out var _))
            {
                Logger?.SkippedDuplicate(builtIn.TargetType.Name, "built-in INpoiConverter");
                continue;
            }

            builtInConverters.Add(builtIn.TargetType, builtIn);
        }

        var customConverters = new Dictionary<Type, INpoiConverter>();

        foreach (var mappingInfo in typeof(TEntity).GetProperties()
                                                     .Where(p => p is { CanRead: true, CanWrite: true }
                                                              && p.GetSetMethod() != null
                                                              && p.GetCustomAttribute<NpoiFieldAttribute>() != null)
                                                     .Select(p => new
                                                     {
                                                         PropertyInfo = p,
                                                         NpoiAttribute =
                                                              p.GetCustomAttribute<NpoiFieldAttribute>()!
                                                     }))
        {
            var converterType = mappingInfo.NpoiAttribute.ConverterType;

            if (converterType == null)
            {
                // see if the property type is one of the built-in converters
                if (builtInConverters.TryGetValue(mappingInfo.PropertyInfo.PropertyType, out var temp))
                    _columns.Add(new ImportedColumn<TEntity>(mappingInfo.NpoiAttribute.NpoiFieldName,
                                                                 mappingInfo.PropertyInfo,
                                                                 temp,
                                                                 loggerFactory));
                else Logger?.UndefinedNpoiConverter(mappingInfo.PropertyInfo.PropertyType.Name);

                continue;
            }

            // see if the converter type is already created
            if (customConverters.TryGetValue(converterType, out var temp2))
            {
                _columns.Add(new ImportedColumn<TEntity>(mappingInfo.NpoiAttribute.NpoiFieldName,
                                                             mappingInfo.PropertyInfo,
                                                             temp2,
                                                             loggerFactory));
                continue;
            }

            // make sure the converter type actually is an INpoiConverter
            if (converterType.GetInterface(nameof(INpoiConverter)) == null)
            {
                Logger?.UndefinedNpoiConverter(mappingInfo.PropertyInfo.PropertyType.Name);
                continue;
            }

            try
            {
                var converter = (INpoiConverter)Activator.CreateInstance(converterType)!;
                customConverters.Add(converterType, converter);

                _columns.Add(new ImportedColumn<TEntity>(mappingInfo.NpoiAttribute.NpoiFieldName,
                                                           mappingInfo.PropertyInfo,
                                                           converter,
                                                           loggerFactory));
            }
            catch (Exception ex)
            {
                Logger?.NpoiConverterNotCreatable(converterType.Name, ex.Message);
            }
        }
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

            foreach (var column in _columns.Where(c=>c.ColumnNumber>0  ))
            {
                var cell = row.GetCell(column.ColumnNumber);
                if (cell == null)
                    continue;

                if (column.SetValue(sheet, entity, cell))
                    continue;

                Logger?.FailedToSetCellValue(column.ColumnNumber, rowNum);
            }

            if ( !EntityAdjuster?.AdjustEntity( entity ) ?? false )
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

        if( string.IsNullOrWhiteSpace( context.SheetName ) )
        {
            Logger?.NoSheetName();
            return false;
        }

        IWorkbook workbook;

        try
        {
            workbook = context.WorksheetType switch
            {
                WorksheetType.Xlsx => new XSSFWorkbook( context.ImportStream ),
                WorksheetType.Xls => new HSSFWorkbook( context.ImportStream ),
                _ => throw new InvalidEnumArgumentException(
                    $"Unsupported {nameof( WorksheetType )} value '{context.WorksheetType.ToString()}'" )
            };
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
             && ValidateColumns( sheet )
             && Initialize();
        }
        catch( Exception ex )
        {
            Logger?.MissingSheetWithMessage( context.SheetName, ex.Message );
            return false;
        }
    }

    private bool ValidateColumns( ISheet sheet )
    {
        var headerRow = sheet.GetRow( 0 );

        if( headerRow == null )
        {
            Logger?.MissingSheetHeaderRow( sheet.SheetName );
            return false;
        }

        var npoiFields = new List<string>();

        for( var colNum = 0; colNum < headerRow.LastCellNum; colNum++ )
        {
            var cell = headerRow.GetCell(colNum);

            if (cell == null )
                continue;

            npoiFields.Add( cell.StringCellValue );
        }

        var retVal = true;

        // check for duplicate fields in the NPOI table
        foreach( var dupeField in npoiFields
                                .GroupBy( n => n, n => n, ( n, e ) => new { NpoiField = n, Count = e.Count() } )
                                .Where( x => x.Count > 1 ) )
        {
            Logger?.DuplicateNpoiField( dupeField.NpoiField );
            retVal = false;
        }

        // update mappings
        if( retVal )
        {
            for( var colIdx = 0; colIdx < npoiFields.Count; colIdx++ )
            {
                var mappedCol = _columns.FirstOrDefault(
                    c => c.ColumnNameInSheet.Equals(npoiFields[colIdx], StringComparison.OrdinalIgnoreCase ) );

                if( mappedCol != null )
                    mappedCol.ColumnNumber = colIdx;
            }
        }

        foreach( var column in _columns )
        {
            if( column.ColumnNumber >= 0 )
                continue;

            Logger?.UnmappedNpoiField( column.ColumnNameInSheet );
            retVal = false;
        }

        return retVal;
    }

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
