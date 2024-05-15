using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

internal static partial class DbLogExtensions
{
    [LoggerMessage(Level = LogLevel.Error, Message = "{caller}: Some database entities were not configured: {types}")]
    public static partial void EntitiesNotConfigured(this ILogger logger, string types, [CallerMemberName] string caller = "");

    [LoggerMessage(Level = LogLevel.Critical, Message = "{caller}: Could not save changes to database, message was {mesg}")]
    public static partial void DbUpdateExceptionEncountered(this ILogger logger, string mesg, [CallerMemberName] string caller = "");

    [LoggerMessage(Level = LogLevel.Error, Message = "{caller}: Could not find entity type {entityType} in {schemaType}")]
    public static partial void MissingEntityType(this ILogger logger, Type entityType, Type schemaType, [CallerMemberName] string caller = "");

    [ LoggerMessage( LogLevel.Warning, Message = "{caller}: No Names found for RemappedConstituentId {id}" ) ]
    public static partial void NoNamesFound( this ILogger logger, int id , [CallerMemberName] string caller = "");

    [LoggerMessage(LogLevel.Error, Message = "{caller}: Could not find {type} with id {id}")]
    public static partial void InvalidId(
        this ILogger logger,
        Type type,
        int id,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: {dbContextType} does not contain {setType}, message was '{mesg}'")]
    public static partial void UnknownDbSet(
        this ILogger logger,
        Type dbContextType,
        Type setType,
        string mesg,
        [ CallerMemberName ] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: Could not find DbSet for {entityType} on {dbContextType}, message was '{mesg}'")]
    public static partial void InvalidDbSet(
        this ILogger logger,
        Type dbContextType,
        Type entityType,
        string mesg,
        [CallerMemberName] string caller = ""
    );

}
