using TaskManager.Core.Enums;

namespace TaskManager.Api.DTOs
{
    public record TaskCreateDto(
        string Title,
        string? Description,
        Status Status,
        Priority Priority,
        DateTime? DueDate);
}
