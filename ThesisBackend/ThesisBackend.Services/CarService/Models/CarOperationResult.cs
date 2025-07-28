namespace ThesisBackend.Services.CarService.Models;

public class CarOperationResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; } // Could also be List<string> for multiple errors
    public Domain.Messages.CarResponse CarResponse { get; set; }
}

public class AllCarOperationResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; } // Could also be List<string> for multiple errors
    public List<Domain.Messages.CarResponse> CarResponses { get; set; }
}