# File Utilities: Overview

This library simplifies the *reading* of CSV, JSON and Excel files and the *writing* of JSON files and Excel files which contain tabular data. 

Each set of tabular data in Excel is assumed to exist in its own spreadsheet. There is also limited support for writing non-tabular Excel spreadsheets.

It also supports resolving file names by searching across multiple directories.

I've found that sometimes I need to modify (tweak) data after I've imported it, because I don't always control what gets put into the file I'm importing. The library supports tweaking based on data sourced from a separate file whose contents you control. There's a generalized system for updating the values of imported fields, which depends on each imported record being defined by a unique key field.

## Why This Level of Abstraction?

When you first read the documentation you may wonder about the level of abstraction involved. Aren't there simpler ways to import a CSV file or Excel tabular data? In fact, there are...but  the source material is very different. One's a simple file, the other is a set of spreadsheet cell values. The abstraction results from the goal of trying to treat the two different sources of data in a common way.

[Importing Data](importing/#file-utilities-importing-data)
[Exporting Data to Excel]()
