using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public class NpoiDecimalConverter(ILoggerFactory? loggerFactory) : NpoiConverter<decimal>(loggerFactory)
{
    public override decimal Convert(ICell cell) => System.Convert.ToDecimal(cell.NumericCellValue);
}
