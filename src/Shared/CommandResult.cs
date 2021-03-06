using System.Collections.Generic;
using FluentValidation.Results;

namespace Vetrina.Shared
{
    public class CommandResult : Result
    {
        public CommandResult(bool isSuccess) : base(isSuccess)
        {
        }

        public CommandResult(bool isSuccess, string message) : base(isSuccess, message)
        {
        }

        public CommandResult(IEnumerable<ValidationFailure> validationMessages) : base(validationMessages)
        {
        }
    }
}
