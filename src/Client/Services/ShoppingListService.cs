using System;
using System.Threading.Tasks;
using Vetrina.Autogen.API.Client.Contracts;

namespace Vetrina.Client.Services;

public class ShoppingListService : IShoppingListService
{
    private readonly IShoppingListRepository shoppingListRepository;
    private readonly ApplicationState applicationState;

    public ShoppingListService(
        IShoppingListRepository shoppingListRepository,
        ApplicationState applicationState)
    {
        this.shoppingListRepository = shoppingListRepository;
        this.applicationState = applicationState;
    }

    public async Task AddPromotionalItemToShoppingList(Promotion promotion)
    {
        await this.shoppingListRepository.AddItemToShoppingListAsync(promotion);

        this.applicationState.ShakeCartClass = "cartshake";

        var shoppingList = await this.shoppingListRepository.GetAllItemsAsync();

        this.applicationState.NotifyShoppingListStateChanged(shoppingList);

        await Task.Delay(TimeSpan.FromMilliseconds(400));

        this.applicationState.ShakeCartClass = string.Empty;

        this.applicationState.NotifyShoppingListStateChanged(shoppingList);
    }

    public async Task IncreaseQuantityInShoppingList(Promotion promotion)
    {
        await this.shoppingListRepository.IncreaseQuantityInShoppingListAsync(promotion);
        var shoppingList = await this.shoppingListRepository.GetAllItemsAsync();

        this.applicationState.NotifyShoppingListStateChanged(shoppingList);
    }

    public async Task DecreaseQuantityInShoppingList(Promotion promotion)
    {
        await this.shoppingListRepository.DecreaseQuantityInShoppingListAsync(promotion);
        var shoppingList = await this.shoppingListRepository.GetAllItemsAsync();

        this.applicationState.NotifyShoppingListStateChanged(shoppingList);
    }

    public async Task RemoveFromShoppingList(Promotion promotion)
    {
        await this.shoppingListRepository.RemoveItemFromShoppingListAsync(promotion);
        var shoppingList = await this.shoppingListRepository.GetAllItemsAsync();

        this.applicationState.NotifyShoppingListStateChanged(shoppingList);
    }

    public async Task ClearShoppingList()
    {
        await this.shoppingListRepository.ClearShoppingListAsync();
        var shoppingList = await this.shoppingListRepository.GetAllItemsAsync();

        this.applicationState.NotifyShoppingListStateChanged(shoppingList);
    }
}