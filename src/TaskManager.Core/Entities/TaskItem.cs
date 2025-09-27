using TaskManager.Core.Enums;

namespace TaskManager.Core.Entities
{
    public class TaskItem
    {
        #region Required Fields
        public required Guid Id { get; init; }
        public required string Title { get; set; }
        public required Status Status { get; set; }
        public required Priority Priority { get; set; }
        public required DateTime CreatedAt { get; init; }
        #endregion

        #region Optional Fields
        public string Description { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        #endregion
    }
}
