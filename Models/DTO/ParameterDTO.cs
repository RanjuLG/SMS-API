using System.ComponentModel.DataAnnotations;

namespace SMS.Models.DTO
{
    // Legacy DTO for backward compatibility
    public class ParameterDTO
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }

    // New standardized error response format
    public class ErrorResponseDTO
    {
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
    }

    // Standard success response format
    public class SuccessResponseDTO
    {
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }
}
