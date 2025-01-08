using MathNet.Numerics.Statistics.Mcmc;

namespace J4JSoftware.FileUtilities;

public interface IEntityPropertyAdjuster
{
    Type EntityType { get; }

    bool AdjustEntity( object entity );
    void SaveAdjustmentInfo();
}

public interface IEntityPropertyAdjuster<in TEntity> : IEntityPropertyAdjuster
    where TEntity : class
{
    bool AdjustEntity( TEntity entity );
}
