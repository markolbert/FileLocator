using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public class Aggregator<TEntity, TProp>(
    AggregateFunction aggFunc,
    IExportableColumn<TEntity, TProp> column,
    StyleSetBase? labelStyleSet
)
    : Stylable( column.TableCreator, AdjustLabelStyleSet(column, labelStyleSet) )
    where TEntity : class
{
    private static StyleSetBase AdjustLabelStyleSet( IExportableColumn<TEntity, TProp> column, StyleSetBase? toAdjust )
    {
        if( toAdjust == null )
            return column.TableCreator.StyleSets.DefaultBase;

        return toAdjust with { HorizontalAlignment = HorizontalAlignment.Left };
    }

    public Type PropertyType => typeof( TProp );

    public AggregateFunction AggregateFunction { get; } = aggFunc;

    public void PopulateSheet( IWorkbook workbook, int startingRow, int startingCol, int aggFuncNum )
    {
        if( AggregateFunction == AggregateFunction.None )
            return;

        if( Creator.Sheet == null )
            return;

        var sheetCol = startingCol;
        var tableExporter = (TableCreator<TEntity>) Creator;
        var firstDataRow = startingRow + 1 - aggFuncNum;
        var lastDataRow = startingRow + tableExporter.NumDataRows - aggFuncNum;
        var aggFuncName = AggregateFunction.GetFunctionText();

        StyleSetBase aggStyle;

        if( AggregateFunction == AggregateFunction.Sum )
        {
            // be careful about what StyleSet you're using: this instance's StyleSet is 
            // the >>label<< StyleSet, while the >>column's<< StyleSet is the one
            // controlling how the values in the column are displayed
            var borderInfo = column.StyleSet.BorderInfo with { Bottom = BorderStyle.Double, Top = BorderStyle.Thin };
            aggStyle = column.StyleSet with { BorderInfo = borderInfo };
        }
        else aggStyle = column.StyleSet;

        for( var idx = 0; idx < column.ColumnsNeeded; idx++ )
        {
            var colText = NpoiExtensions.ConvertColumnNumberToText( sheetCol );

            // have to add 1 to startingRow because it's zero-based but the sheet
            // is one-based
            var range = $"{colText}{firstDataRow}:{colText}{lastDataRow}";

            var cell = Creator.Sheet.GetOrCreateCell( startingRow + tableExporter.NumDataRows, sheetCol );

            cell.CellStyle = Creator.StyleSets.ResolveCellStyle(tableExporter.Sheet!.Workbook, aggStyle);
            cell.SetCellFormula($"{aggFuncName}({range})");
            //cell.SetCellValue(-5.0);

            sheetCol++;
        }

        // only add the label if the cell immediately to the left of the first aggregate
        // formula cell is blank.
        var labelCell = Creator.Sheet.GetOrCreateCell( startingRow + tableExporter.NumDataRows, startingCol - 1 );

        if( labelCell.CellType != CellType.Blank )
            return;

        labelCell.SetCellValue( AggregateFunction.GetLabel() );
        labelCell.CellStyle = Creator.StyleSets.ResolveCellStyle( workbook, labelStyleSet ?? tableExporter.StyleSets.DefaultBase );
    }
}
