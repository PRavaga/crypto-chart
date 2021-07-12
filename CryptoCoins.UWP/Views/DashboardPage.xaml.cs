using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.DirectX;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using CryptoCoins.UWP.Models.Services;
using CryptoCoins.UWP.Models.Services.Entries;
using CryptoCoins.UWP.Platform.BackgroundTasks;
using CryptoCoins.UWP.ViewModels;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace CryptoCoins.UWP.Views
{
    public sealed partial class DashboardPage : MvvmPage, IBitmapProvider
    {
        private readonly Brush _listOddBgBrush;
        private readonly Brush _transparentBrush;

        public DashboardPage()
        {
            InitializeComponent();
            ViewModel.ShareImageProvider = this;
            _listOddBgBrush = (Brush) Application.Current.Resources["ConversionListOddBackgroundBrush"];
            _transparentBrush = new SolidColorBrush(Colors.Transparent);
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.ConversionsChanged -= OnConversionsChanged;
            Bindings.StopTracking();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.ConversionsChanged += OnConversionsChanged;
        }

        private void OnConversionsChanged()
        {
            RefreshItemsBackground();
        }

        public DashboardViewModel ViewModel => (DashboardViewModel) DataContext;

        private void ConversionsList_OnContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            args.ItemContainer.Background = args.ItemIndex % 2 == 1 ? _listOddBgBrush : _transparentBrush;
        }

        private void RefreshItemsBackground()
        {
            var items = ConversionsList.Items;

            for (var i = 0; i < items.Count; i++)
            {
                var item = (SelectorItem)ConversionsList.ContainerFromItem(items[i]);
                if (item != null)
                {
                    item.Background = i % 2 == 1 ? _listOddBgBrush : _transparentBrush;
                }
            }
        }

        private void ConversionsList_OnDragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            RefreshItemsBackground();
        }

        private void FrameworkElement_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var wrapGrid = (ItemsWrapGrid) sender;
            var targetWidth = 144 + 4 + 4;
            var count = Math.Floor(e.NewSize.Width / targetWidth);
            wrapGrid.ItemWidth = e.NewSize.Width / count;
        }

        private void FeaturedOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var wrapGrid = (ItemsWrapGrid)sender;
            var targetWidth = 220 + 4 + 4;
            var count = Math.Floor(e.NewSize.Width / targetWidth);
            wrapGrid.ItemWidth = e.NewSize.Width / count;
        }

        public async Task<RandomAccessStreamReference> GetBitmap(object arg)
        {
            if (arg is DetailedConversionInfo)
            {
                var tcs = new TaskCompletionSource<RandomAccessStreamReference>();
                // XAML objects can only be accessed on the UI thread, and the call may come in on a background thread
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    var container = FeaturedConversions.ContainerFromItem(arg);
                    if (container is UIElement view)
                    {
                        RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
                        InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();
                        // Render to an image at the current system scale and retrieve pixel contents
                        await renderTargetBitmap.RenderAsync(view);
                        var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();

                        CanvasDevice device = CanvasDevice.GetSharedDevice();
                        CanvasRenderTarget offscreen = new CanvasRenderTarget(device, renderTargetBitmap.PixelWidth, renderTargetBitmap.PixelHeight, DisplayInformation.GetForCurrentView().LogicalDpi);
                        using (CanvasDrawingSession ds = offscreen.CreateDrawingSession())
                        {
                            var color = (Color) Application.Current.Resources["ContentBackgroundColor"];
                            ds.Clear(color);
                            ds.DrawImage(CanvasBitmap.CreateFromBytes(ds, pixelBuffer, renderTargetBitmap.PixelWidth, renderTargetBitmap.PixelHeight,
                                DirectXPixelFormat.B8G8R8A8UIntNormalized, DisplayInformation.GetForCurrentView().LogicalDpi));
                        }
                        await offscreen.SaveAsync(stream, CanvasBitmapFileFormat.Png);

                        tcs.SetResult(RandomAccessStreamReference.CreateFromStream(stream));
                    }
                    else
                    {
                        tcs.SetResult(null);
                    }
                });
                return await tcs.Task;
            }
            throw new NotImplementedException("Only detailedConversionInfo is supported");
        }
    }
}
