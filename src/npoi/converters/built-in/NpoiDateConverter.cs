using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public class NpoiDateConverter(ILoggerFactory? loggerFactory) : NpoiConverter<DateTime>(loggerFactory)
{
    public override DateTime Convert(ICell cell) => cell.DateCellValue ?? DateTime.MinValue;
}
