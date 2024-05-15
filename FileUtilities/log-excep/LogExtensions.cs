using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

internal static partial class LogExtensions
{
    [LoggerMessage(LogLevel.Information, Message = "{caller}: {text}")]
    public static partial void Information(this ILogger logger, string text, [CallerMemberName] string caller = "");

    [LoggerMessage( LogLevel.Information, Message = "{caller}: {text}" ) ]
    public static partial void Success( this ILogger logger, string text , [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: {text}")]
    public static partial void Warning(this ILogger logger, string text, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Error, Message = "{caller}: {mesg}")]
    public static partial void Error(this ILogger logger, string mesg, [CallerMemberName] string caller = "");

    [LoggerMessage( LogLevel.Error,
                     Message = "{caller}: Could not set value for {entityName}::{propName}, message was '{mesg}'" ) ]
    public static partial void SetValueFailed( this ILogger logger, string entityName, string propName, string mesg , [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: {prop} cannot be an empty string, defaulting to '{defaultValue}'")]
    public static partial void EmptyStringInvalid( this ILogger logger, string prop, string defaultValue , [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: Duplicate {type} '{value}', skipping")]
    public static partial void SkippedDuplicate( this ILogger logger, Type type, string value , [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: Duplicate {key} '{value}', skipping")]
    public static partial void SkippedDuplicate(this ILogger logger, string key, string value, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: Duplicate {type} id = {id}, skipping")]
    public static partial void SkippedDuplicate(this ILogger logger, Type type, int id, [CallerMemberName] string caller = "");

    [ LoggerMessage( LogLevel.Warning, Message = "{caller}: Duplicate {hint} {value}, replacing existing" ) ]
    public static partial void ReplacedDuplicate( this ILogger logger, string hint, string value , [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, Message = "{caller}: {prop} {value} not found")]
    public static partial void KeyNotFound(
        this ILogger logger,
        string prop,
        string value,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage( LogLevel.Error, Message = "{caller}: Undefined {parameter}" ) ]
    public static partial void UndefinedParameter( this ILogger logger, string parameter , [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Error, Message = "{caller}: Undefined {parameter}, using default value {defaultValue}")]
    public static partial void UsingDefaultValue(this ILogger logger, string parameter, string defaultValue, [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Warning, "{caller}: Unsupported {enumType} value '{value}', returning {returning}")]
    public static partial void UnsupportedEnumValue(
        this ILogger logger,
        Type enumType,
        string value,
        string returning,
        [ CallerMemberName ] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: Unsupported reader type '{importerType}'")]
    public static partial void UnsupportedImporterType(
        this ILogger logger,
        string importerType,
        [ CallerMemberName ] string caller = ""
    );
}
