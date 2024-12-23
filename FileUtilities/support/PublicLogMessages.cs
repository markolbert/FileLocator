using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public static partial class PublicLogMessages
{
    #region general

    [LoggerMessage(Level = LogLevel.Error, Message = "{caller}: Could not convert {text} to {type}")]
    public static partial void ConvertTextToType(this ILogger logger, string text, Type type, [CallerMemberName] string caller = "");

    [LoggerMessage(Level = LogLevel.Error, Message = "{caller}: Could not parse {text} to {type}")]
    public static partial void ParseTextToType(this ILogger logger, string text, Type type, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, "{caller}: Unsupported {enumType} value '{value}', returning {returning}")]
    public static partial void UnsupportedEnumValue(
        this ILogger logger,
        Type enumType,
        string value,
        string returning,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Warning, "{caller}: Unsupported {enumType} value '{value}'")]
    public static partial void SkippingEnumValue(
        this ILogger logger,
        Type enumType,
        string value,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: Duplicate {type} '{value}', skipping")]
    public static partial void SkippedDuplicate(this ILogger logger, Type type, string value, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: Duplicate {key} '{value}', skipping")]
    public static partial void SkippedDuplicate(this ILogger logger, string key, string value, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: Duplicate {type} id = {id}, skipping")]
    public static partial void SkippedDuplicate(this ILogger logger, Type type, int id, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Information, Message = "{caller}: {text}")]
    public static partial void Information(this ILogger logger, string text, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Information, Message = "{caller}: {text}")]
    public static partial void Success(this ILogger logger, string text, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: {text}")]
    public static partial void Warning(this ILogger logger, string text, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Error, Message = "{caller}: {mesg}")]
    public static partial void Error(this ILogger logger, string mesg, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Error, "{caller}: End of JSON stream encountered")]
    public static partial void EndOfJson(this ILogger logger, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, "{caller}: {type} does not contain a public property named {propName}")]
    public static partial void PropertyNotFound(
        this ILogger logger,
        Type type,
        string propName,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Warning, "{caller}: Could not parse {jsonPropName} value '{textValue}' as {targetPropName} for {entityType} with id {key}")]
    public static partial void JsonParsingFailed(
        this ILogger logger,
        Type entityType,
        string key,
        string jsonPropName,
        string textValue,
        string targetPropName,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Error,
                     Message = "{caller}: Expected a {type} for the {entityType} key, but got a {badType}")]
    public static partial void InvalidTweakKey(
        this ILogger logger,
        Type entityType,
        Type type,
        Type badType,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: Expected {expected} but encountered {encountered}")]
    public static partial void InvalidJsonValue(
        this ILogger logger,
        string expected,
        string encountered,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: {prop} {value} not found")]
    public static partial void KeyNotFound(
        this ILogger logger,
        string prop,
        string value,
        [CallerMemberName] string caller = ""
    );

    #endregion

    #region DbContext 

    [LoggerMessage(Level = LogLevel.Critical, Message = "{caller}: Could not save changes to database, message was {mesg}")]
    public static partial void DbUpdateExceptionEncountered(this ILogger logger, string mesg, [CallerMemberName] string caller = "");

    [LoggerMessage(Level = LogLevel.Error, Message = "{caller}: Could not find entity type {entityType} in {schemaType}")]
    public static partial void MissingEntityType(this ILogger logger, Type entityType, Type schemaType, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Error, "{caller}: Could not find DbSet for {entityType} on {dbContextType}, message was '{mesg}'")]
    public static partial void InvalidDbSet(
        this ILogger logger,
        Type dbContextType,
        Type entityType,
        string mesg,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Warning, "{caller}: {entityType}::{propName} is not tweakable")]
    public static partial void PropertyNotTweakable(this ILogger logger, string entityType, string propName, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Error, "{caller}: {dbContextType} is not defined")]
    public static partial void UndefinedDatabase(
        this ILogger logger,
        Type dbContextType,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: {dbContextType} does not contain {setType}, message was '{mesg}'")]
    public static partial void UnknownDbSet(
        this ILogger logger,
        Type dbContextType,
        Type setType,
        string mesg,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: No Names found for RemappedConstituentId {id}")]
    public static partial void NoNamesFound(this ILogger logger, int id, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Error, Message = "{caller}: Could not find {type} with id {id}")]
    public static partial void InvalidId(
        this ILogger logger,
        Type type,
        int id,
        [CallerMemberName] string caller = ""
    );

    #endregion

    #region file-related

    [LoggerMessage(Level = LogLevel.Error,
                   Message = "{caller}: File '{file}' not found")]
    public static partial void FileNotFound(
        this ILogger logger,
        string file,
        [CallerMemberName] string caller = ""
    );

    [ LoggerMessage( LogLevel.Error, "{type}::{caller}: undefined path" ) ]
    public static partial void UndefinedPath(
        this ILogger logger,
        string type,
        [ CallerMemberName ] string caller = ""
    );

    [LoggerMessage(LogLevel.Error,
                     "{caller}: File {path} not parsed, message was '{mesg}' ({lineNum} : {srcFile})")]
    public static partial void FileParsingError(
        this ILogger logger,
        string path,
        string mesg,
        [CallerMemberName] string caller = "",
        [CallerFilePath] string srcFile = "",
        [CallerLineNumber] int lineNum = 0
    );

    [LoggerMessage(LogLevel.Error, "{caller}: No import file source defined")]
    public static partial void UndefinedImportSource(this ILogger logger, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Error, "{caller}: {srcType} does not contain tweak file path information")]
    public static partial void TweaksPathMissing(this ILogger logger, Type srcType, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Error, "{caller}: No import file source defined for importer {importerType}")]
    public static partial void UndefinedImportSource(this ILogger logger, Type importerType, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Error, "{caller}: Unsupported file scope '{scope}'")]
    public static partial void UnsupportedFileScope(
        this ILogger logger,
        string scope,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: Could not read header on '{fileName}'")]
    public static partial void HeaderUnreadable(
        this ILogger logger,
        string fileName,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: Duplicate column {colIdx} value read for record {curRecord} from '{filePath}'")]
    public static partial void DuplicateColumnRead(
        this ILogger logger,
        string filePath,
        int colIdx,
        int curRecord,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: File '{filePath}' appears {count} times in the import configuration file")]
    public static partial void MultipleImportFileDescriptors(
        this ILogger logger,
        string filePath,
        int count,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: Directory {path} not found")]
    public static partial void DirectoryNotFound(
        this ILogger logger,
        string path,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: Unsupported file source type {type}")]
    public static partial void UnsupportedFileSourceType(
        this ILogger logger,
        Type type,
        [CallerMemberName] string caller = ""
    );

    #endregion

    #region type-related

    [LoggerMessage(Level = LogLevel.Error, Message = "{caller}: Expected a {type} but got a {badType}")]
    public static partial void UnexpectedType(
        this ILogger logger,
        Type type,
        Type badType,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Critical,
                   Message = "{caller}: Could not create instance of {type}, message was '{mesg}'")]
    public static partial void InstanceCreationFailed(
        this ILogger logger,
        Type type,
        string mesg,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, Message = "{caller}: Supplied {type} is not assignable to {correctType}")]
    public static partial void InvalidTypeAssignment(
        this ILogger logger,
        Type type,
        Type correctType,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Critical, Message = "{caller}: No matching internal constructor found")]
    public static partial void NoMatchingConstructor(this ILogger logger, [CallerMemberName] string caller = "");

    [LoggerMessage(Level = LogLevel.Warning, Message = "{caller}: {type} is not configured")]
    public static partial void UnconfiguredInstance(
        this ILogger logger,
        Type type,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, Message = "{caller}: Unsupported {type} value {value}")]
    public static partial void UnsupportedValue(
        this ILogger logger,
        Type type,
        string value,
        [CallerMemberName] string caller = ""
    );

    #endregion
}
