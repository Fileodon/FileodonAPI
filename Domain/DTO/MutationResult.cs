using Domain.Enum;

namespace Domain.DTO;

public sealed record MutationResult<T>(
    MutationStatus Status,
    T? Data);
