using Domain.DTO;
using Domain.Enum;
using Domain.Interface;
using Infrastructure.Interface;
using Infrastructure.Models;
using Infrastructure.Models.DTO;
using Infrastructure.Models.Enums;

namespace Domain.Service;

public class SFTPConnectionService : ISFTPConnectionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICryptoService _cryptoService;

    public SFTPConnectionService(IUnitOfWork unitOfWork, ICryptoService cryptoService)
    {
        _unitOfWork = unitOfWork;
        _cryptoService = cryptoService;
    }

    public async Task<MutationEnvelope<SFTPConnectionAuthCode>> GenerateVerificationCodeAsync(int organizationId, string userId)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            string verificationCode = GenerateVerificationCode(8);
            SFTPConnectionAuthCode? sftpAuthCode = await _unitOfWork.SFTPConnectionAuthCodeRepository.AddAuthCodeAsync(verificationCode, organizationId, userId);

            if (sftpAuthCode == null)
            {
                await _unitOfWork.RollbackAsync();
                return new MutationEnvelope<SFTPConnectionAuthCode>(MutationStatus.NotFound, null);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            return new MutationEnvelope<SFTPConnectionAuthCode>(MutationStatus.Success, sftpAuthCode);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<MutationEnvelope<SFTPConnection>> AddSFTPConnectionAsync(string authCode, int organizationId, string? displayName, string host, int? port)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            SFTPConnection sFTPConnection = new()
            {
                DisplayName = displayName,
                OrganizationId = organizationId,
                Host = host,
                Port = port,
                Status = SFTPConnectionStatus.Pending,
            };

            sFTPConnection = await _unitOfWork.SFTPConnectionRepository.AddSFTPConnectionAsync(sFTPConnection);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            return new MutationEnvelope<SFTPConnection>(MutationStatus.Success, sFTPConnection);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<MutationEnvelope<SFTPConnection>> UpdateSFTPConnectionStatusAsync(int sFTPConnectionId, int organizationId, string userId, SFTPConnectionStatus newStatus)
    {
        SFTPConnection? sFTPConnection = await _unitOfWork.SFTPConnectionRepository.GetSFTPConnectionByIdAsync(sFTPConnectionId, organizationId, userId);
        if (sFTPConnection == null)
        {
            return new MutationEnvelope<SFTPConnection>(MutationStatus.NotFound, null);
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
                    return new MutationEnvelope<SFTPConnection>(MutationStatus.Forbidden, sFTPConnection);
                default:
                    await _unitOfWork.RollbackAsync();
                    return new MutationEnvelope<SFTPConnection>(MutationStatus.Forbidden, null);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            return new MutationEnvelope<SFTPConnection>(MutationStatus.Success, sFTPConnection);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<MutationEnvelope<SFTPConnection>> PollSFTPConnectionStatusAsync(int sFTPConnectionId, int organizationId)
    {
        SFTPConnection? sftpConnectionStatus = await _unitOfWork.SFTPConnectionRepository.GetSFTPConnectionStatusByIdAsync(sFTPConnectionId, organizationId);

        if (sftpConnectionStatus == null)
        {
            return new MutationEnvelope<SFTPConnection>(MutationStatus.NotFound, default);
        }

        if (sftpConnectionStatus.PublicKey is not null)
        {
            byte[] decryptedKey = _cryptoService.Decrypt(Convert.FromBase64String(sftpConnectionStatus.PublicKey));
            string authorizedKey = _cryptoService.ConvertPublicKeyToOpenSsh(decryptedKey);
            sftpConnectionStatus.PublicKey = authorizedKey;
        }

        return new MutationEnvelope<SFTPConnection>(MutationStatus.Success, sftpConnectionStatus);
    }

    public async Task<MutationEnvelope<SFTPConnection>> GetSFTPConnectionAsync(int sFTPConnectionId, int organizationId, string userId)
    {
        SFTPConnection? sFTPConnection = await _unitOfWork.SFTPConnectionRepository.GetSFTPConnectionByIdAsync(sFTPConnectionId, organizationId, userId, noTracking: true);

        if (sFTPConnection == null)
        {
            return new MutationEnvelope<SFTPConnection>(MutationStatus.NotFound, null);
        }

        if (sFTPConnection.PublicKey is not null)
        {
            byte[] decryptedKey = _cryptoService.Decrypt(Convert.FromBase64String(sFTPConnection.PublicKey));
            string authorizedKey = _cryptoService.ConvertPublicKeyToOpenSsh(decryptedKey);
            sFTPConnection.PublicKey = authorizedKey;
        }

        return new MutationEnvelope<SFTPConnection>(MutationStatus.Success, sFTPConnection);
    }

    public async Task<IEnumerable<SFTPConnection>> GetOrganizationSFTPConnectionsAsync(int organizationId, string userId)
    {
        return await _unitOfWork.SFTPConnectionRepository.GetOrganizationSFTPConnectionsAsync(organizationId, userId);
    }

    public async Task<MutationEnvelope<SFTPConnection>> DeleteSFTPConnectionAsync(int sFTPConnectionId, int organizationId, string userId)
    {
        SFTPConnection? sFTPConnection = await _unitOfWork.SFTPConnectionRepository.GetSFTPConnectionByIdAsync(sFTPConnectionId, organizationId, userId);

        if (sFTPConnection == null)
        {
            return new MutationEnvelope<SFTPConnection>(MutationStatus.NotFound, null);
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            SFTPConnection? deletedConnection = await _unitOfWork.SFTPConnectionRepository.DeleteAsync(sFTPConnectionId);

            if (deletedConnection == null)
            {
                await _unitOfWork.RollbackAsync();
                return new MutationEnvelope<SFTPConnection>(MutationStatus.NotFound, null);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            return new MutationEnvelope<SFTPConnection>(MutationStatus.Success, deletedConnection);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<MutationEnvelope<SFTPConnection>> UpdateSFTPConnectionAsync(int sFTPConnectionId, int organizationId, string userId, string displayName)
    {
        SFTPConnection? sFTPConnection = await _unitOfWork.SFTPConnectionRepository.GetSFTPConnectionByIdAsync(sFTPConnectionId, organizationId, userId);

        if (sFTPConnection == null)
        {
            return new MutationEnvelope<SFTPConnection>(MutationStatus.NotFound, null);
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            sFTPConnection.DisplayName = displayName;
            SFTPConnection updatedConnection = await _unitOfWork.SFTPConnectionRepository.UpdateAsync(sFTPConnection);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            return new MutationEnvelope<SFTPConnection>(MutationStatus.Success, updatedConnection);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<MutationEnvelope<SFTPConnectionAuthCode>> GetSFTPConnectionAuthCodeAsync(string authCode)
    {
        SFTPConnectionAuthCode? sFTPConnectionAuthCode = await _unitOfWork.SFTPConnectionAuthCodeRepository.GetAuthCodeAsync(authCode);

        if (sFTPConnectionAuthCode == null)
        {
            return new MutationEnvelope<SFTPConnectionAuthCode>(MutationStatus.NotFound, null);
        }

        return new MutationEnvelope<SFTPConnectionAuthCode>(MutationStatus.Success, sFTPConnectionAuthCode);
    }

    private static string GenerateVerificationCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string([.. Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)])]);
    }
}
