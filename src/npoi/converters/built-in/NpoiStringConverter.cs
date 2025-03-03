using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public class NpoiStringConverter(ILoggerFactory? loggerFactory) : NpoiConverter<string>(loggerFactory)
{
    public override string Convert(ICell cell)
    {
        //if( cell.CellType != CellType.String )
        //    cell.SetCellType( CellType.String );

        return cell.StringCellValue;
    }
}
