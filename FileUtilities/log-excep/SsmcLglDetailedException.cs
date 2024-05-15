namespace J4JSoftware.FileUtilities;

public class SsmcLglDetailedException<TDetail>(
    Type type,
    string methodName,
    string mesg,
    TDetail detail,
    Exception? innerException = null
) : SsmcLglException( type, methodName, mesg, innerException )
    where TDetail : class;
