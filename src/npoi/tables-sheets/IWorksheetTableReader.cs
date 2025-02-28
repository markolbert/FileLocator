using System.Linq.Expressions;

namespace J4JSoftware.FileUtilities;

public interface IWorksheetTableReader<TEntity, in TContext> : ITableReader<TEntity, TContext>
    where TEntity : class
    where TContext : WorksheetImportContext
{
    void AddMapping( int colNum, string colNameInSheet, Expression<Func<TEntity, double>> propExpr );
    void AddMapping( int colNum, string colNameInSheet, Expression<Func<TEntity, int>> propExpr );
    void AddMapping( int colNum, string colNameInSheet, Expression<Func<TEntity, bool>> propExpr );
    void AddMapping( int colNum, string colNameInSheet, Expression<Func<TEntity, DateTime>> propExpr );
    void AddMapping( int colNum, string colNameInSheet, Expression<Func<TEntity, string?>> propExpr );
}
