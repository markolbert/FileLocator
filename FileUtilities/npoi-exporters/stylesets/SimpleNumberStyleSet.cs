using System.Text;

namespace J4JSoftware.FileUtilities;

public record SimpleNumberStyleSet : FormatCodeStyleSet
{
    public SimpleNumberStyleSet(
        SimpleNumberStyle config
    )
        : base( config.StyleName )
    {
        Clauses = config.Clauses;

        var sb = new StringBuilder();

        for( var idx = 0; idx < 4 && idx < Clauses.Count; idx++ )
        {
            if( sb.Length > 0 )
                sb.Append( ";" );

            var clause = Clauses[ idx ];
            if( clause[ ^1 ] == ';' )
                clause = clause[ ..^1 ];

            sb.Append( clause );
        }

        FormatText = sb.ToString();
    }

    public List<string> Clauses { get; init; }
}
