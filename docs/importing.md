# Importing Data

- [Why this API?](#why-this-api)
- [Key limitation](#key-limitation)
- [Defining the target entity](#defining-the-target-entity)
- Reading streams
  - [Reading CSV streams](#reading-csv-streams)
  - [Reading Excel files]()
- [Resolving Paths]()
- [Supporting Logic]()
- [JSON utilities]()

## Why this API?

You can read CSV, JSON and Excel files in a variety of ways, including [CSVHelper](https://joshclose.github.io/CsvHelper) and [NPOI](https://github.com/nissl-lab/npoi), each of which I use here. So why did I develop this API?

I had a task involving importing data in both CSV and tabular Excel spreadsheets from a source I did not control and that was used by a variety of data entry people, who didn't alwyas follow the same protocols when posting information. Consequently, the imported data had to be cleaned up before it was useful.

This API grew out of that need: to be able to treat CSV and tabular Excel data in the same way, and to be able to clean it up during import.

I quickly found I was using two kinds of cleanup techniques:

- correct what was imported by applying an algorithm to it (e.g., always replace 'Street' with 'St' to enforce consistency); and,
- replace field values that were determined to be incorrect.

Both types of correction can be used by this API, either separately or together.

## Key limitation

In order to determine which record should be updated when a field value replacement is made, the records have to have a uniquely identifying key.

The current verison of the API requires that this key field be an integer.

Relaxing that constraint (e.g., so a non-integer, or composite key) could be used turns out to be fairly involved. I may return to the task of implementing more complex unique keys in a future verison of the API.

## Defining the Target Entity

Central to importing and correcting data is defining the shape of the data being imported and corrected. The API uses plain-old classes to do this, with property attribute decorations supplying essential information.

There are two different mapping attributes, one each for the two different types of input streams:

|Input Stream Type|Mapping Attribute|Optional Converter Type|
|-----------------|-----------------|-----------------------|
|CSV|`CsvField`|if supplied, must implement `INpoiConverter`|
|workbook|`NpoiField`|if supplied, must implement `ITypeConverter` (see CsvHelper documentation for details)|

Both mapping attributes require you to specify the name of the CSV field or workbook data table column. They also allow you to specify a converter type, for converting the raw imported data to the type of the property it's mapped to.



Here's an example for a CSV import:

```c#
public class Campaign : IPropertyLengths
{
    [ CsvField( "LGL Campaign ID" ) ]
    public int Id { get; set; }

    [ CsvField("Name", typeof(CsvTrimmedTextConverter)) ]
    public string Name { get; set; } = null!;

    [ CsvField( "Code" ) ]
    public string? Code { get; set; }

    [ CsvField( "Description" ) ]
    public string? Description { get; set; }

    [ CsvField( "Goal" ) ]
    public string? Goal { get; set; }

    [ CsvField( "Start Date" ) ]
    public DateTime? StartDate { get; set; }

    [ CsvField( "End Date" ) ]
    public DateTime? EndDate { get; set; }

    [ CsvField( "Is Active?" ) ]
    public bool IsActive { get; set; }

    public ICollection<Appeal> Appeals { get; set; }
    public ICollection<Gift> Gifts { get; set; }
    public ICollection<Goal> Goals { get; set; }
    public ICollection<RelatedGift> RelatedGifts { get; set; }
}
```

This class is used in an EF Core database (hence the `ICollection` relationship properties at the end), but you can ignore that for purposes of this explanation.

For CSV files, you can also exclude properties from the import process by decorating the import class with a `CsvExcluded` attribute. It takes a list of property names, separated by commas. However, that generally shouldn't be necessary, since any property **not** decorated with a `CsvField` attribute is ignored.

## Reading streams

Once you've defined the target, importing the stream involves creating an instance of a `ITableReader<TEntity, in TContext>`. There are two classes to choose from, one for CSV streams and one for workbook streams:

|Stream type|Table Reader|
|-----------|------------|
|CSV|`CsvTableReader<TEntity>`|
|workbook|`WorkbookTableReader<TEntity, TContext>`|

In both cases, `TEntity` is the type you're importing into (i.e., the decorated target class described above).

`TContext` defines the type of import context to be passed to the data retrieval methods in the workbook reader. `TContext` must derive from `WorksheetImportContext` (see [Defining the import context](#defining-the-import-context), below).

No context type is needed for CSV imports because they always use the base `ImportContext` class.

Doing the import simply involves calling one of the data retrieval methods:

|Stream Type|Method|Style|Arguments|Returns|
|-----------|------|-----|---------|-------|
|CSV|`GetData`|synchronous|`ImportContext` context|`IEnumerable<TEntity>`
||`GetDataAsync`|asynchronous|`ImportContext` context,<br>`CancellationToken`ctx|`IAsyncEnumerable<TEntity>`|
|workbook|`GetData`|synchronous|`TContext` context|`IEnumerable<TEntity>`
||`GetDataAsync`|asynchronous|`TContext` context,<br>`CancellationToken`ctx|`IAsyncEnumerable<TEntity>`|

Note that due to the way the NPOI library is implemented, its `GetDataAsync` method does **not** actually retrieve data asynchronously. The retrieval is done _synchronously_, instead. The method exists to maintain a consistent API interface.

### Defining the import context

The import context defines where the imported data is coming from. The base class, `ImportContext` (which is used for CSV streams), looks like this:

```c#
public class ImportContext( params string[] fieldsToIgnore )
{
    public Stream? ImportStream { get; set; }

    public bool HasHeaders { get; set; }
    public string? ReplacementsPath { get; set; }
    public virtual string[] FieldsToIgnore { get; } = fieldsToIgnore;
}
```

`ImportStream` is normally set by assigning it a file stream, but it can be assigned a `StreamReader` instance instead if the CSV stream is, for example, coming from a web API call:

```c#
// file-based import
var importStream =
    File.OpenRead( Path.Combine( Configuration.Directories.ImportFolders.LglImportFolders.Current, FileName ) );

var context = new ImportContext
{
    HasHeaders = true,
    ImportStream = importStream,
    ReplacementsPath = string.IsNullOrWhiteSpace( TweaksFile )
        ? null
        : Path.Combine( Configuration.Directories.ImportFolders.TweaksFolder, TweaksFile )
};

// web-based import
var webImportContext = new ImportContext
{
    HasHeaders = true, 
    ImportStream = await httpClient.GetStreamAsync( url, ctx )
};
```
