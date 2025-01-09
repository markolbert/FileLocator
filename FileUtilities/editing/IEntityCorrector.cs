using MathNet.Numerics.Statistics.Mcmc;

namespace J4JSoftware.FileUtilities;

public interface IEntityCorrector
{
    Type EntityType { get; }

    bool AdjustEntity( object entity );
    void SaveAdjustmentInfo();
}

public interface IEntityCorrector<in TEntity> : IEntityCorrector
    where TEntity : class
{
    bool AdjustEntity( TEntity entity );
}