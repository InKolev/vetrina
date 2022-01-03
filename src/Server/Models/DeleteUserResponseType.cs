namespace Vetrina.Server.Models
{
    public enum DeleteUserResponseType
    {
        Successful = 1,
        UnexpectedError = 2,
        ValidationError = 3,
        NotFound = 4
    }
}