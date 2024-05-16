using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public static partial class TypeLogExtensions
{
    [ LoggerMessage( Level = LogLevel.Error, Message = "{caller}: Expected a {type} but got a {badType}" ) ]
    public static partial void UnexpectedType(
        this ILogger logger,
        Type type,
        Type badType,
        [ CallerMemberName ] string caller = ""
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

    [LoggerMessage( LogLevel.Critical, Message = "{caller}: Could not create instance of {type}" ) ]
    internal static partial void InstanceNotRetrieved(
        this ILogger logger,
        Type type,
        [ CallerMemberName ] string caller = ""
    );

    [ LoggerMessage( Level = LogLevel.Error, Message = "{caller}: {type} is unsupported{retVal}" ) ]
    internal static partial void UnsupportedType(
        this ILogger logger,
        Type type,
        string retVal,
        [ CallerMemberName ] string caller = ""
    );

}
