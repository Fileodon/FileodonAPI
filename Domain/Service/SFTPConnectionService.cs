using System;
using Domain.DTO;
using Domain.Enum;
using Domain.Interface;
using Infrastructure.Models;
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

            RSAKeyPairResult keyPairResult = _cryptoService.CreateRSAKeyPair();

            byte[] encryptedPrivateKey = _cryptoService.Encrypt(keyPairResult.PrivateKey);
            byte[] encryptedPublicKey = _cryptoService.Encrypt(keyPairResult.PublicKey);

            SFTPConnection sFTPConnection = new()
            {
                DisplayName = displayName,
                OrganizationId = sFTPConnectionAuthCode.OrganizationId,
                Host = host,
                Port = port,
                Status = SFTPConnectionStatus.Pending,
                PublicKey = Convert.ToBase64String(encryptedPublicKey),
                PrivateKey = Convert.ToBase64String(encryptedPrivateKey)
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

    private static string GenerateVerificationCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string([.. Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)])]);
    }
}
