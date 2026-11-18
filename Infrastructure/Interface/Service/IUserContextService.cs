namespace Infrastructure.Interface.Service;

public interface IUserContextService
{
    string? GetCurrentUserId();
    string? getUserAgent();
}