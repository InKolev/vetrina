using System.Collections.Generic;
using FluentValidation.Results;

namespace Vetrina.Shared
{
    public class QueryResult : Result
    {
        public QueryResult(bool isSuccess) : base(isSuccess)
        {
        }

        public QueryResult(bool isSuccess, string message) : base(isSuccess, message)
        {
        }

        public QueryResult(IEnumerable<ValidationFailure> validationMessages) : base(validationMessages)
        {
        }
    }
}