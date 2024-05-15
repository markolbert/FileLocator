using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public record FieldUpdater<TEntity, TSrcProp> : MultiFieldUpdater<TEntity, TSrcProp, TSrcProp>
    where TEntity : class
{
    public FieldUpdater(
        Expression<Func<TEntity, int>> keyPropExpr,
        Expression<Func<TEntity, TSrcProp>> srcPropExpr,
        IUpdateRecorder updateRecorder,
        bool isCleaner,
        ILoggerFactory? loggerFactory
    )
        : base( keyPropExpr, srcPropExpr, srcPropExpr, updateRecorder, isCleaner, loggerFactory )
    {
    }
}
