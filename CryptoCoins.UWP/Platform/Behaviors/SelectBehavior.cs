using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Microsoft.Xaml.Interactivity;

namespace CryptoCoins.UWP.Platform.Behaviors
{
    public abstract class SelectBehavior<T> : Behavior<Selector>
    {
        private long _itemSourceCallbackToken;
        private bool _preventReentrancy;

        public event EventHandler<SelectionEventArgs> SelectionChanged;

        protected override void OnAttached()
        {
            base.OnAttached();
            SubscribeToEvents();
            UpdateSelectionFromSource();
        }

        protected override void OnDetaching()
        {
            UnsubscribeFromEvents();
            base.OnDetaching();
        }

        private void SubscribeToEvents()
        {
            AssociatedObject.SelectionChanged += OnControlSelectionChanged;
            _itemSourceCallbackToken = AssociatedObject.RegisterPropertyChangedCallback(ItemsControl.ItemsSourceProperty, OnItemSourceChanged);
        }

        private void UnsubscribeFromEvents()
        {
            AssociatedObject.SelectionChanged -= OnControlSelectionChanged;
            AssociatedObject.UnregisterPropertyChangedCallback(ItemsControl.ItemsSourceProperty, _itemSourceCallbackToken);
        }

        private void OnControlSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_preventReentrancy)
            {
                foreach (T item in e.AddedItems)
                {
                    SetSelected(item, true);
                }

                foreach (T item in e.RemovedItems)
                {
                    SetSelected(item, false);
                }

                SelectionChanged?.Invoke(this, new SelectionEventArgs());
            }
        }

        private void OnItemSourceChanged(DependencyObject sender, DependencyProperty dp)
        {
            UpdateSelectionFromSource();
        }

        private void UpdateSelectionFromSource()
        {
            if (AssociatedObject.ItemsSource is IEnumerable<T> items)
            {
                _preventReentrancy = true;
                AssociatedObject.SelectedIndex = -1;
                try
                {
                    foreach (var item in items)
                    {
                        if (IsSelected(item))
                        {
                            if (AssociatedObject is ListBox listBox)
                            {
                                listBox.SelectedItems.Add(item);
                            }
                            else if (AssociatedObject is ListView listView)
                            {
                                listView.SelectedItems.Add(item);
                            }
                        }
                    }
                }
                finally
                {
                    _preventReentrancy = false;
                }
            }
        }

        protected abstract bool IsSelected(T item);

        protected abstract void SetSelected(T item, bool value);
    }
}
