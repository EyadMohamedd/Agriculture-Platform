using FluentValidation;
using AgriculturalMonitorSystem.Src.Features.Auth.Models.DTOs;

namespace AgriculturalMonitorSystem.Src.Features.Auth.Validators;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches(@"[a-zA-Z]").WithMessage("Password must contain at least one letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number.");
    }
}
