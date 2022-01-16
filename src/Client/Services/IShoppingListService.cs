using System.Threading.Tasks;
using Vetrina.Autogen.API.Client.Contracts;

namespace Vetrina.Client.Services;

public interface IShoppingListService
{
    Task AddPromotionalItemToShoppingList(Promotion promotion);
        
    Task IncreaseQuantityInShoppingList(Promotion promotion);

    Task DecreaseQuantityInShoppingList(Promotion promotion);
        
    Task RemoveFromShoppingList(Promotion promotion);
        
    Task ClearShoppingList();
}