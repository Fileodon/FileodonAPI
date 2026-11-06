using Domain.DTO;
using Domain.Enum;
using Domain.Interface;
using Infrastructure.Interface;
using Infrastructure.Models;
using Infrastructure.Models.DTO;
using Infrastructure.Models.Enums;

namespace Domain.Service;

public class SFTPConnectionService(
    IUnitOfWork unitOfWork,
    ICryptoService cryptoService) : ISFTPConnectionService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICryptoService _cryptoService = cryptoService;

    public async Task<MutationResult<SFTPConnectionAuthCode>> GenerateVerificationCodeAsync(int organizationId, string userId)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            string verificationCode = GenerateVerificationCode(8);
            SFTPConnectionAuthCode? sftpAuthCode = await _unitOfWork.SFTPConnectionAuthCodeRepository.AddAuthCodeAsync(verificationCode, organizationId, userId);

            if (sftpAuthCode == null)
            {
                await _unitOfWork.RollbackAsync();
                return new MutationResult<SFTPConnectionAuthCode>(MutationStatus.NotFound, null);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            return new MutationResult<SFTPConnectionAuthCode>(MutationStatus.Success, sftpAuthCode);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<MutationResult<SFTPConnection>> AddSFTPConnectionAsync(string authCode, int organizationId, string displayName, string host, int port)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            SFTPConnectionAuthCode? sFTPConnectionAuthCode = await _unitOfWork.SFTPConnectionAuthCodeRepository.GetAuthCodeAsync(authCode, organizationId);

            if (sFTPConnectionAuthCode == null)
            {
                await _unitOfWork.RollbackAsync();
                return new MutationResult<SFTPConnection>(MutationStatus.NotFound, null);
            }

            if (sFTPConnectionAuthCode.ExpiresAt < DateTime.UtcNow)
            {
                await _unitOfWork.RollbackAsync();
                return new MutationResult<SFTPConnection>(MutationStatus.Forbidden, null);
            }

            SFTPConnection sFTPConnection = new()
            {
                DisplayName = displayName,
                OrganizationId = sFTPConnectionAuthCode.OrganizationId,
                Host = host,
                Port = port,
                Status = SFTPConnectionStatus.Pending,
            };

            sFTPConnection = await _unitOfWork.SFTPConnectionRepository.AddSFTPConnectionAsync(sFTPConnection);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            return new MutationResult<SFTPConnection>(MutationStatus.Success, sFTPConnection);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<MutationResult<SFTPConnection>> UpdateSFTPConnectionStatusAsync(int sFTPConnectionId, int organizationId, string userId, SFTPConnectionStatus newStatus)
    {
        SFTPConnection? sFTPConnection = await _unitOfWork.SFTPConnectionRepository.GetSFTPConnectionByIdAsync(sFTPConnectionId, organizationId, userId);
        if (sFTPConnection == null)
        {
            return new MutationResult<SFTPConnection>(MutationStatus.NotFound, null);
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            switch (newStatus)
            {
                case SFTPConnectionStatus.Accepted:
                    RSAKeyPairResult keyPairResult = _cryptoService.CreateRSAKeyPair();
                    sFTPConnection.PublicKey = Convert.ToBase64String(_cryptoService.Encrypt(keyPairResult.PublicKey));
                    sFTPConnection.PrivateKey = Convert.ToBase64String(_cryptoService.Encrypt(keyPairResult.PrivateKey));
                    sFTPConnection = _unitOfWork.SFTPConnectionRepository.AcceptSFTPConnection(sFTPConnection);
                    break;
                case SFTPConnectionStatus.Rejected:
                    if (sFTPConnection.PublicKey is not null)
                    {
                        sFTPConnection.PublicKey = null;
                    }
                    if (sFTPConnection.PrivateKey is not null)
                    {
                        sFTPConnection.PrivateKey = null;
                    }
                    sFTPConnection = _unitOfWork.SFTPConnectionRepository.RejectSFTPConnection(sFTPConnection);
                    break;
                case SFTPConnectionStatus.Pending:
                    await _unitOfWork.RollbackAsync();
                    return new MutationResult<SFTPConnection>(MutationStatus.Forbidden, sFTPConnection);
                default:
                    await _unitOfWork.RollbackAsync();
                    return new MutationResult<SFTPConnection>(MutationStatus.Forbidden, null);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            return new MutationResult<SFTPConnection>(MutationStatus.Success, sFTPConnection);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<MutationResult<SFTPConnection>> PollSFTPConnectionStatusAsync(int sFTPConnectionId, int organizationId, string userId)
    {
        SFTPConnection? sftpConnectionStatus = await _unitOfWork.SFTPConnectionRepository.GetSFTPConnectionStatusByIdAsync(sFTPConnectionId, organizationId, userId);

        if (sftpConnectionStatus == null)
        {
            return new MutationResult<SFTPConnection>(MutationStatus.NotFound, default);
        }

        return new MutationResult<SFTPConnection>(MutationStatus.Success, sftpConnectionStatus);
    }

    public async Task<MutationResult<SFTPConnection>> GetSFTPConnectionAsync(int sFTPConnectionId, int organizationId, string userId)
    {
        SFTPConnection? sFTPConnection = await _unitOfWork.SFTPConnectionRepository.GetSFTPConnectionByIdAsync(sFTPConnectionId, organizationId, userId, noTracking: true);

        if (sFTPConnection == null)
        {
            return new MutationResult<SFTPConnection>(MutationStatus.NotFound, null);
        }

        return new MutationResult<SFTPConnection>(MutationStatus.Success, sFTPConnection);
    }

    public async Task<IEnumerable<SFTPConnection>> GetOrganizationSFTPConnectionsAsync(int organizationId, string userId)
    {
        return await _unitOfWork.SFTPConnectionRepository.GetOrganizationSFTPConnectionsAsync(organizationId, userId);
    }

    public async Task<MutationResult<SFTPConnection>> DeleteSFTPConnectionAsync(int sFTPConnectionId, int organizationId, string userId)
    {
        SFTPConnection? sFTPConnection = await _unitOfWork.SFTPConnectionRepository.GetSFTPConnectionByIdAsync(sFTPConnectionId, organizationId, userId);

        if (sFTPConnection == null)
        {
            return new MutationResult<SFTPConnection>(MutationStatus.NotFound, null);
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            SFTPConnection? deletedConnection = await _unitOfWork.SFTPConnectionRepository.DeleteAsync(sFTPConnectionId);

            if (deletedConnection == null)
            {
                await _unitOfWork.RollbackAsync();
                return new MutationResult<SFTPConnection>(MutationStatus.NotFound, null);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            return new MutationResult<SFTPConnection>(MutationStatus.Success, deletedConnection);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<MutationResult<SFTPConnection>> UpdateSFTPConnectionAsync(int sFTPConnectionId, int organizationId, string userId, string displayName)
    {
        SFTPConnection? sFTPConnection = await _unitOfWork.SFTPConnectionRepository.GetSFTPConnectionByIdAsync(sFTPConnectionId, organizationId, userId);

        if (sFTPConnection == null)
        {
            return new MutationResult<SFTPConnection>(MutationStatus.NotFound, null);
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            sFTPConnection.DisplayName = displayName;
            SFTPConnection updatedConnection = await _unitOfWork.SFTPConnectionRepository.UpdateAsync(sFTPConnection);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            return new MutationResult<SFTPConnection>(MutationStatus.Success, updatedConnection);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    private static string GenerateVerificationCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string([.. Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)])]);
    }
}
