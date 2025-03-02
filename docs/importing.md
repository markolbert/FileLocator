# Importing Data

- [Why this API?](#why-this-api)
- [Key limitation](#key-limitation)
- [Defining the target entity](#defining-the-target-entity)
- [Reading CSV files]()
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

Central to importing and correcting data is defining the shape of the data being imported and corrected. The API uses plain-old classes to do this, with property attribute decorations supplying essential information. Here's an example:

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

A `CsvField` attribute maps the property to a particular field within a CSV stream. It must include the name of the field the property is bound to, and may include an optional `Type` value defining a class that will convert raw text read from a CSV file to a field value. That later capability is often necessary because `CSVHelper` makes  assumptions about parsing text values that may need to be overridden in a specific situation.

For CSV files, you can also exclude properties from the import process by decorating the import class with a `CsvExcluded` attribute. It takes a list of property names, separated by commas. However, that generally shouldn't be necessary, since any property **not** decorated with a `CsvField` attribute is ignored.

 