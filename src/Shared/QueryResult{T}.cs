using System.Collections.Generic;
using FluentValidation.Results;

namespace Vetrina.Shared
{
    public class QueryResult<T> : Result<T>
    {
        public QueryResult(bool isSuccess) : base(isSuccess)
        {
        }

        public QueryResult(bool isSuccess, string message) : base(isSuccess, message)
        {
        }

        public QueryResult(bool isSuccess, string message, T data) : base(isSuccess, message, data)
        {
        }

        public QueryResult(IEnumerable<ValidationFailure> validationMessages) : base(validationMessages)
        {
        }
    }
}