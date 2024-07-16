namespace Tables.Runtime
{
    public interface IValidator<T>
    {
        ValidationResult Validate(ColumnMetadata column, T data, object value);
    }

    public readonly struct ValidationResult
    {
        public readonly Type ResultType;
        public readonly string Message;

        private ValidationResult(Type type, string message)
        {
            ResultType = type;
            Message = message;
        }

        public static ValidationResult Success() => new(Type.Success, string.Empty);
        public static ValidationResult Warning(string message) => new(Type.Warning, message);
        public static ValidationResult Error(string message) => new(Type.Error, message);

        public enum Type
        {
            Success,
            Warning,
            Error,
        }
    }
}
