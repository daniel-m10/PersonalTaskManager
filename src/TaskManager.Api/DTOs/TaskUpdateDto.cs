using TaskManager.Core.Enums;

namespace TaskManager.Api.DTOs
{
    public record TaskUpdateDto(
        string Title,
        string? Description,
        Status Status,
        Priority Priority,
        DateTime? DueDate,
        DateTime? CompletedAt,
        DateTime UpdatedAt);
}
