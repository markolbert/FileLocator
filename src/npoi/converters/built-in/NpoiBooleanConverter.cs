using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public class NpoiBooleanConverter( ILoggerFactory? loggerFactory ) : NpoiConverter<bool>( loggerFactory )
{
    public override bool Convert( ICell cell ) => cell.BooleanCellValue;
}
