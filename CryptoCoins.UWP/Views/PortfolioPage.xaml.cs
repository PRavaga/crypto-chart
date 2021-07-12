using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using CryptoCoins.UWP.Helpers;
using CryptoCoins.UWP.ViewModels;
using CryptoCoins.UWP.Views.Controls;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Telerik.UI.Xaml.Controls.Grid;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CryptoCoins.UWP.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PortfolioPage : MvvmPage
    {
        public PortfolioPage()
        {
            InitializeComponent();
            UnwrapGridCellTapCommand.UnwrapParameter = UnwrapGridCellTapParameter;
            UnwrapHoldingParameterCommand.UnwrapParameter = UnwrapGridCellTapParameter;
            ViewModel.PropertyChanged += OnPropertyChanged;
        }

        public PortfolioViewModel ViewModel => (PortfolioViewModel) DataContext;

        public object UnwrapGridCellTapParameter(object o)
        {
            var cellInfo = (DataGridCellInfo) o;
            return cellInfo.Value;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PortfolioViewModel.Holdings))
            {
                var panel = HoldingsBar.FindDescendant<BarPanel>();
                panel?.InvalidateMeasure();
            } else if (e.PropertyName == nameof(PortfolioViewModel.IsHistoryLoaded))
            {
                ToolTipService.SetToolTip(ChangeHeaderPanel, ViewModel.IsHistoryLoaded ? null : "PortfolioPage_ChangeHeaderPanel_Partial".GetLocalized());
            }
        }
    }
}
