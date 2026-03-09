using System.Collections.Generic;

namespace InventorySales.Application.DTOs.Common
{
    // operations that do not return data
    public class Result
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string>? Errors { get; set; }

        public static Result Success(string message = "") => new Result { IsSuccess = true, Message = message };
        public static Result Failure(string message, List<string>? errors = null) => new Result { IsSuccess = false, Message = message, Errors = errors };
    }

    // data return processes
    public class Result<T> : Result
    {
        public T? Data { get; set; }

        public static Result<T> Success(T data, string message = "") => new Result<T> { IsSuccess = true, Data = data, Message = message };
        public new static Result<T> Failure(string message, List<string>? errors = null) => new Result<T> { IsSuccess = false, Message = message, Errors = errors };
    }
}