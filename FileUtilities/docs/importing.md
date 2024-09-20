# File Utilities: Importing Data

The basic outline for importing data using this assembly is:

- locate the file to import;
- define the importer to use;
- import the data

## Table of Contents

- [Resolving files](#resolving-files)
- [Defining the target entity](#defining-the-target-entity)
- [Reading CSV files]()
- [Reading Excel files]()
- [Resolving Paths]()
- [Supporting Logic]()

## Resolving files

To read a file you have to find it. You can specify files in a fully-specified way, but sometimes it's more convenient to specify just a file name and a set of directories where the file should be located.

The library supports that kind of searching. The process is built around the concept of **file contexts**. At its most basic, an `IFileContext` defines both a **path** to a file and a **scope** within which the file exists:

```csharp
public interface IFileContext
{
    string FilePath { get; set; }
    string Scope { get; }
}
```

The file path may or not be fully-defined (i.e., it might be relative, or simply the name of a file which exists in some directory).

Scope defines the set of directories within which the file may exist. You can have multiple scopes defined in a project, each defined by a unique name. Mapping a scope to a set of directories is done by implementing the `IDirectoryResolver` interface in your project:

```csharp
public interface IDirectoryResolver
{
    List<string> GetDirectories( string scope );
}
```

Resolving a file name to a particular file is defined by the `IFileResolver` interface:

```csharp
public interface IFileResolver
{
    bool TryResolveFile(
        IFileContext source,
        out List<string> filePaths,
        bool searchSubDir = false,
        FileResolution resolution = FileResolution.SingleFile
    );
}
```

A default implementation of `IFileResolver` is provided by the `FileResolver` class. By default, it treats finding multiple matching file names in the directories it searches as an error (i.e., it assumes the file name is unique within its scope), but you can override that behavior by specifying `FileResolution.SingleFile` for the `resolution` parameter. Not finding a match is always an error.

## 

## Defining the Target Entity

By itself a file context isn't terribly useful, because while it describes how to find a file to read, it doesn't describe the entities you want to create when you read the tabular data (which would be a single line in a CSV file, or a single row in Excel tabular data).

The target entity information is defined by the `ITableSource` interface, which extends `IFileContext`:

```csharp
public interface ITableSource : IFileContext
{
    Type EntityType { get; }
    string FileType { get; }

    string? TweakPath { get; set; }
}
```

`TweakPath` is the path to a file -- which may or may not need to be resolved to find the actual file -- that contains the data needed to adjust values for specified imported records. We'll come back to tweaking in the section [Tweaking imported data]().

Mating a table source and a file context is defined in an interface unique to a given file format, either CSV or Excel. There are two such interfaces defined in the assembly:

|Source File Type|Table Source Interface|Parent Interfaces|
|----------------|----------------------|-----------------|
|CSV file|`ICSVTableSource`|`ITableSource`, `ICSVContext`|
|Excel file|`IWorksheetTableSource`|`ITableSource`, `IWorksheetContext`|

`ICSVContext` and `IWorksheetContext` are described below, in the sections about reading CSV and Excel files.

## Reading CSV Files

For CSV files `IFileContext` is extended by `ICSVContext` to define whether or not the CSV file has a header:

```csharp
public interface ICsvContext : IFileContext
{
    bool HasHeader { get; }
}
```
