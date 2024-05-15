using System.Linq.Expressions;

namespace J4JSoftware.FileUtilities;

public interface IWorksheetTableReader<TEntity> : ITableReader, IEnumerable<TEntity>
    where TEntity : class, new()
{
    IWorkbookFileInfo? Source { get; set; }

     void AddMapping( int colNum, string colNameInSheet, Expression<Func<TEntity, double>> propExpr );
     void AddMapping( int colNum, string colNameInSheet, Expression<Func<TEntity, int>> propExpr );
     void AddMapping( int colNum, string colNameInSheet, Expression<Func<TEntity, bool>> propExpr );
     void AddMapping( int colNum, string colNameInSheet, Expression<Func<TEntity, DateTime>> propExpr );
     void AddMapping( int colNum, string colNameInSheet, Expression<Func<TEntity, string?>> propExpr );
}
