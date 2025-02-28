using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public record FormatCodeStyleSet : StyleSetBase
{
    public FormatCodeStyleSet(
        FormatCodeStyle config
    )
        : base( config.StyleName )
    {
        FormatText = config.FormatText;
    }

    protected FormatCodeStyleSet(
        string styleName
    )
        : base( styleName )
    {
    }

    public string? FormatText { get; init; }

    public override ICellStyle CreateCellStyle( IWorkbook workbook )
    {
        var retVal = base.CreateCellStyle( workbook );

        if( !string.IsNullOrEmpty( FormatText ) )
            retVal.DataFormat = workbook.CreateDataFormat().GetFormat( FormatText );

        return retVal;
    }
}
