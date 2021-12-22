using System;
using System.Collections.Generic;
using System.Linq;
using MudBlazor;
using Vetrina.Shared;

namespace Vetrina.Client.Services
{
    public class ApplicationState
    {
        public event Action ThemeColorChanged;

        public event Action ShoppingListChanged;

        public Color MainThemeColor { get; private set; } = Color.Primary;

        public ICollection<ShoppingListItem> ShoppingList { get; set; } = new List<ShoppingListItem>();

        public void AddPromotionalItemToShoppingList(Promotion promotion)
        {
            var listItem = this.ShoppingList.SingleOrDefault(x => x.Promotion.Id == promotion.Id);
            if (listItem == default)
            {
                this.ShoppingList.Add(new ShoppingListItem { Promotion = promotion, Quantity = 1 });
            }

            this.NotifyShoppingListStateChanged();
        }

        public void IncreaseQuantityInShoppingList(Promotion promotion)
        {
            var listItem = this.ShoppingList.SingleOrDefault(x => x.Promotion.Id == promotion.Id);

            if (listItem != default && listItem.Quantity < 999)
            {
                listItem.Quantity++;

                this.NotifyShoppingListStateChanged();
            }
        }

        public void DecreaseQuantityInShoppingList(Promotion promotion)
        {
            var listItem = this.ShoppingList.SingleOrDefault(x => x.Promotion.Id == promotion.Id);

            if (listItem != default && listItem.Quantity > 1)
            {
                listItem.Quantity--;

                this.NotifyShoppingListStateChanged();
            }
        }

        public void RemoveFromShoppingList(Promotion promotion)
        {
            var listItem = this.ShoppingList.SingleOrDefault(x => x.Promotion.Id == promotion.Id);

            if (listItem != default)
            {
                this.ShoppingList.Remove(listItem);

                this.NotifyShoppingListStateChanged();
            }
        }

        public void ClearShoppingList()
        {
            this.ShoppingList.Clear();

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