using Domain.Enum;

namespace Domain.DTO;

public sealed record MutationEnvelope<T>(
    MutationStatus Status,
    T? Data);
