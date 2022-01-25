using System;
using System.Collections.Generic;
using MudBlazor;

namespace Vetrina.Client.Services
{
    public class ApplicationState
    {
        public event Action ThemeColorChanged;

        public event Action<List<ShoppingListItem>> ShoppingListChanged;

        public string ShakeCartClass { get; set; }

        public string TossToCartClass { get; set; }

        public Color MainThemeColor { get; private set; } = Color.Tertiary;

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
                        MainThemeColor = Color.Transparent;
                        break;
                    }
                case Color.Transparent:
                    {
                        MainThemeColor = Color.Primary;
                        break;
                    }
            }

            this.NotifyThemeColorStateChanged();
        }

        public void NotifyThemeColorStateChanged() => ThemeColorChanged?.Invoke();

        public void NotifyShoppingListStateChanged(List<ShoppingListItem> shoppingList) => ShoppingListChanged?.Invoke(shoppingList);
    }
}