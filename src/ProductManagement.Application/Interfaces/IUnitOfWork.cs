namespace ProductManagement.Application.Interfaces;

public interface IUnitOfWork
{
    Task SaveChangesAsync();
}