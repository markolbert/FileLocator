using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace J4JSoftware.FileUtilities;

internal class SheetCollection() : KeyedCollection<string, ISheetCreator>( StringComparer.OrdinalIgnoreCase )
{
    protected override string GetKeyForItem( ISheetCreator item ) => item.SheetName;
}

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

    void IWorkbookCreatorInternal.ChangeSheetName( string oldName, string newName )
    {
        if( oldName.Equals( newName, StringComparison.OrdinalIgnoreCase ) )
            return;

        // avoid name collisions
        if( _sheetCreators.TryGetValue( newName, out _ ) )
        {
            _logger?.DuplicateSheetName( newName );
            return;
        }

        if( !_sheetCreators.TryGetValue( oldName, out var sheetCreator ) )
        {
            _logger?.UndefinedSheetName( oldName );
            return;
        }

        sheetCreator.SheetName = newName;

        _sheetCreators.Remove( oldName );
        _sheetCreators.Add( sheetCreator );
    }

    public ILoggerFactory? LoggerFactory { get; } = loggerFactory;

    public ITableCreator<TEntity> AddTableSheet<TEntity>( IEnumerable<TEntity> entities )
        where TEntity : class
    {
        var retVal = new TableCreator<TEntity>( entities, this, styleConfig, LoggerFactory );

        if( string.IsNullOrEmpty( retVal.SheetName ) )
            retVal.SheetName = $"Sheet{_sheetCreators.Count + 1}";

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

    public bool Export( string filePath, bool forceRecreation = false )
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

        //using var writeStream =
        //    new FileStream(filePath, createFile ? FileMode.Create : FileMode.Open, FileAccess.Write);

        //workbook.Write(writeStream);

        //using( var writeStream =
        //      new FileStream( filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None ) )
        //{
        //    writeStream.SetLength( 0 );
        //    workbook.Write( writeStream );
        //    workbook.Close();
        //}

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
