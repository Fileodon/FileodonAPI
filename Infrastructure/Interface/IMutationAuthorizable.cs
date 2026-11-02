namespace Infrastructure.Interface;

public interface IMutationAuthorizable
{
    int Id { get; }
    string? UserId { get; }
}
