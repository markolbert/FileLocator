using System.Linq.Expressions;

namespace J4JSoftware.FileUtilities;

public interface IWorksheetTableReader<TEntity, in TContext> : ITableReader<TEntity, TContext>
    where TEntity : class
    where TContext : WorksheetImportContext;
