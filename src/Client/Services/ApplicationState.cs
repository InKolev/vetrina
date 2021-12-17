using System;
using System.Collections.Generic;
using System.Linq;
using MudBlazor;
using Vetrina.Shared;

namespace Vetrina.Client.Services
{
    public class ApplicationState
    {
        private readonly Dictionary<int, (Promotion Item, int Quantity)> shoppingList =
            new Dictionary<int, (Promotion item, int count)>();

        public event Action ThemeColorChanged;

        public event Action ShoppingListChanged;

        public Color MainThemeColor { get; private set; } = Color.Primary;

        public List<(Promotion item, int count)> ShoppingList => shoppingList.Values.ToList();

        public void AddPromotionalItemToShoppingList(Promotion promotion)
        {
            if (this.shoppingList.TryGetValue(promotion.Id, out var entry))
            {
                entry.Quantity++;
                this.shoppingList[promotion.Id] = entry;
            }
            else
            {
                this.shoppingList.TryAdd(promotion.Id, (promotion, 1));
            }

            this.NotifyShoppingListStateChanged();
        }

        public void IncreaseQuantityInShoppingList(Promotion promotion)
        {
            if (this.shoppingList.TryGetValue(promotion.Id, out var entry))
            {
                entry.Quantity++;
            }

            this.NotifyShoppingListStateChanged();
        }

        public void DecreaseQuantityInShoppingList(Promotion promotion)
        {
            if (this.shoppingList.TryGetValue(promotion.Id, out var entry))
            {
                if (entry.Quantity > 1)
                {
                    entry.Quantity--;
                }
            }

            this.NotifyShoppingListStateChanged();
        }

        public void RemoveFromShoppingList(Promotion promotion)
        {
            if (this.shoppingList.ContainsKey(promotion.Id))
            {
                this.shoppingList.Remove(promotion.Id);
                this.NotifyShoppingListStateChanged();
            }
        }

        public void ClearShoppingList()
        {
            this.shoppingList.Clear();
            this.NotifyShoppingListStateChanged();
        }

        public void SwitchColor()
        {
            switch (MainThemeColor)
            {
                case Color.Primary:
                {
                    MainThemeColor = Color.Secondary;
                    break;
                }
                case Color.Secondary:
                {
                    MainThemeColor = Color.Tertiary;
                    break;
                }
                case Color.Tertiary:
                {
                    MainThemeColor = Color.Info;
                    break;
                }
                case Color.Info:
                {
                    MainThemeColor = Color.Dark;
                    break;
                }
                case Color.Dark:
                {
                    MainThemeColor = Color.Warning;
                    break;
                }
                case Color.Warning:
                {
                    MainThemeColor = Color.Primary;
                    break;
                }
            }

            this.NotifyThemeColorStateChanged();
        }

        private void NotifyThemeColorStateChanged() => ThemeColorChanged?.Invoke();

        private void NotifyShoppingListStateChanged() => ShoppingListChanged?.Invoke();
    }
}