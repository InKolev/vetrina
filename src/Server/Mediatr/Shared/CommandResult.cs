using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;

namespace Vetrina.Server.Mediatr.Shared
{
    public class CommandResult
    {
        public CommandResult(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }

        public CommandResult(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message;
        }

        public CommandResult(IEnumerable<ValidationFailure> validationMessages)
        {
            IsSuccess = false;
            ValidationMessages = validationMessages;
            Message = string.Join(";", validationMessages.Select(x => x.ErrorMessage));
        }

        public bool IsSuccess { get; private set; }

        public string Message { get; private set; }

        public IEnumerable<ValidationFailure> ValidationMessages { get; private set; } = new List<ValidationFailure>();
    }
}
