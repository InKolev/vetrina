using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Vetrina.Autogen.API.Client.Contracts;

namespace Vetrina.Client.Services;

public class LocalStorageShoppingListRepository : IShoppingListRepository
{
    private const string ShoppingListKey = "ShoppingList";

    private readonly ILocalStorageService localStorageService;

    public LocalStorageShoppingListRepository(ILocalStorageService localStorageService)
    {
        this.localStorageService = localStorageService;
    }

    public async Task<ShoppingListItem> GetItemByPromotionIdAsync(int promotionId)
    {
        if (await this.localStorageService.ContainKeyAsync(ShoppingListKey))
        {
            var shoppingList = await this.localStorageService.GetItemAsync<List<ShoppingListItem>>(ShoppingListKey);
            var shoppingListItem = shoppingList.FirstOrDefault(x => x.Promotion.Id == promotionId);

            return shoppingListItem;
        }

        return null;
    }

    public async Task AddItemToShoppingListAsync(Promotion promotion)
    {
        if (!await this.localStorageService.ContainKeyAsync(ShoppingListKey))
        {
            await this.localStorageService.SetItemAsync(ShoppingListKey, new List<ShoppingListItem>());
        }

        var shoppingList = await this.localStorageService.GetItemAsync<List<ShoppingListItem>>(ShoppingListKey);
        var shoppingListItem = new ShoppingListItem { Promotion = promotion, Quantity = 1 };

        shoppingList.Add(shoppingListItem);

        Console.WriteLine($"{nameof(AddItemToShoppingListAsync)} - {shoppingList.Count}");

        await this.localStorageService.SetItemAsync(ShoppingListKey, shoppingList);
    }

    public async Task RemoveItemFromShoppingListAsync(Promotion promotion)
    {
        if (await this.localStorageService.ContainKeyAsync(ShoppingListKey))
        {
            var shoppingList = await this.localStorageService.GetItemAsync<List<ShoppingListItem>>(ShoppingListKey);
            var shoppingListItem = shoppingList.FirstOrDefault(x => x.Promotion.Id == promotion.Id);
            if (shoppingListItem != null)
            {
                shoppingList.Remove(shoppingListItem);

                await this.localStorageService.SetItemAsync(ShoppingListKey, shoppingList);
            }
        }
    }

    public async Task IncreaseQuantityInShoppingListAsync(Promotion promotion)
    {
        if (await this.localStorageService.ContainKeyAsync(ShoppingListKey))
        {
            var shoppingList = await this.localStorageService.GetItemAsync<List<ShoppingListItem>>(ShoppingListKey);
            var shoppingListItem = shoppingList.FirstOrDefault(x => x.Promotion.Id == promotion.Id);
            if (shoppingListItem is { Quantity: < 999 })
            {
                shoppingListItem.Quantity++;

                await this.localStorageService.SetItemAsync(ShoppingListKey, shoppingList);
            }
        }
    }

    public async Task DecreaseQuantityInShoppingListAsync(Promotion promotion)
    {
        if (await this.localStorageService.ContainKeyAsync(ShoppingListKey))
        {
            var shoppingList = await this.localStorageService.GetItemAsync<List<ShoppingListItem>>(ShoppingListKey);
            var shoppingListItem = shoppingList.FirstOrDefault(x => x.Promotion.Id == promotion.Id);
            if (shoppingListItem is { Quantity: > 1 })
            {
                shoppingListItem.Quantity--;

                await this.localStorageService.SetItemAsync(ShoppingListKey, shoppingList);
            }
        }
    }

    public async Task ClearShoppingListAsync()
    {
        if (await this.localStorageService.ContainKeyAsync(ShoppingListKey))
        {
            var shoppingList = await this.localStorageService.GetItemAsync<List<ShoppingListItem>>(ShoppingListKey);

            shoppingList.Clear();

            await this.localStorageService.SetItemAsync(ShoppingListKey, shoppingList);
        }
    }

    public async Task<bool> IsEmptyAsync()
    {
        if (await this.localStorageService.ContainKeyAsync(ShoppingListKey))
        {
            var shoppingList = await this.localStorageService.GetItemAsync<List<ShoppingListItem>>(ShoppingListKey);

            return shoppingList.Count == 0;
        }

        return true;
    }

    public async Task<int> GetItemsCountAsync()
    {
        if (await this.localStorageService.ContainKeyAsync(ShoppingListKey))
        {
            var shoppingList = await this.localStorageService.GetItemAsync<List<ShoppingListItem>>(ShoppingListKey);

            return shoppingList.Count;
        }

        return 0;
    }

    public async Task<List<ShoppingListItem>> GetAllItemsAsync()
    {
        if (await this.localStorageService.ContainKeyAsync(ShoppingListKey))
        {
            var shoppingList = await this.localStorageService.GetItemAsync<List<ShoppingListItem>>(ShoppingListKey);

            return shoppingList;
        }

        return new List<ShoppingListItem>();
    }
}