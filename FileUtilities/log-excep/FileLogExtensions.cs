using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

internal static partial class FileLogExtensions
{
    [LoggerMessage(Level = LogLevel.Error,
                   Message = "{caller}: File '{file}' not found")]
    public static partial void FileNotFound(
        this ILogger logger,
        string file,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: No import file source defined")]
    public static partial void UndefinedImportSource( this ILogger logger, [ CallerMemberName ] string caller = "" );

    [LoggerMessage(LogLevel.Error, "{caller}: No import file source defined for importer {importerType}")]
    public static partial void UndefinedImportSource(this ILogger logger, Type importerType, [CallerMemberName] string caller = "");

    [LoggerMessage( LogLevel.Warning,
                    Message =
                        "{caller}: Could not read file '{file}', message was '{mesg}'; deleting and re-creating instead" ) ]
    public static partial void WorkbookFileUnreadable(
        this ILogger logger,
        string file,
        string mesg,
        [ CallerMemberName ] string caller = ""
    );

    [ LoggerMessage( Level = LogLevel.Error,
                     Message = "{caller}: Multiple matches to '{file}' found in '{dir}' and its subdirectories" ) ]
    public static partial void MultipleFileMatches(
        this ILogger logger,
        string file,
        string dir,
        [ CallerMemberName ] string caller = ""
    );

    [ LoggerMessage( LogLevel.Critical, Message = "{caller}: Could not read file '{fileName}', message was '{mesg}'" ) ]
    public static partial void FileUnreadable(
        this ILogger logger,
        string fileName,
        string mesg,
        [ CallerMemberName ] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: File {path} not parsed, message was '{mesg}' ({lineNum} : {srcFile})")]
    public static partial void FileParsingError(this ILogger logger, string path, string mesg, [CallerMemberName] string caller = "", [CallerFilePath] string srcFile="", [CallerLineNumber] int lineNum = 0);

    [LoggerMessage(LogLevel.Error, "{caller}: Directory {path} not found")]
    public static partial void DirectoryNotFound(
        this ILogger logger,
        string path,
        [ CallerMemberName ] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: Unsupported file source type {type}")]
    public static partial void UnsupportedFileSourceType(
        this ILogger logger,
        Type type,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: Unsupported file scope '{scope}'")]
    public static partial void UnsupportedFileScope(
        this ILogger logger,
        string scope,
        [CallerMemberName] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: File '{filePath}' appears {count} times in the import configuration file")]
    public static partial void MultipleImportFileDescriptors(
        this ILogger logger,
        string filePath,
        int count,
        [ CallerMemberName ] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: Could not read header on '{fileName}'")]
    public static partial void HeaderUnreadable(
        this ILogger logger,
        string fileName,
        [ CallerMemberName ] string caller = ""
    );

    [LoggerMessage(LogLevel.Error, "{caller}: Duplicate column {colIdx} value read for record {curRecord} from '{filePath}'")]
    public static partial void DuplicateColumnRead(
        this ILogger logger,
        string filePath,
        int colIdx,
        int curRecord,
        [ CallerMemberName ] string caller = ""
    );
}
