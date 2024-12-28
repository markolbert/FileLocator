# File Utilities: Field Cleaning

## Overview

Cleaning imported data is quite common, and usually done by applying hard-coded rules to the fields being cleaned.

While you can use this library and still do your cleaning that way, it also provides another way: defining a set of field cleaners that are applied to records as they are created but before they are returned to the calling code. In a sense, this provides a type of inline cleaning during import.

You configure cleaning by associating one or more `IFieldCleaner` objects with an `IEntityCleaner` object. Each `IEntityCleaner` object is associated with a particular type of record entity.

Cleaning a record/entity is then a matter of calling the `IEntityCleaner.CleanFields()` method on the record/entity instance.

## Field cleaners

The library defines two different types of cleaners:

- `FieldToFieldCleaner`, used to clean a field based on the value of another field; and, 
- `FieldCleaner`, which is used to clean a field based solely on its value.

In my experience, so far at least, `FieldCleaner`s are far more common than `FieldToFieldCleaner`s.

You don't create a cleaner directly. Instead, you call the protected `AddFieldCleaner()` method on the `IEntityCleaner` object you're defining for a particular record/entity type. You do this in the `EntityCleaner<TEntity>` constructor. Here's an example using one of those rare (for me) `FieldToFieldCleaner`s:

```cs
public PhoneCleaner(
    IUpdateRecorder<ImportDb> updateRecorder,
    IPhoneUtilities utilities,
    ILoggerFactory? loggerFactory
)
    : base( updateRecorder, null, loggerFactory )
{
    _utilities = utilities;

    AddFieldCleaner( p => p.PhoneId, x => x.Number, x => x.PhoneType, CorrectPhoneType );
    AddFieldCleaner( p => p.PhoneId, x => x.Number, FormatPhoneNumber );
}
```

The `FieldToFieldCleaner` `AddFieldCleaner<TSrcProp, TTgtProp>()` method's signature is as follows:

|Argument|Type|Comments|
|--------|----|--------|
|keyExpr|`Expression<Func<TEntity, int>>`|selects the unique integer key property in the entity|
|srcPropExpr|`Expression<Func<TEntity, TTgtProp>>`|selects the source property, the one whose value controls how the field to be cleaned is changed|
|tgtPropExpr|`Expression<Func<TEntity, TTgtProp>>`|selects the field to be cleaned|
|cleaners|`params Action<IFieldCleaner, IUpdateRecorder, TEntity>[]`|one or more cleaning methods; these can be private|

Cleaners can be as simple or as complex as necessary. Here's the definition of the `FormatPhoneNumber` cleaner shown in the example above:

```cs
private void FormatPhoneNumber( 
    IFieldCleaner fieldCleaner, 
    IUpdateRecorder updateRecorder, 
    Phone entity )
{
    var modified = _utilities.ConvertToValidNumber( entity );

    if( string.IsNullOrEmpty( modified ) )
        return;

    if( entity.Number!.Equals( modified, StringComparison.OrdinalIgnoreCase ) )
        return;

    updateRecorder.RecordChange( typeof( Phone ),
                                    entity.PhoneId,
                                    nameof( Phone.Number ),
                                    entity.Number,
                                    modified );

    entity.Number = modified;
}
```

Each cleaner method must accept the following arguments, and only those arguments:

|Argument|Type|Comments|
|--------|----|--------|
|fieldCleaner|`IFieldCleaner`|the field cleaner object managing a field's cleaning|
|IUpdateRecorder|`IUpdateRecorder`|an object which records the changes made by the cleaner|
|entity|`TEntity`|the record/entity Type being cleaned|

