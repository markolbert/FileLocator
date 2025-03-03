using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public class NpoiInt32Converter(ILoggerFactory? loggerFactory) : NpoiConverter<int>(loggerFactory)
{
    public override int Convert(ICell cell) => System.Convert.ToInt32(cell.NumericCellValue);
}
