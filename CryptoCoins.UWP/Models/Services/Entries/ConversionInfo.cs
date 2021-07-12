using System;
using System.Diagnostics;
using Windows.UI.Xaml.Controls;
using CryptoCoins.UWP.Helpers;
using CryptoCoins.UWP.Models.UserPreferences;
using CryptoCoins.UWP.Platform.BackgroundTasks;
using CryptoCoins.UWP.Views.Formatter;

namespace CryptoCoins.UWP.Models.Services.Entries
{
    public class ConversionInfo : Observable
    {
        private double _change24;

        private double _changeValue;
        private string _from;
        private string _fromFullName;

        private Uri _fromIcon;
        private bool _isPinned;

        private ConversionPreference _pref;
        private double _rate;
        private string _to;
        private double _volume24;

        public string From
        {
            get => _from;
            set => Set(ref _from, value);
        }

        public string FromFullName
        {
            get => _fromFullName;
            set => Set(ref _fromFullName, value);
        }
        

        public Uri FromIcon
        {
            get => _fromIcon;
            set => Set(ref _fromIcon, value);
        }

        public string To
        {
            get => _to;
            set => Set(ref _to, value);
        }

        public double Volume24
        {
            get => _volume24;
            set => Set(ref _volume24, value);
        }

        public double Change24
        {
            get => _change24;
            set => Set(ref _change24, value);
        }

        public double ChangeValue
        {
            get => _changeValue;
            set => Set(ref _changeValue, value);
        }

        public double Rate
        {
            get => _rate;
            set => Set(ref _rate, value);
        }

        public ConversionPreference Pref
        {
            get => _pref;
            set => Set(ref _pref, value);
        }

        public bool IsPinned
        {
            get => _isPinned;
            set => Set(ref _isPinned, value);
        }
    }
}
