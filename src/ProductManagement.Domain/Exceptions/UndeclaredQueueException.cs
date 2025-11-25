namespace ProductManagement.Domain.Exceptions;

public class UndeclaredQueueException : Exception
{
    public UndeclaredQueueException()
    {
    }

    public UndeclaredQueueException(string topicName)
        : base($"Cannot find declared queue for topic '{topicName}'.")
    {
    }
}