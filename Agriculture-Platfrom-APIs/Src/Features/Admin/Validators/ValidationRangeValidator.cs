using FluentValidation;
using AgriculturalMonitorSystem.Src.Features.Admin.Models.DTOs;

namespace AgriculturalMonitorSystem.Src.Features.Admin.Validators;

public class ValidationRangeValidator : AbstractValidator<ValidationRangeDto>
{
    private static readonly string[] ValidTypes =
        ["temperature", "ph", "moisture", "npk_n", "npk_p", "npk_k", "rainfall"];

    public ValidationRangeValidator()
    {
        RuleFor(x => x.SensorType)
            .NotEmpty().WithMessage("Sensor type is required.")
            .Must(t => ValidTypes.Contains(t))
            .WithMessage($"Sensor type must be one of: {string.Join(", ", ValidTypes)}");

        RuleFor(x => x.CriticalLow)
            .LessThan(x => x.WarningLow).WithMessage("CriticalLow must be less than WarningLow.");
        RuleFor(x => x.WarningLow)
            .LessThan(x => x.MinNormal).WithMessage("WarningLow must be less than MinNormal.");
        RuleFor(x => x.MinNormal)
            .LessThan(x => x.MaxNormal).WithMessage("MinNormal must be less than MaxNormal.");
        RuleFor(x => x.MaxNormal)
            .LessThan(x => x.WarningHigh).WithMessage("MaxNormal must be less than WarningHigh.");
        RuleFor(x => x.WarningHigh)
            .LessThan(x => x.CriticalHigh).WithMessage("WarningHigh must be less than CriticalHigh.");
    }
}
