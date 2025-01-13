using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.FileUtilities;

public interface IUpdateRecorder
{
    bool RecordSkipped( Type entityType, int keyValue, string? reason = null );
    bool RecordMerged( Type entityType, int originalKeyValue, int mergedIntoKeyValue, string? reason = null );

    bool FieldNullified(
        Type entityType,
        int recordKey,
        string fieldName,
        string? originalValue,
        string? reason = null
    );

    bool FieldRevised(
        Type entityType,
        int recordKey,
        string fieldName,
        string? originalValue,
        string? revisedValue,
        string? reason = null
    );

    bool SaveChanges();
}

// ReSharper disable once UnusedTypeParameter
public interface IUpdateRecorder<TDb> : IUpdateRecorder
    where TDb : DbContext;
