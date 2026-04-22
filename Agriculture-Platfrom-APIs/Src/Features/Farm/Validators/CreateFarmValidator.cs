using FluentValidation;
using AgriculturalMonitorSystem.Src.Features.Farm.Models.DTOs;

namespace AgriculturalMonitorSystem.Src.Features.Farm.Validators;

public class CreateFarmValidator : AbstractValidator<CreateFarmDto>
{
    public CreateFarmValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Farm name is required.")
            .MaximumLength(200).WithMessage("Farm name cannot exceed 200 characters.");

        When(x => x.Location != null, () =>
        {
            RuleFor(x => x.Location!.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90.");
            RuleFor(x => x.Location!.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180.");
        });
    }
}
