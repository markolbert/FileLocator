# File Utilities: Overview

This library simplifies the *reading* of CSV, JSON and Excel files and the *writing* of JSON files and Excel files which contain tabular data.

It also includes methods for searching directories for files, and ensuring, if necessary, that they can be written.

## Table of Contents

- Locating Files
- Importing data
  - Cleaning fields
  - Replacing fields
  - Reading JSON files
  - Reading CSV files
  - Reading Excel files
- Exporting data
  - Overview
  - Styles
  - Fluent API

## Introduction

Each set of tabular data in Excel is assumed to exist in its own spreadsheet. There is also limited support for writing non-tabular Excel spreadsheets.

The reading process supports two different kinds of data correction during reading:

- modifying imported fields using an arbitrary set of field cleaners ("cleaning"); and,
- replacing field values based on a separate input file ("field replacment").

See [Data correction](#data-correction) below for details.

Writing Excel files is done using a fluent API that modifies an exporter object to define what tables/sheets are being exported, and their content and format.

Reading and creating Excel files is handled by the [NPOI package](https://github.com/nissl-lab/npoi), which was inspried by the [Apache POI project](https://poi.apache.org/). I greatly appreciate all the work both library's authors have done to make reading and writing Excel files in C# straightforward.

I would also like to thank the authors of [CsvHelper](https://joshclose.github.io/CsvHelper/), a C# library that I use extensively here to read CSV files.

## Why This Level of Abstraction?

When you first read the documentation you may wonder about the level of abstraction involved. Aren't there simpler ways to import a CSV file or Excel tabular data?

In fact, there are...but the source material is very different. One's a simple file, the other is a set of spreadsheet cell values.

The abstraction results from the goal of trying to treat the two different sources of data in a common way.

## Data correction

Both field cleaning and field replacment depend on linking correction information to a particular record. *Consequently, all records subject to correction must have a way of being uniquely identified.*

Currently, the library only supports using `int` fields as unique identifying keys. This was a simplification I adopted because I needed to use the library in a project I was pursuing and supporting more general keys proved more difficult than I expected. I hope to support more general kinds of keys in a future update.
