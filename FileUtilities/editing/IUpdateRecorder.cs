using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.FileUtilities;

public interface IUpdateRecorder
{
    bool EntitySkipped( Type entityType, int keyValue, string? reason = null );
    bool EntityMerged( Type entityType, int originalKeyValue, int mergedIntoKeyValue, string? reason = null );

    bool PropertyValueChanged(
        Type entityType,
        int entityKey,
        string propName,
        ChangeSource source,
        string? originalValue,
        string? revisedValue,
        string? reason = null
    );

    bool SaveChanges();
}

// ReSharper disable once UnusedTypeParameter
public interface IUpdateRecorder<TDb> : IUpdateRecorder
    where TDb : DbContext;
