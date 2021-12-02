namespace Vetrina.Shared.Models
{
    public enum GetUserByIdResponseType
    {
        Successful = 1,
        UnexpectedFailure = 2,
        ValidationError = 3,
        NotFound = 4
    }
}