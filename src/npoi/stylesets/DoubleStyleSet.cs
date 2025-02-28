namespace J4JSoftware.FileUtilities;

public record DoubleStyleSet : NumericStyleSet
{
    public DoubleStyleSet(
        DoubleStyle config
    )
        : base( config )
    {
        DecimalPlaces = config.DecimalPlaces;
    }

    public int DecimalPlaces { get; init; }

    public override bool ValuesEqual( StyleSetBase other )
    {
        if( !base.ValuesEqual( other ) )
            return false;

        if( other is not DoubleStyleSet doubleOther )
            return false;

        return DecimalPlaces == doubleOther.DecimalPlaces
         && IsPercent == doubleOther.IsPercent;
    }

    protected override string GetDigitsSection() =>
        DecimalPlaces > 0
            ? $"{base.GetDigitsSection()}{DecimalSeparator}{new string( '0', DecimalPlaces )}"
            : base.GetDigitsSection();
}
