using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public class NpoiDoubleConverter(ILoggerFactory? loggerFactory) : NpoiConverter<double>(loggerFactory)
{
    public override double Convert(ICell cell) => cell.NumericCellValue;
}
