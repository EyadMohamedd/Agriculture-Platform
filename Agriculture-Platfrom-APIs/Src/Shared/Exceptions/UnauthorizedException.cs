namespace AgriculturalMonitorSystem.Src.Shared.Exceptions;

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Unauthorized.") : base(message, 401) { }
}
