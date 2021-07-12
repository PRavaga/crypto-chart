using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace CryptoCoins.UWP.Views.Controls
{
    public sealed partial class ChangeIndicator : UserControl
    {
        private double _value;

        public ChangeIndicator()
        {
            InitializeComponent();
        }

        public double Value
        {
            get => _value;
            set
            {
                _value = value;
                if (_value > 0)
                {
                    UpIndicator.Visibility = Visibility.Visible;
                    DownIndicator.Visibility = Visibility.Collapsed;
                }
                else if (_value == 0d)
                {
                    UpIndicator.Visibility = Visibility.Collapsed;
                    DownIndicator.Visibility = Visibility.Collapsed;
                }
                else
                {
                    UpIndicator.Visibility = Visibility.Collapsed;
                    DownIndicator.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
