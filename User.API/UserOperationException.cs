namespace User.API;

class UserOperationException : Exception
{
    public UserOperationException(string message) : base(message)
    {
    }

    public UserOperationException(string message, Exception innerException) : base(message, innerException)
    {

    }
}