namespace TaskManager.Core.Common
{
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public T? Value { get; }
        public IReadOnlyList<string> Errors { get; }

        private Result(bool success, T? value, List<string> errors)
        {
            IsSuccess = success;
            Value = value;
            Errors = errors;
        }

        public static Result<T> Success(T value) => new(true, value, []);
        public static Result<T> Failure(string error) => new(false, default, [error]);
        public static Result<T> Failure(params string[] errors) => new(false, default, [.. errors]);
        public static Result<T> Failure(List<string> errors) => new(false, default, errors);
    }
}
