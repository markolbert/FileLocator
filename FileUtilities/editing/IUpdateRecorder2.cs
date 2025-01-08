using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.FileUtilities;

public interface IUpdateRecorder2
{
    bool RecordSkipped( Type entityType, int keyValue, string? reason );
    bool RecordMerged( Type entityType, int originalKeyValue, int mergedIntoKeyValue, string? reason );
    bool FieldNullified( Type entityType, int recordKey, string fieldName, string? originalValue, string? reason );

    bool FieldRevised(
        Type entityType,
        int recordKey,
        string fieldName,
        string? originalValue,
        string? revisedValue,
        string? reason
    );

    bool SaveChanges();
}

// ReSharper disable once UnusedTypeParameter
public interface IUpdateRecorder2<TDb> : IUpdateRecorder2
    where TDb : DbContext;
