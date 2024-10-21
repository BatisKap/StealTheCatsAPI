using FluentValidation;
using StealTheCatsAPI.Application.Models;
using System;

namespace StealTheCatsAPI.Application.Validations
{
    public class CatValidator : AbstractValidator<Cat>
    {
        public CatValidator()
        {
            RuleFor(cat => cat.CatId)
                .NotEmpty().WithMessage("CatId is required.")
                .MaximumLength(50).WithMessage("CatId cannot be longer than 50 characters.");

            RuleFor(cat => cat.Width)
                .InclusiveBetween(100, 9000).WithMessage("Width must be between 100 and 9000.");

            RuleFor(cat => cat.Height)
                .InclusiveBetween(100, 9000).WithMessage("Height must be between 100 and 9000.");

            RuleFor(cat => cat.Image)
                .NotNull().WithMessage("Image is required.")
                .Must(image => image.Length > 0).WithMessage("Image cannot be empty.");

            RuleFor(cat => cat.Created)
                .NotEmpty().WithMessage("Created date is required.");
        }
    }
}
