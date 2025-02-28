namespace J4JSoftware.FileUtilities;

public enum AggregateFunction
{
    None,

    [ AggregateFunctionUsage( "sum", "Total", typeof( int ), typeof( double ) ) ]
    Sum,

    [ AggregateFunctionUsage( "count", "Count" ) ]
    Count,

    [ AggregateFunctionUsage( "average", "Average", typeof( int ), typeof( double ) ) ]
    Average,

    [ AggregateFunctionUsage( "min", "Minimum", typeof( int ), typeof( double ), typeof( DateTime ) ) ]
    Minimum,

    [ AggregateFunctionUsage( "max", "Maximum", typeof( int ), typeof( double ), typeof( DateTime ) ) ]
    Maximum,

    [ AggregateFunctionUsage( "stdevp", "Std Dev Pop", typeof( int ), typeof( double ) ) ]
    StandardDeviation
}
