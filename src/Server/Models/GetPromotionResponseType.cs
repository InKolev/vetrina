namespace Vetrina.Server.Models
{
    public enum GetPromotionResponseType
    {
        Successful = 1,
        UnexpectedError = 2,
        ValidationError = 3,
        NotFound = 4
    }
}