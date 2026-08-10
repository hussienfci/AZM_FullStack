using FluentValidation;

namespace MoviePlatform.Modules.Catalog.Application.Commands.CreateMovie;

public class CreateMovieCommandValidator : AbstractValidator<CreateMovieCommand>
{
    public CreateMovieCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.ReleaseYear)
            .GreaterThanOrEqualTo(1900)
            .LessThanOrEqualTo(2100)
            .WithMessage("Release year must be between 1900 and 2100.");

        RuleFor(x => x.DurationMinutes)
            .GreaterThanOrEqualTo(1)
            .LessThanOrEqualTo(500)
            .When(x => x.DurationMinutes.HasValue)
            .WithMessage("Duration must be between 1 and 500 minutes.");

        RuleFor(x => x.GenreIds)
            .NotEmpty().WithMessage("At least one genre is required.");
    }
}
