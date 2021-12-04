using FluentValidation;
using Vetrina.Server.Mediatr.Commands;

namespace Vetrina.Server.Mediatr.CommandValidators
{
    public class ScrapeLidlPromotionsCommandValidator : AbstractValidator<ScrapeLidlPromotionsCommand>
    {
        public ScrapeLidlPromotionsCommandValidator()
        {
            // Pass through validator.
        }
    }
}
