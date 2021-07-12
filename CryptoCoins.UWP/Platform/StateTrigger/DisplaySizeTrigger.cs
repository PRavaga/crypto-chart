using Windows.ApplicationModel;
using Windows.Graphics.Display;
using Windows.UI.Xaml;

namespace CryptoCoins.UWP.Platform.StateTrigger
{
    public class DisplaySizeTrigger : StateTriggerBase
    {
        private readonly double? _diagonalSize;
        private double _minDiagonalSize;

        public DisplaySizeTrigger()
        {
            if (!DesignMode.DesignModeEnabled)
            {
                _diagonalSize = DisplayInformation.GetForCurrentView().DiagonalSizeInInches;
            }
        }


        public double MinDiagonalSize
        {
            get { return _minDiagonalSize; }
            set
            {
                _minDiagonalSize = value;
                var active = false;
                if (_diagonalSize < value)
                {
                    active = false;
                }
                else if (_diagonalSize > value)
                {
                    active = true;
                }
                SetActive(active);
            }
        }
    }
}