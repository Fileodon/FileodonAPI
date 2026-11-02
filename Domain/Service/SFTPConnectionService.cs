using System;
using Domain.Interface;
using Infrastructure.Models;

namespace Domain.Service;

public class SFTPConnectionService(IUnitOfWork unitOfWork) : ISFTPConnectionService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<SFTPConnectionAuthCode> GenerateVerificationCodeAsync(int organizationId, string userId)
    {
        string code = GenerateVerificationCode(8);

        // Create a new SFTPConnectionAuthCode object
        var authCode = new SFTPConnectionAuthCode
        {
            OrganizationId = organizationId,
            UserId = userId,
            VerificationCode = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };

        await _unitOfWork.BeginTransactionAsync();
        await _unitOfWork.SFTPConnectionAuthCodeRepository.PostAsync(authCode);
        await _unitOfWork.CommitAsync();

        return await Task.FromResult(authCode);
    }

    private static string GenerateVerificationCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string([.. Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)])]);
    }
}
