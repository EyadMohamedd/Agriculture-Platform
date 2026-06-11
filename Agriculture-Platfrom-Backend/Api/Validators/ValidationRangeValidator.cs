using FluentValidation;
using AgriculturalMonitorSystem.Api.DTOs;

namespace AgriculturalMonitorSystem.Api.Validators;

public class ValidationRangeValidator : AbstractValidator<ValidationRangeDto>
{
    private static readonly string[] ValidTypes =
        ["temperature", "ph", "moisture", "npk_n", "npk_p", "npk_k", "rainfall"];

    public ValidationRangeValidator()
    {
        RuleFor(x => x.SensorType)
            .NotEmpty().WithMessage("Sensor type is required.")
            .Must(t => t != null && ValidTypes.Contains(t.ToLowerInvariant()))
            .WithMessage($"Sensor type must be one of: {string.Join(", ", ValidTypes)}");

        RuleFor(x => x.CriticalLow)
            .LessThanOrEqualTo(x => x.WarningLow).WithMessage("CriticalLow must be <= WarningLow.");
        RuleFor(x => x.WarningLow)
            .LessThanOrEqualTo(x => x.MinNormal).WithMessage("WarningLow must be <= MinNormal.");
        RuleFor(x => x.MinNormal)
            .LessThanOrEqualTo(x => x.MaxNormal).WithMessage("MinNormal must be <= MaxNormal.");
        RuleFor(x => x.MaxNormal)
            .LessThanOrEqualTo(x => x.WarningHigh).WithMessage("MaxNormal must be <= WarningHigh.");
        RuleFor(x => x.WarningHigh)
            .LessThanOrEqualTo(x => x.CriticalHigh).WithMessage("WarningHigh must be <= CriticalHigh.");
    }
}
