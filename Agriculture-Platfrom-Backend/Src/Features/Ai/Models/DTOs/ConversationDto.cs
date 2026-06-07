namespace AgriculturalMonitorSystem.Src.Features.Ai.Models.DTOs;

public class ConversationDto
{
    public string Id { get; set; } = string.Empty;
    public string FarmId { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public AiSensorDataDto ContextSnapshot { get; set; } = new();
    public DateTime Timestamp { get; set; }
}
