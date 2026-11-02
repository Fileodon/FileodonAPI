using System;
using Domain.DTO;
using Domain.Enum;
using Domain.Interface;
using Infrastructure.Models;

namespace Domain.Service;

public class SFTPConnectionService(IUnitOfWork unitOfWork, IMutationAuthorizationService<SFTPConnectionAuthCode> mutationAuthorizationService) : ISFTPConnectionService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMutationAuthorizationService<SFTPConnectionAuthCode> _mutationAuthorizationService = mutationAuthorizationService;

    public async Task<MutationResult<SFTPConnectionAuthCode>> GenerateVerificationCodeAsync(int organizationId, string userId)
    {
        string verificationCode = GenerateVerificationCode(8);
        MutationResult<SFTPConnectionAuthCode> result = await _mutationAuthorizationService.AuthorizeMutationAsync(
            () => _unitOfWork.SFTPConnectionAuthCodeRepository.AddAuthCodeAsync(verificationCode, organizationId, userId),
            userId
        );

        return result;
    }

    private static string GenerateVerificationCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string([.. Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)])]);
    }
}
