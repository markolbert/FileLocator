using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public record FieldCleaner<TEntity, TSrcProp> : FieldToFieldCleaner<TEntity, TSrcProp, TSrcProp>
    where TEntity : class
{
    public FieldCleaner(
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
