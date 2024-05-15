using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

internal static partial class ConversionLogExtensions
{
    [LoggerMessage(Level = LogLevel.Error, Message = "{caller}: Could not parse {text} to {type}")]
    public static partial void ParseTextToType(this ILogger logger, string text, Type type, [CallerMemberName] string caller = "");

    [LoggerMessage(Level = LogLevel.Error, Message = "{caller}: Could not convert {text} to {type}")]
    public static partial void ConvertTextToType(this ILogger logger, string text, Type type, [CallerMemberName] string caller = "");

    [LoggerMessage(Level = LogLevel.Error, Message = "{caller}: Could not convert '{value}' to a {type}")]
    public static partial void ConversionFailed(this ILogger logger, string value, Type type, [CallerMemberName] string caller = "");
}
