using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;

namespace Vetrina.Shared
{
    public abstract class Result
    {
        protected Result(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }

        protected Result(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message;
        }

        protected Result(IEnumerable<ValidationFailure> validationMessages)
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