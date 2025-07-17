namespace ThesisBackend.Services.UserService.Models;

public class UserOperationResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } // Could also be List<string> for multiple errors
    public Domain.Messages.UserResponse UserResponse { get; set; }
}

public class AllUserOperationResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } // Could also be List<string> for multiple errors
    public List<Domain.Messages.UserResponse> UserResponse { get; set; }
}

public class UserEventOperationResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } // Could also be List<string> for multiple errors
    public List<Domain.Messages.SmallEventResponse>? SmallEventResponse { get; set; }
}
