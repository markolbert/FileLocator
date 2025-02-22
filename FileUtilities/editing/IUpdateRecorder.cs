using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.FileUtilities;

public interface IUpdateRecorder
{
    string DbName { get; }
    bool IsConfigured { get; }

    bool EntitySkipped( Type entityType, int keyValue, string? reason = null );
    bool EntityMerged( Type entityType, int originalKeyValue, int mergedIntoKeyValue, string? reason = null );

    bool PropertyValueChanged<TEntity>(
        TEntity entity,
        string propName,
        ChangeSource source,
        string? originalValue,
        string? revisedValue,
        string? reason = null
    ) where TEntity: class;

    bool SaveChanges();
}

// ReSharper disable once UnusedTypeParameter
public interface IUpdateRecorder<TDb> : IUpdateRecorder
    where TDb : DbContext;
