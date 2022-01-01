using Vetrina.Autogen.API.Client.Contracts;

namespace Vetrina.Client.Services
{
    public class ShoppingListItem
    {
        public Promotion Promotion { get; set; }

        public int Quantity { get; set; }
    }
}