using Domain.DTO;
using Infrastructure.Interface;

namespace Domain.Interface;

public interface IMutationAuthorizationService<TEntity>
    where TEntity : IMutationAuthorizable
{
    Task<MutationResult<TEntity>> AuthorizeMutationAsync(
        Func<Task<TEntity?>> action,
        string userId);
}
