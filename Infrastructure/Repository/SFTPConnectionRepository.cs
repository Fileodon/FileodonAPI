using Infrastructure.Interface.Repository;
using Infrastructure.Models;

namespace Infrastructure.Repository;

public class SFTPConnectionRepository(ApplicationDbContext context) : BaseRepository<SFTPConnection>(context), ISFTPConnectionRepository
{
}