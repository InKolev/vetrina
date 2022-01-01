using System.Collections.Generic;
using FluentValidation.Results;

namespace Vetrina.Shared
{
    public class CommandResult<T> : Result<T>
    {
        public CommandResult(bool isSuccess) : base(isSuccess)
        {
        }

        public CommandResult(bool isSuccess, string message) : base(isSuccess, message)
        {
        }

        public CommandResult(bool isSuccess, string message, T data) : base(isSuccess, message, data)
        {
        }

        public CommandResult(IEnumerable<ValidationFailure> validationMessages) : base(validationMessages)
        {
        }
    }
}