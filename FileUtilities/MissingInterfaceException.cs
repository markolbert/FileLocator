namespace J4JSoftware.FileUtilities;

public class MissingInterfaceException(
    Type checkedType,
    Type reqdInterfaceType
) : Exception( $"{checkedType} does not implement the required {reqdInterfaceType} interface" );

public class MissingConstructorException(
    Type checkedType,
    string message
) : Exception( $"{checkedType} {message}" );
