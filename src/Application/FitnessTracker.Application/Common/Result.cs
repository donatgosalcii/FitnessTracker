namespace FitnessTracker.Application.Common
{
    public enum ErrorType
    {
        None = 0,
        Validation = 1,
        NotFound = 2,
        Unauthorized = 3,
        Conflict = 4, 
        Failure = 5, 
        Unexpected = 6 
    }

    public class Error
    {
        public string Code { get; }
        public string Message { get; }

        public Error(string message, string code = "")
        {
            Message = message;
            Code = code;
        }
    }

    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error? Error { get; } 
        public ErrorType ErrorType { get; }

        protected Result(bool isSuccess, Error? error, ErrorType errorType = ErrorType.None)
        {
            if (isSuccess && error != null)
            {
                throw new InvalidOperationException("A successful result cannot contain an error.");
            }
            if (!isSuccess && error == null)
            {
                throw new InvalidOperationException("A failed result must contain an error.");
            }

            IsSuccess = isSuccess;
            Error = error;
            ErrorType = isSuccess ? ErrorType.None : (errorType == ErrorType.None ? ErrorType.Failure : errorType);
        }

        public static Result Success() => new Result(true, null);

        public static Result Failure(Error error, ErrorType errorType = ErrorType.Failure) => 
            new Result(false, error, errorType);
        
        public static Result Failure(string errorMessage, ErrorType errorType = ErrorType.Failure, string errorCode = "") => 
            new Result(false, new Error(errorMessage, errorCode), errorType);


        public static Result NotFound(string message = "Resource not found.", string code = "") => 
            Failure(new Error(message, code), ErrorType.NotFound);

        public static Result ValidationFailed(string message = "Validation failed.", string code = "") => 
            Failure(new Error(message, code), ErrorType.Validation);
        
        public static Result Unauthorized(string message = "Unauthorized access.", string code = "") =>
            Failure(new Error(message, code), ErrorType.Unauthorized);

        public static Result Conflict(string message = "A conflict occurred.", string code = "") =>
            Failure(new Error(message, code), ErrorType.Conflict);
            
        public static Result Unexpected(string message = "An unexpected error occurred.", string code = "") =>
            Failure(new Error(message, code), ErrorType.Unexpected);
    }

    public class Result<T> : Result
    {
        private readonly T? _value;

        public T Value => IsSuccess && _value != null 
            ? _value 
            : throw new InvalidOperationException("Cannot access the value of a failed result or a null success value.");

        public T? GetValueOrDefault() => _value;


        protected Result(T? value, bool isSuccess, Error? error, ErrorType errorType = ErrorType.None)
            : base(isSuccess, error, errorType)
        {
            if (isSuccess && value == null && typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) == null)
            {
            }
            _value = value;
        }

        public static Result<T> Success(T value) => 
            new Result<T>(value, true, null);


        public static new Result<T> Failure(Error error, ErrorType errorType = ErrorType.Failure) =>
            new Result<T>(default, false, error, errorType);

        public static new Result<T> Failure(string errorMessage, ErrorType errorType = ErrorType.Failure, string errorCode = "") =>
            new Result<T>(default, false, new Error(errorMessage, errorCode), errorType);
        
        public static new Result<T> NotFound(string message = "Resource not found.", string code = "") =>
            Failure(new Error(message, code), ErrorType.NotFound);

        public static new Result<T> ValidationFailed(string message = "Validation failed.", string code = "") =>
            Failure(new Error(message, code), ErrorType.Validation);
        
        public static new Result<T> Unauthorized(string message = "Unauthorized access.", string code = "") =>
            Failure(new Error(message, code), ErrorType.Unauthorized);

        public static new Result<T> Conflict(string message = "A conflict occurred.", string code = "") =>
            Failure(new Error(message, code), ErrorType.Conflict);
        
        public static new Result<T> Unexpected(string message = "An unexpected error occurred.", string code = "") =>
            Failure(new Error(message, code), ErrorType.Unexpected);


        public static implicit operator Result<T>(T value) => Success(value);
    }
}