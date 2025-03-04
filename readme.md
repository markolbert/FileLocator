# File Locator

This library includes methods for searching directories for files, and ensuring, if necessary, that they can be written.

This library requires Net9 and has nullability enabled.

You can find the library's GitHub repository [here](https://github.com/markolbert/FileLocator).

The change log is available [here](docs/changes.md).

- [Overview](#overview)
- [Configuring via Fluent API](#fluent-api-configuration-methods)
- [Search methods](#search-methods)
- [Results](#results)

## Overview

When dealing with files, I sometimes want to allow "the" file to exist in one of several locations.
Consequently, finding "the file" requires searching various directories. `FileLocator` is intended
to simplify doing that.

You define the search parameters by using a fluent API applied to an instance of `FileLocator` (supplying
an `ILoggerFactory` instance is optional, but enables logging):

```c#
// define the directories to be searched
// there are other scan methods that use various default locations
var toSearch = dirToSearch.Select( x => Path.Combine( Environment.CurrentDirectory, x ) );
foundPath = Path.Combine( Environment.CurrentDirectory, foundPath );

var fileLoc = new FileLocator(_loggerFactory)
                .FileToFind( fileToFind )
                .Required()
                .ScanDirectories( toSearch );

fileLoc.Matches.Should().Be( matches );
fileLoc.FirstMatch.Should().NotBeNull();
fileLoc.FirstMatch!.Path.ToLower().Should().Be( foundPath.ToLower() );
```

After one of the scan methods is called, the `FileLocator` instances will contain whatever
files were found which match the criteria. The matches can be obtained by iterating the instance,
which returns a sequence of `PathInfo` objects describing the files that were found (see the details on `PathInfo` [below](#results))

[return to table of contents](#file-locator)

## Fluent API Configuration Methods

The fluent API methods for configuring how the search is done are:

|Method|Arguments|Comments|
|------|---------|--------|
|FileSystemIsCaseSensitive||makes the search case sensitive, overriding the default defined when `FileLocator` is created (which relies on `Environment.OSVersion.Platform`)|
|FileSystemIsNotCaseSensitive||makes the search case insensitive, overriding the default defined when `FileLocator` is created (which relies on `Environment.OSVersion.Platform`)|
|FileToFind|`string` searchPath|sets the file, optionally including a path, to be searched for. Paths can be rooted or not|
|StopOnFirstMatch||stops the search as soon as the first match is found. By default, searching continues through all possible locations|
|FindAllMatches||search through all possible locations (default)|
|FindAtMost|`int` maxMatches|stop searching after finding `maxMatches`|
|Exists||a file matching the search parameters must exist|
|Optional||search succeeds even if no matching file is found (default)|
|Readable||file must be readable by the current user to be a match|
|ReadabilityOptional||file need not be readable by the current user to be a match (default)|
|Writeable||file must be writeable by the current user to be a match. This is determined by attempting to create a dummy file in the directory where a matching file was found.|
|WriteabilityOptional||file need not be writeable by the current user (default)|

[return to table of contents](#file-locator)

## Search Methods

The search is performed by calling one of several scanning methods, most of which allow
an optional enumerable of secondary search paths to be included:

|Method|Arguments|Comments|
|------|---------|--------|
|ScanCurrentDirectory|`IEnumerable<string>?` secondaryPaths (*optional*)|scans `Environment.CurrentDirectory` plus any `secondaryPaths`|
|ScanExecutableDirectory|`IEnumerable<string>?` secondaryPaths (*optional*)|scans `AppDomain.CurrentDomain.BaseDirectory` plus any `secondaryPaths`|
|ScanDirectories|`IEnumerable<string>?` reqdPaths (**required**)|scans the supplied `reqdPaths`|
|ScanDirectory|`string` primaryPath (*required*)<br>`IEnumerable<string>?` secondaryPaths (*optional*)|scans the supplied `reqdPaths` plus any `secondaryPaths`|

[return to table of contents](#file-locator)

## Results

Searches generate an enumeration of matching files, which can be obtained by iterating the `FileLocator` instance used to do the search.

The values returned by the enumeration are `PathInfo` objects:

|Property|Type|Comments|
|--------|----|--------|
|Path|`string`|the absolute path to a matching file|
|State|`PathState`|a description of the matching file's characteristics|

`PathState` is a flags `Enum` that can contain one or more values:

```c#
[ Flags ]
public enum PathState
{
    Exists = 1 << 0,
    Readable = 1 << 1,
    Writeable = 1 << 2,

    None = 0,
    All = Exists | Readable | Writeable
}

```

[return to table of contents](#file-locator)
