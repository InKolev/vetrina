using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MudBlazor;
using Vetrina.Autogen.API.Client.Contracts;

namespace Vetrina.Client.Services
{
    public class ApplicationState
    {
        public event Action ThemeColorChanged;

        public event Action ShoppingListChanged;

        public string ShakeCartClass { get; set; }

        public string TossToCartClass { get; set; }

        public Color MainThemeColor { get; private set; } = Color.Primary;

        public ICollection<ShoppingListItem> ShoppingList { get; set; } = new List<ShoppingListItem>();

        public async Task AddPromotionalItemToShoppingList(Promotion promotion)
        {
            var listItem = this.ShoppingList.SingleOrDefault(x => x.Promotion.Id == promotion.Id);
            if (listItem == default)
            {
                this.ShoppingList.Add(new ShoppingListItem { Promotion = promotion, Quantity = 1 });
            }

            this.ShakeCartClass = "cartshake";

            this.NotifyShoppingListStateChanged();

            await Task.Delay(TimeSpan.FromMilliseconds(600));

            this.ShakeCartClass = string.Empty;

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