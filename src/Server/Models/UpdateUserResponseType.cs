namespace Vetrina.Server.Models
{
    public enum UpdateUserResponseType
    {
        Successful = 1,
        UnexpectedFailure = 2,
        ValidationError = 3,
        NotFound = 4
    }
}