namespace J4JSoftware.FileUtilities;

public class DataRecord( int recNum, List<string> headers )
{
    private readonly Dictionary<int, FieldValue> _fieldValues = [];

    public int RecordNumber { get; } = recNum;

    public bool AddValue( int fieldNum, string value )
    {
        if( _fieldValues.TryGetValue( fieldNum, out _ ) )
            return false;

        _fieldValues.Add( fieldNum, new FieldValue( fieldNum, value ) );

        return true;
    }

    public bool TryGetValue( int fieldNum, out string? value )
    {
        value = null;

        if( !_fieldValues.TryGetValue( fieldNum, out var retVal ) )
            return false;

        value = retVal.Value;
        return true;
    }

    public bool TrySetValue( int fieldNum, string value )
    {
        if( !_fieldValues.Remove( fieldNum, out _ ) )
            return false;

        _fieldValues.Add( fieldNum, new FieldValue( fieldNum, value ) );

        return true;
    }

    public string this[ int idx ] => _fieldValues[ idx ].Value;

    public string this[ string fieldName ]
    {
        get
        {
            var fieldIndex = headers.FindIndex( x => x.Equals( fieldName, StringComparison.OrdinalIgnoreCase ) );

            return this[ fieldIndex ];
        }
    }
}
