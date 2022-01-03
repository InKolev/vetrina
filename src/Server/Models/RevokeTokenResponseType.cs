namespace Vetrina.Server.Models
{
    public enum RevokeTokenResponseType
    {
        Successful = 1,
        UnexpectedError = 2,
        ValidationError = 3,
        Forbidden = 4
    }
}