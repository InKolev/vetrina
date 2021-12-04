using FluentValidation;
using Vetrina.Server.Mediatr.Commands;

namespace Vetrina.Server.Mediatr.CommandValidators
{
    public class ScrapeKauflandPromotionsCommandValidator : AbstractValidator<ScrapeKauflandPromotionsCommand>
    {
        public ScrapeKauflandPromotionsCommandValidator()
        {
            // Pass through validator.
        }
    }
}
