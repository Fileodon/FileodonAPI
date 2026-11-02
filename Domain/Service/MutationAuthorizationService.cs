using Domain.DTO;
using Domain.Enum;
using Domain.Interface;
using Infrastructure.Interface;

namespace Domain.Service;

public class MutationAuthorizationService<TEntity> : IMutationAuthorizationService<TEntity>
    where TEntity : IMutationAuthorizable
{
    public async Task<MutationResult<TEntity>> AuthorizeMutationAsync(
        Func<Task<TEntity?>> action,
        string userId)
    {
        ArgumentNullException.ThrowIfNull(action);

        TEntity? mutationResult = await action();

        if (mutationResult == null)
        {
            return new MutationResult<TEntity>(MutationStatus.NotFound, default);
        }

        if (!string.Equals(
                mutationResult.UserId,
                userId,
                StringComparison.OrdinalIgnoreCase))
        {
            return new MutationResult<TEntity>(MutationStatus.Forbidden, default);
        }

        return new MutationResult<TEntity>(MutationStatus.Success, mutationResult);
    }
}
