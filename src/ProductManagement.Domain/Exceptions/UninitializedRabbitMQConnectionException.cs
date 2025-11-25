namespace ProductManagement.Domain.Exceptions;

public class UninitializedRabbitMQConnectionException : Exception
{
    public UninitializedRabbitMQConnectionException()
    {
    }

    public UninitializedRabbitMQConnectionException(string message)
        : base(message)
    {
    }
}