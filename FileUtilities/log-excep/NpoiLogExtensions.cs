using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public static partial class NpoiLogExtensions
{
    [LoggerMessage(Level = LogLevel.Error, Message = "{caller}: ISheet is not defined")]
    public static partial void UndefinedSheet(this ILogger logger, [CallerMemberName] string caller = "");

    [LoggerMessage(Level = LogLevel.Warning, Message = "{caller}: Unsupported cell value type {type} in {sheetName}")]
    public static partial void UnsupportedCellType(this ILogger logger, Type type, string sheetName, [CallerMemberName] string caller = "");

    [LoggerMessage(Level = LogLevel.Warning, Message = "{caller}: No range name specified, ignoring")]
    public static partial void MissingRangeName(this ILogger logger, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: No columns supplied for defining range {range}, ignoring")]
    public static partial void UndefinedNamedRange(this ILogger logger, string range, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: Created a replacement workbook-wide NamedRange '{name}'")]
    public static partial void DuplicateNamedRange(this ILogger logger, string name, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: Ignoring max column width because column is not autosized")]
    public static partial void SuperfluousMaxColumnWidth(this ILogger logger, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: Invalid max column width ({maxWidth})")]
    public static partial void InvalidMaxColumnWidth(this ILogger logger, int maxWidth, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: Could not find sheet '{sheet}' to update, creating new ISheet")]
    public static partial void MissingSheet(this ILogger logger, string sheet, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: Missing header row for '{fileName}'")]
    public static partial void MissingCsvHeaderRow(this ILogger logger, string fileName, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: Missing header row for sheet {sheetName}")]
    public static partial void MissingSheetHeaderRow(this ILogger logger, string sheetName, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Error, "{caller}: No column mappings defined for sheet {sheetName}")]
    public static partial void NoColumnMappings(
        this ILogger logger,
        string sheetName,
        [ CallerMemberName ] string caller = ""
    );

    [LoggerMessage(Level = LogLevel.Warning, Message = "{caller}: Truncated data vector: row {row} only has {col} columns")]
    public static partial void TruncatedDataVector(this ILogger logger, int row, int col, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: StyleSet {name} not defined, defaulting")]
    public static partial void UndefinedStyleSet(this ILogger logger, string name, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: StyleSet for {type} not defined, defaulting")]
    public static partial void UndefinedStyleSet(this ILogger logger, Type type, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: {styleName} not found, returning default")]
    public static partial void MissingStyle(
        this ILogger logger,
        string styleName,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: Unknown color '{color}', returning {returning}")]
    public static partial void UnknownColor(
        this ILogger logger,
        string color,
        string returning,
        [ CallerMemberName ] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: {type} is not initialized")]
    public static partial void NotInitialized( this ILogger logger, Type type, [ CallerMemberName ] string caller = "" );

    [LoggerMessage(LogLevel.Warning, "{caller}: ICellStyle cannot be cached because {type} is not derived from StyleSetBase")]
    public static partial void UnsupportedStyleSet(
        this ILogger logger,
        Type type,
        [ CallerMemberName ] string caller = ""
    );

    [LoggerMessage(LogLevel.Warning,"{caller}: Configuration file contains no style definitions")]
    public static partial void StylesNotDefined( this ILogger logger, [ CallerMemberName ] string caller = "" );

    [LoggerMessage(LogLevel.Error, "{caller}: Multiple {styleType} definitions, only the first will be used")]
    public static partial void MultipleStyleDefinitions(
        this ILogger logger,
        Type styleType,
        [ CallerMemberName ] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: Style name collision, changed {oldName} to {newName}")]
    public static partial void StyleNameCollision(
        this ILogger? logger,
        string oldName,
        string newName,
        [ CallerMemberName ] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: No format clauses defined for SimpleNumberStyle")]
    public static partial void NoSimpleNumberClauses( this ILogger logger, [ CallerMemberName ] string caller = "" );

    [LoggerMessage(LogLevel.Error, "{caller}: More than 4 format clauses defined for SimpleNumberStyle")]
    public static partial void TooManySimpleNumberClauses(this ILogger logger, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Error, "{caller}: Unsupported style configuration type {type}")]
    public static partial void UnsupportedStyleConfig(
        this ILogger logger,
        Type type,
        [ CallerMemberName ] string caller = ""
    );

    [LoggerMessage(LogLevel.Warning, "{caller}: Default style set '{styleName}' not defined, defaulting")]
    public static partial void UndefinedDefaultStyle(
        this ILogger logger,
        string styleName,
        [ CallerMemberName ] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: Duplicate sheet name '{sheetName}'")]
    public static partial void DuplicateSheetName(
        this ILogger logger,
        string sheetName,
        [ CallerMemberName ] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: Undefined sheet name '{sheetName}'")]
    public static partial void UndefinedSheetName(
        this ILogger logger,
        string sheetName,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(Level = LogLevel.Error, Message = "{caller}: Could not find ISheet {sheetName}, message was '{mesg}'")]
    public static partial void MissingSheetWithMessage(this ILogger logger, string sheetName, string mesg, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Error, Message = "{caller}: In {sheetName}, expected column {colNum} name to be '{name}' but found '{badName}'")]
    public static partial void BadHeaderName(this ILogger logger, string sheetName, int colNum, string name, string badName, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Error, Message = "{caller}: Failed to set column {colNum} in row {rowNum}")]
    public static partial void FailedToSetCellValue(this ILogger logger, int colNum, int rowNum, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Error, Message = "{caller}: Expected {num} columns but only matched {badNum}")]
    public static partial void HeaderCountMismatch(this ILogger logger, int num, int badNum, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: Invalid {prop} value {value}, ignoring")]
    public static partial void InvalidPropertyValue(this ILogger logger, string prop, int value, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Error, "{caller}: Unsupported Expression, message was '{message}'")]
    public static partial void UnsupportedExpression(
        this ILogger logger,
        string message,
        [ CallerMemberName ] string caller = ""
    );
}
