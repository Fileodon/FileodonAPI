using Infrastructure.Models;

namespace Infrastructure.Interface.Repository;

public interface IRefreshTokenRepository : IBaseRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, bool includeUser = false);
    Task AddAsync(RefreshToken refreshToken);
}
