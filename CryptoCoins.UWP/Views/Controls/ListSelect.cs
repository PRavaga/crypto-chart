using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Microsoft.Toolkit.Uwp.UI;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace CryptoCoins.UWP.Views.Controls
{
    public sealed class ListSelect : ListBox
    {
        public ListSelect()
        {
            this.DefaultStyleKey = typeof(ListSelect);
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            var listItem = (ListSelectItem) element;
            var index = IndexFromContainer(listItem);
            if (index == 0)
            {
                listItem.SeparatorVisibility=Visibility.Collapsed;
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ListSelectItem();
        }
    }
}
