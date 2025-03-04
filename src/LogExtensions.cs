using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace J4JSoftware.FileUtilities;

internal static partial class LogExtensions
{
    [ LoggerMessage( LogLevel.Trace, "{caller}: starting validation of path {path} " ) ]
    internal static partial void PathValidationStart(
        this ILogger logger,
        string path,
        [ CallerMemberName ] string caller = ""
    );

    [ LoggerMessage( LogLevel.Trace, "{caller}: added required file extension {ext} " ) ]
    internal static partial void AddedRequiredExtension(
        this ILogger logger,
        string ext,
        [ CallerMemberName ] string caller = ""
    );

    [ LoggerMessage( LogLevel.Trace, "{caller}: found file {path} " ) ]
    internal static partial void FoundFile(
        this ILogger logger,
        string path,
        [ CallerMemberName ] string caller = ""
    );

    [ LoggerMessage( Level = LogLevel.Error,
                     Message = "{caller}: File '{file}' not found" ) ]
    public static partial void FileNotFound(
        this ILogger logger,
        string file,
        [ CallerMemberName ] string caller = ""
    );

    [ LoggerMessage( LogLevel.Trace, "{caller}: checking {folder} for {path}" ) ]
    internal static partial void CheckAlternativeLocation(
        this ILogger logger,
        string folder,
        string path,
        [ CallerMemberName ] string caller = ""
    );

    [ LoggerMessage( LogLevel.Error, Message = "{caller}: exception when trying to {action}, message was {exMesg}" ) ]
    internal static partial void Exception(
        this ILogger logger,
        string action,
        string exMesg,
        [ CallerMemberName ] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: file {path} is not accessible")]
    internal static partial void FileNotAccessible(
        this ILogger logger,
        string path,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: unsupported operating system, {mesg}")]
    internal static partial void UnsupportedOs(
        this ILogger logger,
        string mesg,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Trace, "{caller}: invalid path, {mesg}")]
    internal static partial void InvalidPath(
        this ILogger logger,
        string mesg,
        [CallerMemberName] string caller = ""
    );

}