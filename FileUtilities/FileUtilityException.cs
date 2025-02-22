namespace J4JSoftware.FileUtilities;

public class FileUtilityException(
    Type type,
    string methodName,
    string mesg,
    Exception? innerException = null
) : Exception( $"{type.Assembly.GetName().Name}::{type.Name}::{methodName}(): {mesg}", innerException );
