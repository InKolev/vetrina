using System.Collections.Generic;
using System.Threading.Tasks;
using Vetrina.Autogen.API.Client.Contracts;

namespace Vetrina.Client.Services;

public interface IShoppingListRepository
{
    Task AddItemToShoppingListAsync(Promotion promotion);

    Task RemoveItemFromShoppingListAsync(Promotion promotion);

    Task IncreaseQuantityInShoppingListAsync(Promotion promotion);

    Task DecreaseQuantityInShoppingListAsync(Promotion promotion);

    Task ClearShoppingListAsync();

    Task<bool> IsEmptyAsync();

    Task<int> GetItemsCountAsync();

    Task<List<ShoppingListItem>> GetAllItemsAsync();
}