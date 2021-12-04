using System.Collections.Generic;
using FluentValidation.Results;

namespace Vetrina.Server.Mediatr.Shared
{
    public class CommandResult<T> : CommandResult
    {
        public CommandResult(bool isSuccess)
            : base(isSuccess)
        {

        }

        public CommandResult(bool isSuccess, string message)
            : base(isSuccess, message)
        {
        }

        public CommandResult(bool isSuccess, string message, T data)
            : base(isSuccess, message)
        {
            Data = data;
        }

        public CommandResult(IEnumerable<ValidationFailure> validationMessages)
            : base(validationMessages)
        {
        }

        public T Data { get; private set; }
    }
}