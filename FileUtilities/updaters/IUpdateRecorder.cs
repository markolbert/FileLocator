namespace J4JSoftware.FileUtilities;

public interface IUpdateRecorder
{
    bool RecordChange( Type entityType, int keyValue, string fieldName, string? priorValue, string? newValue );

    bool MarkAsInvalid( Type entityType, int keyValue, string fieldName, string? fieldValue );

    bool SaveChanges();
}
