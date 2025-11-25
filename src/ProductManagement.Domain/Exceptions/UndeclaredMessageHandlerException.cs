namespace ProductManagement.Domain.Exceptions;

public class UndeclaredMessageHandlerException : Exception
{
    public UndeclaredMessageHandlerException()
    {
    }

    public UndeclaredMessageHandlerException(string topicName)
        : base($"Cannot find handler for messages from '{topicName}' topic.")
    {
    }
}