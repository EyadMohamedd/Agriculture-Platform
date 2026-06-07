using FluentValidation;
using AgriculturalMonitorSystem.Src.Features.Alert.Models.DTOs;

namespace AgriculturalMonitorSystem.Src.Features.Alert.Validators;

public class ResolveAlertValidator : AbstractValidator<ResolveAlertDto>
{
    public ResolveAlertValidator()
    {
        When(x => x.Notes != null, () =>
        {
            RuleFor(x => x.Notes!)
                .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.");
        });
    }
}
