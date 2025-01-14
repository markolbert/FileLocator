using System.Linq.Expressions;
using J4JSoftware.Utilities;

namespace J4JSoftware.FileUtilities;

internal record Corrector<TEntity, TProp> : ICorrector<TEntity>
{
    private readonly Func<TEntity, int> _keyGetter;
    private readonly Func<TEntity, TProp?> _propGetter;
    private readonly Action<TEntity, TProp?> _propSetter;
    private readonly IUpdateRecorder? _updateRecorder;

    public Corrector(
        Func<TEntity, int> keyGetter,
        Expression<Func<TEntity, TProp?>> propGetter,
        IUpdateRecorder? updateRecorder
    )
    {
        var propInfo = propGetter.GetPropertyInfo();

        PropertyName = propInfo.Name;

        _keyGetter = keyGetter;
        _propGetter = propGetter.Compile();
        _propSetter = ( e, p ) => propInfo.SetValue( e, p );
        _updateRecorder = updateRecorder;
    }

    public string PropertyName { get; init; }

    public List<IPropertyAdjuster<TProp?>> Adjusters { get; } = [];

    public void CorrectEntity( TEntity entity )
    {
        var valueToAdjust = _propGetter( entity );

        var adjusted = false;

        foreach( var adjuster in Adjusters )
        {
            if( !adjuster.AdjustField( valueToAdjust, out var adjValue ) )
                continue;

            valueToAdjust = adjValue;
            adjusted = true;
        }

        if( !adjusted )
            return;

        _updateRecorder?.PropertyValueChanged( typeof( TEntity ),
                                               _keyGetter( entity ),
                                               PropertyName,
                                               ChangeSource.Rule,
                                               _propGetter( entity )?.ToString(),
                                               valueToAdjust?.ToString() );

        _propSetter( entity, valueToAdjust );
    }
}
