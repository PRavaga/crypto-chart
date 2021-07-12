using System.Collections.Generic;

namespace CryptoCoins.UWP.Models.Services.Entries
{
    public class DetailedConversionInfo : ConversionInfo
    {
        private double _max;
        private double _min;
        private List<double> _rateHourlyHistory;

        public List<double> RateHourlyHistory
        {
            get => _rateHourlyHistory;
            set => Set(ref _rateHourlyHistory, value);
        }

        public double Min
        {
            get => _min;
            set => Set(ref _min, value);
        }

        public double Max
        {
            get => _max;
            set => Set(ref _max, value);
        }
    }
}
