namespace J4JSoftware.FileUtilities;

public class DoubleStyle : NumericStyle
{
    private int _decPlaces;

    public int DecimalPlaces
    {
        get => _decPlaces;
        set => _decPlaces = value < 0 ? 0 : value;
    }

}

