using FluentValidation;
using Vetrina.Server.Mediatr.Commands;

namespace Vetrina.Server.Mediatr.CommandValidators;

public class ScrapeBillaPromotionsCommandValidator : AbstractValidator<ScrapeBillaPromotionsCommand>
{
    public ScrapeBillaPromotionsCommandValidator()
    {
        // Pass through validator.
    }
}