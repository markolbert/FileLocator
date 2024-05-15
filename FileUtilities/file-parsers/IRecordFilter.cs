namespace J4JSoftware.FileUtilities;

public interface IRecordFilter
{
    bool Include( object? record );
}

public interface IRecordFilter<in TRaw> : IRecordFilter
    where TRaw : class
{
    bool Include( TRaw record );
}
