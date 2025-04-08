namespace InsurtixTask.Core.Exceptions;

public class BookExistsException : Exception
{
    public BookExistsException(string message) : base(message) { }
}