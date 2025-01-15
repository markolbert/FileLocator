using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

internal static partial class InternalLogMessages
{
    #region general

    [LoggerMessage(Level = LogLevel.Error, Message = "{caller}: Could not convert '{value}' to a {type}")]
    internal static partial void ConversionFailed(this ILogger logger, string value, Type type, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Error,
                   Message = "{caller}: Could not set value for {entityName}::{propName}, message was '{mesg}'")]
    internal static partial void SetValueFailed(this ILogger logger, string entityName, string propName, string mesg, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: {prop} cannot be an empty string, defaulting to '{defaultValue}'")]
    internal static partial void EmptyStringInvalid(this ILogger logger, string prop, string defaultValue, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: Duplicate {hint} {value}, replacing existing")]
    internal static partial void ReplacedDuplicate(this ILogger logger, string hint, string value, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Error, Message = "{caller}: Undefined {parameter}")]
    internal static partial void UndefinedParameter(this ILogger logger, string parameter, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Error, Message = "{caller}: Undefined {parameter}, using default value {defaultValue}")]
    internal static partial void UsingDefaultValue(this ILogger logger, string parameter, string defaultValue, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Error, "{caller}: Unsupported reader type '{importerType}'")]
    internal static partial void UnsupportedImporterType(
        this ILogger logger,
        string importerType,
        [CallerMemberName] string caller = ""
    );

    #endregion

    #region DbContext

    [LoggerMessage(Level = LogLevel.Error, Message = "{caller}: Some database entities were not configured: {types}")]
    internal static partial void EntitiesNotConfigured(this ILogger logger, string types, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, "{caller}: There were {numIncomplete} incomplete tweaks. Ids: {ids}")]
    internal static partial void JsonIncompleteTweaks(
        this ILogger logger,
        int numIncomplete,
        string ids,
        [CallerMemberName] string caller = ""
    );

    #endregion

    #region file-related

    [LoggerMessage(LogLevel.Warning,
                   Message =
                       "{caller}: Could not read file '{file}', message was '{mesg}'; deleting and re-creating instead")]
    internal static partial void WorkbookFileUnreadable(
        this ILogger logger,
        string file,
        string mesg,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(Level = LogLevel.Error,
                   Message = "{caller}: Multiple matches to '{file}' found in '{dir}' and its subdirectories")]
    internal static partial void MultipleFileMatches(
        this ILogger logger,
        string file,
        string dir,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Critical, Message = "{caller}: Could not read file '{fileName}', message was '{mesg}'")]
    internal static partial void FileUnreadable(
        this ILogger logger,
        string fileName,
        string mesg,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Warning, "{caller}: unneeded record change file '{path}' specified")]
    internal static partial void UnneededTweaksFile(
        this ILogger logger,
        Type importerType,
        string path,
        [ CallerMemberName ] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: Could not find field '{field}' to ignore in {entityType}")]
    internal static partial void IgnoreFieldNotFound(
        this ILogger logger,
        Type entityType,
        string field,
        [ CallerMemberName ] string caller = ""
    );

    #endregion

    #region type-related

    [LoggerMessage(LogLevel.Critical, Message = "{caller}: Could not create instance of {type}")]
    internal static partial void InstanceNotRetrieved(
        this ILogger logger,
        Type type,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(Level = LogLevel.Error, Message = "{caller}: {type} is unsupported{retVal}")]
    internal static partial void UnsupportedType(
        this ILogger logger,
        Type type,
        string retVal,
        [CallerMemberName] string caller = ""
    );

    #endregion

    #region NPOI-related

    [LoggerMessage(Level = LogLevel.Error, Message = "{caller}: ISheet is not defined")]
    internal static partial void UndefinedSheet(this ILogger logger, [CallerMemberName] string caller = "");

    [LoggerMessage(Level = LogLevel.Warning, Message = "{caller}: Unsupported cell value type {type} in {sheetName}")]
    internal static partial void UnsupportedCellType(this ILogger logger, Type type, string sheetName, [CallerMemberName] string caller = "");

    [LoggerMessage(Level = LogLevel.Warning, Message = "{caller}: No range name specified, ignoring")]
    internal static partial void MissingRangeName(this ILogger logger, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: No columns supplied for defining range {range}, ignoring")]
    internal static partial void UndefinedNamedRange(this ILogger logger, string range, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: Created a replacement workbook-wide NamedRange '{name}'")]
    internal static partial void DuplicateNamedRange(this ILogger logger, string name, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: Ignoring max column width because column is not autosized")]
    internal static partial void SuperfluousMaxColumnWidth(this ILogger logger, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: Invalid max column width ({maxWidth})")]
    internal static partial void InvalidMaxColumnWidth(this ILogger logger, int maxWidth, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: Could not find sheet '{sheet}' to update, creating new ISheet")]
    internal static partial void MissingSheet(this ILogger logger, string sheet, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: Missing header row for '{fileName}'")]
    internal static partial void MissingCsvHeaderRow(this ILogger logger, string fileName, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: Missing header row for sheet {sheetName}")]
    internal static partial void MissingSheetHeaderRow(this ILogger logger, string sheetName, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Error, "{caller}: No column mappings defined for sheet {sheetName}")]
    internal static partial void NoColumnMappings(
        this ILogger logger,
        string sheetName,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(Level = LogLevel.Warning, Message = "{caller}: Truncated data vector: row {row} only has {col} columns")]
    internal static partial void TruncatedDataVector(this ILogger logger, int row, int col, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: StyleSet {name} not defined, defaulting")]
    internal static partial void UndefinedStyleSet(this ILogger logger, string name, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: StyleSet for {type} not defined, defaulting")]
    internal static partial void UndefinedStyleSet(this ILogger logger, Type type, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: {styleName} not found, returning default")]
    internal static partial void MissingStyle(
        this ILogger logger,
        string styleName,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: Unknown color '{color}', returning {returning}")]
    internal static partial void UnknownColor(
        this ILogger logger,
        string color,
        string returning,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: {type} is not initialized")]
    internal static partial void NotInitialized(this ILogger logger, Type type, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, "{caller}: ICellStyle cannot be cached because {type} is not derived from StyleSetBase")]
    internal static partial void UnsupportedStyleSet(
        this ILogger logger,
        Type type,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Information, "{caller}: Configuration file contains no style definitions")]
    internal static partial void StylesNotDefined(this ILogger logger, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Error, "{caller}: Multiple {styleType} definitions, only the first will be used")]
    internal static partial void MultipleStyleDefinitions(
        this ILogger logger,
        Type styleType,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: Style name collision, changed {oldName} to {newName}")]
    internal static partial void StyleNameCollision(
        this ILogger? logger,
        string oldName,
        string newName,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: No format clauses defined for SimpleNumberStyle")]
    internal static partial void NoSimpleNumberClauses(this ILogger logger, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Error, "{caller}: More than 4 format clauses defined for SimpleNumberStyle")]
    internal static partial void TooManySimpleNumberClauses(this ILogger logger, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Error, "{caller}: Unsupported style configuration type {type}")]
    internal static partial void UnsupportedStyleConfig(
        this ILogger logger,
        Type type,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Information, "{caller}: Default style set '{styleName}' not defined, defaulting")]
    internal static partial void UndefinedDefaultStyle(
        this ILogger logger,
        string styleName,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: Duplicate sheet name '{sheetName}'")]
    internal static partial void DuplicateSheetName(
        this ILogger logger,
        string sheetName,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: Undefined sheet name '{sheetName}'")]
    internal static partial void UndefinedSheetName(
        this ILogger logger,
        string sheetName,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(Level = LogLevel.Error, Message = "{caller}: Could not find ISheet {sheetName}, message was '{mesg}'")]
    internal static partial void MissingSheetWithMessage(this ILogger logger, string sheetName, string mesg, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Error, Message = "{caller}: In {sheetName}, expected column {colNum} name to be '{name}' but found '{badName}'")]
    internal static partial void BadHeaderName(this ILogger logger, string sheetName, int colNum, string name, string badName, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Error, Message = "{caller}: Failed to set column {colNum} in row {rowNum}")]
    internal static partial void FailedToSetCellValue(this ILogger logger, int colNum, int rowNum, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Error, Message = "{caller}: Expected {num} columns but only matched {badNum}")]
    internal static partial void HeaderCountMismatch(this ILogger logger, int num, int badNum, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: Invalid {prop} value {value}, ignoring")]
    internal static partial void InvalidPropertyValue(this ILogger logger, string prop, int value, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Error, "{caller}: Unsupported Expression, message was '{message}'")]
    internal static partial void UnsupportedExpression(
        this ILogger logger,
        string message,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: {bindingType} cannot bind to property name from {propExpr}, defaulting to '{defaultName}'")]
    internal static partial void UnboundProperty(
        this ILogger logger,
        Type bindingType,
        string propExpr,
        string defaultName,
        [ CallerMemberName ] string caller = ""
    );

    [LoggerMessage(LogLevel.Warning, "{caller}: Header type {headerType} is not supported for {colType}, ignoring")]
    internal static partial void UnsupportedHeader(
        this ILogger logger,
        Type headerType,
        Type colType,
        [ CallerMemberName ] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: {mesg}")]
    internal static partial void NpoiEvaluationException(
        this ILogger logger,
        string mesg,
        [ CallerMemberName ] string caller = ""
    );

    #endregion

    [LoggerMessage(LogLevel.Error, "{caller}: Could not create instance of {adjusterType} on entity {entityType} for property {propName}")]
    internal static partial void PropertyAdjusterNotCreated(
        this ILogger logger,
        Type entityType,
        string propName,
    Type adjusterType,
    [ CallerMemberName ] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: Could not create instance of {adjusterType} on entity {entityType}")]
    internal static partial void RecordAdjusterNotCreated(
        this ILogger logger,
        Type entityType,
        Type adjusterType,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: Could not get value for property {propName} ({propType}) on entity {entityType}, message was '{mesg}' ")]
    internal static partial void CouldNotGetValue(
        this ILogger logger,
        Type entityType,
        string propName,
        Type propType,
        string mesg,
        [ CallerMemberName ] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: Could not get value for property {propName} on entity {entityType}, message was '{mesg}' ")]
    internal static partial void CouldNotGetValue(
        this ILogger logger,
        Type entityType,
        string propName,
        string mesg,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: Could not set value '{value}' to property {propName} ({propType}) on entity {entityType}, message was '{mesg}' ")]
    internal static partial void CouldNotSetValue(
        this ILogger logger,
        Type entityType,
        string propName,
        Type propType,
        string value,
        string mesg,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Critical, "{caller}: Could not create {filterType}")]
    internal static partial void CannotCreateEntityFilter(
        this ILogger logger,
        Type filterType,
        [ CallerMemberName ] string caller = ""
    );

    [LoggerMessage(LogLevel.Critical, "{caller}: Expected a {correct} but got a {incorrect}")]
    internal static partial void IncorrectEntityFilterType(
        this ILogger logger,
        Type incorrect,
        Type correct,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Critical, "{caller}: Could not create an instance of {interfaceType} on type {type}")]
    internal static partial void MissingInterface(
        this ILogger logger,
        Type type,
        Type interfaceType,
        [ CallerMemberName ] string caller = ""
    );

    [LoggerMessage(LogLevel.Critical, "{caller}: Failed to read {entityType} replacements from {path}, message was '{mesg}'")]
    internal static partial void FailedToReadReplacements(
        this ILogger logger,
        Type entityType,
        string path,
        string mesg,
        [ CallerMemberName ] string caller = ""
    );

    [LoggerMessage(LogLevel.Warning, "{caller}: No replacements processed for {type}")]
    internal static partial void NoReplacementsChecked(
        this ILogger logger,
        Type type,
        [ CallerMemberName ] string caller = ""
    );

    [LoggerMessage(LogLevel.Warning, "{caller}: No replacements processed for {type}")]
    internal static partial void NoReplacementsMade(
        this ILogger logger,
        Type type,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Warning, "{caller}: {type} replacement properties not checked: {props}")]
    internal static partial void ReplacementPropertiesNotChecked(
        this ILogger logger,
        Type type,
        string props,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Warning, "{caller}: {type} replacement properties not processed: {props}")]
    internal static partial void ReplacementPropertiesNotProcessed(
        this ILogger logger,
        Type type,
        string props,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: Could not get getter for {propName} on {type}")]
    internal static partial void UndefinedGetter(
        this ILogger logger,
        Type type,
        string propName,
        [ CallerMemberName ] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: Could not get setter for {propName} on {type}")]
    internal static partial void UndefinedSetter(
        this ILogger logger,
        Type type,
        string propName,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Critical, "{caller}: Key field {key} not found int ExpandoObject for {type}")]
    internal static partial void ExpandoKeyFieldNotFound(
        this ILogger logger,
        Type type,
        string key,
        [ CallerMemberName ] string caller = ""
    );

    [LoggerMessage(LogLevel.Warning, "{caller}: {type} adjustments are not being recorded")]
    internal static partial void AdjustmentsNotRecorded(
        this ILogger logger,
        Type type,
        [ CallerMemberName ] string caller = ""
    );
}
