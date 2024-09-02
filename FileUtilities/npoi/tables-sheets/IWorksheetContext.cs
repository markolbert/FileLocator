﻿namespace J4JSoftware.FileUtilities;

public interface IWorksheetContext : IFileContext
{
    string SheetName { get; }
}

public interface IWorksheetTableSource : ITableSource, IWorksheetContext
{
}