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
    IMutationAuthorizationService<SFTPConnectionAuthCode> mutationAuthorizationService,
    ICryptoService cryptoService) : ISFTPConnectionService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMutationAuthorizationService<SFTPConnectionAuthCode> _mutationAuthorizationService = mutationAuthorizationService;
    private readonly ICryptoService _cryptoService = cryptoService;

    public async Task<MutationResult<SFTPConnectionAuthCode>> GenerateVerificationCodeAsync(int organizationId, string userId)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            string verificationCode = GenerateVerificationCode(8);
            MutationResult<SFTPConnectionAuthCode> result = await _mutationAuthorizationService.AuthorizeMutationAsync(
                () => _unitOfWork.SFTPConnectionAuthCodeRepository.AddAuthCodeAsync(verificationCode, organizationId, userId),
                userId
            );

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            return result;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<MutationResult<SFTPConnection>> AddSFTPConnectionAsync(string authCode, string userId, string displayName, string host, int port)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            SFTPConnectionAuthCode? sFTPConnectionAuthCode = await _unitOfWork.SFTPConnectionAuthCodeRepository.GetAuthCodeAsync(authCode, userId);

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

    public async Task<MutationResult<SFTPConnection>> UpdateSFTPConnectionStatusAsync(int sFTPConnectionId, string userId, SFTPConnectionStatus newStatus)
    {
        SFTPConnection? sFTPConnection = await _unitOfWork.SFTPConnectionRepository.GetSFTPConnectionByIdAsync(sFTPConnectionId, userId);
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
                    return new MutationResult<SFTPConnection>(MutationStatus.Forbidden, null);
                default:
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

    public async Task<MutationResult<SFTPConnectionStatus>> PollSFTPConnectionStatusAsync(int sFTPConnectionId, string userId)
    {
        SFTPConnectionStatus? status = await _unitOfWork.SFTPConnectionRepository.GetSFTPConnectionStatusByIdAsync(sFTPConnectionId, userId);

        if (status == null)
        {
            return new MutationResult<SFTPConnectionStatus>(MutationStatus.NotFound, default);
        }

        return new MutationResult<SFTPConnectionStatus>(MutationStatus.Success, status.Value);
    }

    public async Task<MutationResult<SFTPConnection>> GetSFTPConnectionAsync(int sFTPConnectionId, string userId)
    {
        SFTPConnection? sFTPConnection = await _unitOfWork.SFTPConnectionRepository.GetSFTPConnectionByIdAsync(sFTPConnectionId, userId, noTracking: true);

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

    private static string GenerateVerificationCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string([.. Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)])]);
    }
}
