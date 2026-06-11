namespace AgriculturalMonitorSystem.Application.Constants;

public static class ErrorMessages
{
    // Auth
    public const string InvalidCredentials = "Invalid email or password.";
    public const string EmailAlreadyExists = "An account with this email already exists.";
    public const string UserNotFound = "User not found.";
    public const string InvalidToken = "Invalid or expired token.";
    public const string TokenExpired = "Reset token has expired.";
    public const string TokenAlreadyUsed = "Reset token has already been used.";
    public const string CurrentPasswordIncorrect = "Current password is incorrect.";

    // Farm
    public const string FarmNotFound = "Farm not found.";
    public const string FarmAccessDenied = "You do not have access to this farm.";
    public const string OnlyFarmersCanCreateFarms = "Only farmers can create farms.";

    // Sensor
    public const string SensorNotFound = "Sensor not found.";
    public const string SensorFarmMismatch = "Sensor does not belong to the specified farm.";

    // Alert
    public const string AlertNotFound = "Alert not found.";
    public const string AlertAlreadyResolved = "Alert is already resolved.";

    // Admin
    public const string UserHasFarms = "Cannot delete user. User has active farms. Delete farms first.";
    public const string LastAdminProtected = "Cannot delete or demote the last administrator.";
    public const string ValidationRangeNotFound = "Validation range not found.";
    public const string ValidationRangeAlreadyExists = "A validation range for this sensor type already exists.";

    // General
    public const string UnauthorizedAccess = "Authentication required.";
    public const string ForbiddenAccess = "You do not have permission to perform this action.";
    public const string InternalServerError = "An internal server error occurred.";
    public const string InvalidIdFormat = "Invalid ID format.";
}
