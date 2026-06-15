namespace MultiShop.WebUI.Services.NewsletterServices
{
    public class NewsletterStatusDto
    {
        public bool IsSubscribed { get; set; }
        public string Email { get; set; }
    }

    public enum NewsletterCallOutcome
    {
        Ok,
        Reauthenticate,
        Error
    }

    public class NewsletterCallResult
    {
        public NewsletterCallOutcome Outcome { get; set; }
        public NewsletterStatusDto Status { get; set; }
    }

    public interface INewsletterService
    {
        Task<NewsletterCallResult> GetStatusAsync();
        Task<NewsletterCallOutcome> SubscribeAsync();
        Task<NewsletterCallOutcome> UnsubscribeAsync();
    }
}
