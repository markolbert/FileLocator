using System.Linq.Expressions;

namespace J4JSoftware.FileUtilities;

internal interface ITableCreatorInternal
{
    void SetFreezeColumn( int colNum );
    void AddTitle( string text, StyleSetBase styleSet );
    void AddNamedRangeScope( string rangeName, NamedRangeScope namedRange );
}

internal interface ITableCreatorInternal<TEntity> : ITableCreatorInternal
    where TEntity : class
{
    IExportableColumn<TEntity, string?> AddColumn( Expression<Func<TEntity, string?>> propExpr );
    IExportableColumn<TEntity, int> AddColumn( Expression<Func<TEntity, int>> propExpr );
    IExportableColumn<TEntity, double> AddColumn( Expression<Func<TEntity, double>> propExpr );
    IExportableColumn<TEntity, DateTime> AddColumn( Expression<Func<TEntity, DateTime>> propExpr );
    IExportableColumn<TEntity, DateTime?> AddColumn( Expression<Func<TEntity, DateTime?>> propExpr );
    IExportableColumn<TEntity, bool> AddColumn( Expression<Func<TEntity, bool>> propExpr );

    IExportableColumn<TEntity, double> AddVector( Expression<Func<TEntity, List<double>>> propExpr );
    IExportableColumn<TEntity, int> AddVector( Expression<Func<TEntity, List<int>>> propExpr );
    IExportableColumn<TEntity, string> AddVector( Expression<Func<TEntity, List<string>>> propExpr );
}
