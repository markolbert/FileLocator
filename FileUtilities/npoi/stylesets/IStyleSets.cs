using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public interface IStyleSets
{
    string DefaultFontName { get; }
    int DefaultFontHeightInPoints { get; }
    
    StyleSetBase DefaultBase { get; }
    StyleSetBase DefaultTitle { get; }
    StyleSetBase DefaultHeader { get; }
    
    DoubleStyleSet DefaultDouble { get; }
    IntegerStyleSet DefaultInteger { get; }
    IntegerStyleSet DefaultUngroupedInteger { get; }
    IntegerStyleSet DefaultUngroupedIntegerHeader { get; }
    DoubleStyleSet DefaultPercent { get; }
    
    DateStyleSet DefaultDate { get; }
    
    StyleSetBase DefaultBoolean { get; }

    StyleSetBase this[string name] { get; }
    StyleSetBase this[ Type type ] { get; }

    ICellStyle? ResolveCellStyle( IWorkbook workbook, StyleSetBase? styleSet );
}
