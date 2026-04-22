namespace AgriculturalMonitorSystem.Src.Shared.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public object? Errors { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Operation completed successfully")
        => new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> FailureResponse(string message, object? errors = null)
        => new() { Success = false, Message = message, Errors = errors };
}

public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
    public object? Errors { get; set; }

    public static ApiResponse SuccessResponse(string message = "Operation completed successfully", object? data = null)
        => new() { Success = true, Message = message, Data = data };

    public static ApiResponse FailureResponse(string message, object? errors = null)
        => new() { Success = false, Message = message, Errors = errors };
}
