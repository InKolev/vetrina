namespace Vetrina.Shared.Request
{
    public class SearchPromotionsRequest
    {
        /// <summary>
        /// The search text coming from the user.
        /// </summary>
        public string SearchTerm { get; set; }

        /// <summary>
        /// Max number of documents returned by the API.
        /// Default value is 24.
        /// </summary>
        public int MaxNumberOfDocs { get; set; } = 48;

        /// <summary>
        /// Specifies if promotions from Kaufland store should be returned.
        /// If none of the flags is enabled - the API ignores this filter and returns documents from all stores.
        /// </summary>
        public bool IncludeKaufland { get; set; }

        /// <summary>
        /// Specifies if promotions from Lidl store should be returned.
        /// If none of the Include* flags is enabled - the API ignores this filter and returns documents from all stores.
        /// </summary>
        public bool IncludeLidl { get; set; }

        /// <summary>
        /// Specifies if promotions from Billa store should be returned.
        /// If none of the Include* flags is enabled - the API ignores this filter and returns documents from all stores.
        /// </summary>
        public bool IncludeBilla { get; set; }

        /// <summary>
        /// Specifies if promotions from Fantastiko store should be returned.
        /// If none of the Include* flags is enabled - the API ignores this filter and returns documents from all stores.
        /// </summary>
        public bool IncludeFantastiko { get; set; }
    }
}
