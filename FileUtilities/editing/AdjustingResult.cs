namespace J4JSoftware.FileUtilities;

public record AdjustingResult( Type EntityType, string KeyField, int Key, string Field );

public record AdjustingSucceeded( Type EntityType, string KeyField, int Key, string Field, string? PriorValue, string? RevisedValue )
    : AdjustingResult( EntityType, KeyField, Key, Field );

public record AdjustingFailed(EditingFailure Failure, Type EntityType, string KeyField, int Key, string Field )
    : AdjustingResult( EntityType, KeyField, Key, Field );