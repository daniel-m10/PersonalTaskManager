using TaskManager.Core.Enums;

namespace TaskManager.Api.DTOs
{
    public record TaskResponseDto(
        Guid Id,
        string Title,
        string? Description,
        Status Status,
        Priority Priority,
        DateTime CreatedAt,
        DateTime? DueDate,
        DateTime? CompletedAt,
        DateTime? UpdatedAt,
        bool IsDeleted);
}
