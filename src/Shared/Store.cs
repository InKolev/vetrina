namespace Vetrina.Shared
{
    public enum Store
    {
        NotSet = 0,
        Kaufland = 1,
        Lidl = 2,
        Billa = 3
    }

    public static class StoreExtensions
    {
        public static bool IsLogoSquared(this Store store)
        {
            return store == Store.Kaufland;
        }
    }
}