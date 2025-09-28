namespace TaskManager.Services.Validators.Constants
{
    public static partial class ValidationMessages
    {
        public const string TitleRequired = "Title is required.";
        public const string TitleLength = "Title cannot exceed 100 characters.";
        public const string InvalidStatus = "Invalid status value.";
        public const string InvalidPriority = "Invalid priority value.";
        public const string CreatedAtRequired = "CreatedAt is required.";
        public const string DescriptionLength = "Description cannot exceed 500 characters.";
        public const string DueDateNotInPast = "Due date cannot be in the past.";
        public const string CompletedAtAfterCreatedAt = "CompletedAt must be after CreatedAt.";
        public const string UpdatedAtAfterCreatedAt = "UpdatedAt must be after CreatedAt.";
    }
}
