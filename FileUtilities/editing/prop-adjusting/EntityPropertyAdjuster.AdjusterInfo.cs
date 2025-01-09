using System.Collections.ObjectModel;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.FileUtilities;

public partial class EntityCorrector<TEntity>
{
    private record FilteredAdjuster( IPropertyAdjuster PropertyAdjuster, IEntityFilter? EntityFilter );

    private record AdjusterInfo
    {
        private readonly ILogger? _logger;
        private readonly Func<object, object?> _getter;
        private readonly Action<object, object?> _setter;

        public AdjusterInfo(
            PropertyInfo propInfo,
            ILoggerFactory? loggerFactory
        )
        {
            PropertyName = propInfo.Name;
            PropertyType = propInfo.PropertyType;
            _logger = loggerFactory?.CreateLogger<AdjusterInfo>();

            _getter = propInfo.GetValue;
            _setter = propInfo.SetValue;
        }

        public string PropertyName { get; }
        public Type PropertyType { get; }
        public List<FilteredAdjuster> Adjusters { get; } = [];

        public bool TryGetPropertyValue( object entity, out object? value )
        {
            value = null;

            try
            {
                value = _getter( entity );
                return true;
            }
            catch( Exception ex )
            {
                _logger?.CouldNotGetValue( entity.GetType(), PropertyName, PropertyType, ex.Message );
                return false;
            }
        }

        public bool TrySetPropertyValue( object entity, object? value )
        {
            try
            {
                _setter( entity, value );
                return true;
            }
            catch( Exception ex )
            {
                _logger?.CouldNotSetValue( entity.GetType(),
                                           PropertyName,
                                           PropertyType,
                                           value?.ToString() ?? string.Empty,
                                           ex.Message );
                return false;
            }
        }
    }

    private class PropertyAdjusters() : KeyedCollection<string, AdjusterInfo>(StringComparer.Ordinal)
    {
        protected override string GetKeyForItem( AdjusterInfo item ) => item.PropertyName;
    }
}
