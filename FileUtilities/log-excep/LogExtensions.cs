using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public static partial class LogExtensions
{
    [LoggerMessage(LogLevel.Warning, "{caller}: Unsupported {enumType} value '{value}', returning {returning}")]
    public static partial void UnsupportedEnumValue(
        this ILogger logger,
        Type enumType,
        string value,
        string returning,
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

    [ LoggerMessage( LogLevel.Error,
                     Message = "{caller}: Expected a {type} for the {entityType} key, but got a {badType}" ) ]
    public static partial void InvalidTweakKey(
        this ILogger logger,
        Type entityType,
        Type type,
        Type badType,
        [ CallerMemberName ] string caller = ""
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

    [LoggerMessage( LogLevel.Error,
                    Message = "{caller}: Could not set value for {entityName}::{propName}, message was '{mesg}'" ) ]
    internal static partial void SetValueFailed( this ILogger logger, string entityName, string propName, string mesg , [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: {prop} cannot be an empty string, defaulting to '{defaultValue}'")]
    internal static partial void EmptyStringInvalid( this ILogger logger, string prop, string defaultValue , [CallerMemberName] string caller = "");

    [ LoggerMessage( LogLevel.Warning, Message = "{caller}: Duplicate {hint} {value}, replacing existing" ) ]
    internal static partial void ReplacedDuplicate( this ILogger logger, string hint, string value , [CallerMemberName] string caller = "");

    [LoggerMessage( LogLevel.Error, Message = "{caller}: Undefined {parameter}" ) ]
    internal static partial void UndefinedParameter( this ILogger logger, string parameter , [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Error, Message = "{caller}: Undefined {parameter}, using default value {defaultValue}")]
    internal static partial void UsingDefaultValue(this ILogger logger, string parameter, string defaultValue, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Error, "{caller}: Unsupported reader type '{importerType}'")]
    internal static partial void UnsupportedImporterType(
        this ILogger logger,
        string importerType,
        [ CallerMemberName ] string caller = ""
    );

}
