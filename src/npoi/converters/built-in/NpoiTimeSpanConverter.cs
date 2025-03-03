using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public class NpoiTimeSpanConverter(ILoggerFactory? loggerFactory) : NpoiConverter<TimeSpan>(loggerFactory)
{
    public override TimeSpan Convert( ICell cell ) => ( cell.DateCellValue ?? DateTime.MinValue ).TimeOfDay;
}
