# File Locator

This library includes methods for searching directories for files, and ensuring, if necessary, that they can be written.

This library requires Net9 and has nullability enabled.

You can find the library's GitHub repository [here]().

The change log is available [here](docs/changes.md).

## Dedication

This library is dedicated to the developers of the various NPOI projects, particularly the ones behind a [great C# implementation of it](https://github.com/nissl-lab/npoi), and the developers of the [CsvHelper](https://joshclose.github.io/CsvHelper) project. Without their tireless efforts to simplify reading and writing CSV and Excel files I could not have written this API.

## Table of Contents

- [Locating Files](docs/locating.md)
- [Importing data](docs/importing.md)
- [Exporting data](docs/exporting.md)

## Introduction

Each set of tabular data in Excel is assumed to exist in its own spreadsheet. There is also limited support for writing non-tabular Excel spreadsheets.

The reading process supports two different kinds of data correction during reading:

- modifying imported fields using an arbitrary set of field cleaners ("cleaning"); and,
- replacing field values based on a separate input file ("field replacment").

See [Data correction](#data-correction) below for details.

Writing Excel files is done using a fluent API that modifies an exporter object to define what tables/sheets are being exported, and their content and format.

Reading and creating Excel files is handled by the [NPOI package](https://github.com/nissl-lab/npoi), which was inspried by the [Apache POI project](https://poi.apache.org/). I greatly appreciate all the work both library's authors have done to make reading and writing Excel files in C# straightforward.

I would also like to thank the authors of [CsvHelper](https://joshclose.github.io/CsvHelper/), a C# library that I use extensively here to read CSV files.

## Data correction

Both field cleaning and field replacment depend on linking correction information to a particular record. *Consequently, all records subject to correction must have a way of being uniquely identified.*

Currently, the library only supports using `int` fields as unique identifying keys. This was a simplification I adopted because I needed to use the library in a project I was pursuing and supporting more general keys proved more difficult than I expected. I hope to support more general kinds of keys in a future update.
