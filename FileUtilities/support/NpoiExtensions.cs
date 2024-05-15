using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public static class NpoiExtensions
{
    public const int MaxRows = 1048576;
    public const int MaxColumns = 16384;

    public static IRow GetOrCreateRow( this ISheet worksheet, int rowNum )
    {
        rowNum = rowNum < 0 ? 0 : rowNum >= MaxRows ? MaxRows - 1 : rowNum;

        return worksheet.GetRow( rowNum ) ?? worksheet.CreateRow( rowNum );
    }

    public static ICell GetOrCreateCell( this IRow row, int colNum )
    {
        colNum = colNum < 0 ? 0 : colNum >= MaxColumns ? MaxColumns - 1 : colNum;

        return row.GetCell( colNum ) ?? row.CreateCell( colNum );
    }

    public static ICell GetOrCreateCell( this ISheet worksheet, int rowNum, int colNum ) =>
        worksheet.GetOrCreateRow( rowNum ).GetOrCreateCell( colNum );

    public static string ConvertColumnNumberToText( int col )
    {
        var thirdOrder = col / 676;
        var secondOrder = ( col - thirdOrder * 676 ) / 26;
        var firstOrder = col - thirdOrder * 676 - secondOrder * 26;

        var colText = new StringBuilder();

        if( thirdOrder > 0 )
            colText.Append( Convert.ToChar( 65 + thirdOrder - 1 ) );

        if( secondOrder > 0 )
            colText.Append( Convert.ToChar( 65 + secondOrder - 1 ) );

        colText.Append( Convert.ToChar( 65 + firstOrder ) );

        return colText.ToString();
    }

    public static bool AppliesToType( this AggregateFunction aggFunc, Type type )
    {
        var usageAttr = aggFunc.GetAggregateFunctionUsageAttribute();
        return usageAttr != null && usageAttr.IsSupported( type );
    }

    public static string GetFunctionText( this AggregateFunction aggFunc )
    {
        var usageAttr = aggFunc.GetAggregateFunctionUsageAttribute();
        return usageAttr == null ? string.Empty : usageAttr.FunctionText;
    }

    public static string GetLabel( this AggregateFunction aggFunc )
    {
        var usageAttr = aggFunc.GetAggregateFunctionUsageAttribute();
        return usageAttr == null ? string.Empty : usageAttr.Label;
    }

    private static AggregateFunctionUsageAttribute? GetAggregateFunctionUsageAttribute( this AggregateFunction aggFunc )
    {
        foreach( var memberInfo in typeof( AggregateFunction )
                                  .GetMembers()
                                  .Where( mi => mi.MemberType == MemberTypes.Field ) )
        {
            if( !Enum.TryParse( typeof( AggregateFunction ), memberInfo.Name, out var rawFlag ) )
                continue;

            var curFlag = (AggregateFunction) rawFlag;

            if( curFlag == AggregateFunction.None || !aggFunc.HasFlag( curFlag ) )
                continue;

            return memberInfo.GetCustomAttribute<AggregateFunctionUsageAttribute>();
        }

        return null;
    }

    public static ISheet ClearSheet( this IWorkbook workbook, string sheetName, ILogger? logger = null )
    {
        var retVal = workbook.GetSheet( sheetName );

        if( retVal == null )
        {
            logger?.MissingSheet( sheetName );
            return workbook.CreateSheet( sheetName );
        }

        // clear the sheet...but not the named ranges, which we will adjust
        // when we're done adding data to the sheet
        var lastCol = 0;

        for( var rowIdx = 0; rowIdx <= retVal.LastRowNum; rowIdx++ )
        {
            var row = retVal.GetRow( rowIdx );
            if( row == null || row.FirstCellNum < 0 )
                continue;

            if( row.LastCellNum > lastCol)
                lastCol = row.LastCellNum;

            for( var colIdx = row.FirstCellNum; colIdx < row.LastCellNum; colIdx++ )
            {
                var cell = row.GetCell( colIdx );
                cell?.SetBlank();
            }
        }

        for( var colIdx = 0; colIdx <= lastCol; colIdx++ )
        {
            retVal.SetColumnWidth( colIdx, 2560 );
        }

        // have to remove in reverse order because the internal list keeps getting
        // updated upon removal
        for( var mergeRegionIdx = retVal.NumMergedRegions - 1; mergeRegionIdx >= 0; mergeRegionIdx-- )
        {
            retVal.RemoveMergedRegion( mergeRegionIdx );
        }

        return retVal;
    }

    public static int ExportTitlesToSheet( this IEnumerable<TitleRow> titleRows, IWorkbook workbook )
    {
        var rowNum = 0;

        foreach( var titleRow in titleRows )
        {
            if( titleRow.Creator.Sheet == null )
                continue;

            var cell = titleRow.Creator.Sheet.GetOrCreateCell( rowNum, 0 );

            cell.SetCellValue( titleRow.Text );
            cell.CellStyle = titleRow.Creator.StyleSets.ResolveCellStyle( workbook, titleRow.StyleSet );

            rowNum++;
        }

        return rowNum;
    }
}
