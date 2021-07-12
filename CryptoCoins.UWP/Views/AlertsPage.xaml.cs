using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using CryptoCoins.UWP.ViewModels;
using Telerik.Data.Core;
using Telerik.UI.Xaml.Controls.Grid;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CryptoCoins.UWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AlertsPage : MvvmPage
    {
        public AlertsPage()
        {
            this.InitializeComponent();
            UnwrapGridCellTapCommand.UnwrapParameter = UnwrapGridCellTapParameter;
            AlertsList.DataBindingComplete += AlertsList_DataBindingComplete;
        }

        private object UnwrapGridCellTapParameter(object o)
        {
            var cellInfo = (DataGridCellInfo)o;
            return cellInfo.Value;
        }

        private void AlertsList_DataBindingComplete(object sender, DataBindingCompleteEventArgs e)
        {
            if (e.ChangeFlags == DataChangeFlags.Filter)
            {
                if (e.DataView.Items.Count == 0)
                {
                    ViewModel.DataState = Models.Services.Entries.DataState.FilteredEmpty;
                }
                else
                {
                    ViewModel.DataState = Models.Services.Entries.DataState.Available;
                }
            }
        }

        public AlertsViewModel ViewModel => (AlertsViewModel) DataContext;
    }
}
