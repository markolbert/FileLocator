using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace J4JSoftware.FileUtilities;

public class WorkbookCreator(
    StyleConfiguration styleConfig,
    ILoggerFactory? loggerFactory
)
    : IWorkbookCreator, IWorkbookCreatorInternal
{
    private readonly ILogger? _logger = loggerFactory?.CreateLogger<WorkbookCreator>();
    private readonly SheetCollection _sheetCreators = [];

    private IWorkbook? _workbook;
    private string[]? _sheetSequence;

    string[]? IWorkbookCreatorInternal.SheetSequence
    {
        get => _sheetSequence;
        set => _sheetSequence = value;
    }

    SheetCollection IWorkbookCreatorInternal.SheetCreators => _sheetCreators;

    public ILoggerFactory? LoggerFactory { get; } = loggerFactory;

    public ITableCreator<TEntity> AddTableSheet<TEntity>( IEnumerable<TEntity> entities )
        where TEntity : class
    {
        var retVal = new TableCreator<TEntity>( entities, this, styleConfig, LoggerFactory );
        _sheetCreators.Add( retVal );

        return retVal;
    }

    public ISheetCreator AddSheet( ISheetCreator sheet )
    {
        if( string.IsNullOrEmpty( sheet.SheetName ) )
            sheet.SheetName = $"Sheet{_sheetCreators.Count + 1}";

        if( _sheetCreators.TryGetValue( sheet.SheetName, out _ ) )
            _sheetCreators.Remove( sheet.SheetName );
        
        _sheetCreators.Add( sheet );

        return sheet; 
    }

    public bool Export( string filePath, bool forceRecreation = true )
    {
        _workbook = null;
        var createFile = true;

        if( File.Exists( filePath ) && !forceRecreation )
        {
            createFile = false;
            FileStream? readStream = null;

            try
            {
                readStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

                _workbook = new XSSFWorkbook(readStream);
                readStream.Close();
                readStream.Dispose();
            }
            catch( Exception ex )
            {
                _logger?.WorkbookFileUnreadable( filePath, ex.Message );

                _workbook?.Close();
                _workbook = null;

                readStream?.Dispose();

                File.Delete( filePath );
                createFile = true;
            }
        }

        _workbook ??= new XSSFWorkbook();

        foreach( var sheetCreator in _sheetCreators )
        {
            sheetCreator.Export( _workbook, createFile );
        }

        // we don't re-order sheets in existing files to
        // preserve changes that may have been made
        if( createFile )
        {
            var sheetNames = new List<string>();

            foreach( var sheet in _sheetSequence ?? Enumerable.Empty<string>() )
            {
                if( _sheetCreators.TryGetValue( sheet, out var toExport ) )
                    sheetNames.Add( toExport.SheetName );
            }

            if( sheetNames.Count == _sheetCreators.Count )
            {
                for( var idx = 0; idx < sheetNames.Count; idx++ )
                {
                    _workbook.SetSheetOrder( sheetNames[ idx ], idx );
                }
            }
        }

        // write file from MemoryStream per suggestion from support team
        // to avoid file corruption when updating existing sheets
        using var writeStream = new MemoryStream();
        _workbook.Write(writeStream);
        File.WriteAllBytes(filePath, writeStream.ToArray());

        _workbook.Close();
        _workbook = null;

        _logger?.Success($"Wrote file '{filePath}'" );

        return true;
    }
}
