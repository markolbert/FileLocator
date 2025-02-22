namespace J4JSoftware.FileUtilities;

internal interface ICorrector<in TEntity>
{
    string PropertyName { get; }

    void CorrectEntity( TEntity entity );
}
