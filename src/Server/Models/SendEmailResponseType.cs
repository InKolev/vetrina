namespace Vetrina.Server.Models
{
    public enum SendEmailResponseType
    {
        Sent = 1,
        ScheduledForSending = 2,
        Failed = 3,
        Bounced = 4,
    }
}