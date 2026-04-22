namespace AgriculturalMonitorSystem.Src.Shared.Exceptions;

public class ConflictException : AppException
{
    public ConflictException(string message = "Conflict.") : base(message, 409) { }
}
