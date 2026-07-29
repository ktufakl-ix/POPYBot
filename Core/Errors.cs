namespace POPYBot;

public static class Errors
{
    public static readonly Dictionary<int, Type> HttpErrorDict = new()
    {
        [401] = typeof(AuthenticationFailedError),
        [403] = typeof(ForbiddenError),
        [404] = typeof(NotFoundError),
        [405] = typeof(MethodNotAllowedError),
        [429] = typeof(SequenceNumberError),
        [500] = typeof(ServerError),
        [504] = typeof(ServerError),
    };
}

public class AuthenticationFailedError : Exception
{
    public AuthenticationFailedError(string msg) : base(msg) { }
}

public class NotFoundError : Exception
{
    public NotFoundError(string msg) : base(msg) { }
}

public class MethodNotAllowedError : Exception
{
    public MethodNotAllowedError(string msg) : base(msg) { }
}

public class SequenceNumberError : Exception
{
    public SequenceNumberError(string msg) : base(msg) { }
}

public class ServerError : Exception
{
    public ServerError(string msg) : base(msg) { }
}

public class ForbiddenError : Exception
{
    public ForbiddenError(string msg) : base(msg) { }
}
