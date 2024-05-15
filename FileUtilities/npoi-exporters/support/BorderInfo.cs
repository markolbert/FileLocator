using NPOI.SS.UserModel;

namespace J4JSoftware.FileUtilities;

public record BorderInfo( BorderStyle Top, BorderStyle Left, BorderStyle Bottom, BorderStyle Right )
{
    public BorderInfo()
        : this( BorderStyle.None, BorderStyle.None, BorderStyle.None, BorderStyle.None )
    {
    }

    private sealed class BorderInfoEqualityComparer : IEqualityComparer<BorderInfo>
    {
        public bool Equals( BorderInfo? x, BorderInfo? y )
        {
            if( ReferenceEquals( x, y ) )
                return true;
            if( ReferenceEquals( x, null ) )
                return false;
            if( ReferenceEquals( y, null ) )
                return false;
            if( x.GetType() != y.GetType() )
                return false;

            return x.Top == y.Top
             && x.Left == y.Left
             && x.Right == y.Right
             && x.Bottom == y.Bottom;
        }

        public int GetHashCode( BorderInfo obj )
        {
            return HashCode.Combine( (int) obj.Top, (int) obj.Left, (int) obj.Right, (int) obj.Bottom );
        }
    }

    public static IEqualityComparer<BorderInfo> BorderInfoComparer { get; } = new BorderInfoEqualityComparer();
}
