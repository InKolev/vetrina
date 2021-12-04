namespace Vetrina.Shared.SearchModels
{
    public class SearchDocumentHit<T> where T : Promotion
    {
        public T Document { get; set; }

        public float Score { get; set; }
    }
}