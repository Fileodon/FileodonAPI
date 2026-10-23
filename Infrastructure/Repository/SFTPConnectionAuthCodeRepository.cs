using System;
using Infrastructure.Interface;
using Infrastructure.Interface.Repository;
using Infrastructure.Models;

namespace Infrastructure.Repository;

public class SFTPConnectionAuthCodeRepository(ApplicationDbContext context, IRepositoryTransactionSettings transactionSettings) : BaseRepository<SFTPConnectionAuthCode>(context, transactionSettings), ISFTPConnectionAuthCodeRepository
{
}
