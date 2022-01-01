using System.Collections.Generic;
using FluentValidation.Results;

namespace Vetrina.Shared
{
    public class Result<T> : Result
    {
        protected Result(bool isSuccess)
            : base(isSuccess)
        {

        }

        protected Result(bool isSuccess, string message)
            : base(isSuccess, message)
        {
        }

        protected Result(bool isSuccess, string message, T data)
            : base(isSuccess, message)
        {
            Data = data;
        }

        protected Result(IEnumerable<ValidationFailure> validationMessages)
            : base(validationMessages)
        {
        }

        public T Data { get; private set; }
    }
}
