using System;
using CryptoCoins.UWP.Helpers;
using CryptoCoins.UWP.Models.Services.Entries;
using SQLite;

namespace CryptoCoins.UWP.Models.StorageEntities
{
    public class AlertModel : Observable
    {
        private AlertFrequency _frequency;
        private string _fromCode;
        private string _fromIcon;
        private string _fromName;

        private bool _isArmed;
        private bool _isEnabled;
        private AlertTargetMode _targetMode;
        private double _targetValue;
        private string _toCode;
        private string _toSymbol;

        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        public string FromCode
        {
            get => _fromCode;
            set => Set(ref _fromCode, value);
        }

        [Ignore]
        public string FromName
        {
            get => _fromName;
            set => Set(ref _fromName, value);
        }

        [Ignore]
        public string FromIcon
        {
            get => _fromIcon;
            set => Set(ref _fromIcon, value);
        }

        public string ToCode
        {
            get => _toCode;
            set => Set(ref _toCode, value);
        }

        [Ignore]
        public string ToSymbol
        {
            get => _toSymbol;
            set => Set(ref _toSymbol, value);
        }

        public double TargetValue
        {
            get => _targetValue;
            set => Set(ref _targetValue, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => Set(ref _isEnabled, value);
        }

        /// <summary>
        ///     Determinates whether the alert will be triggered the next time when condition is true. It helps implement "trigger"
        ///     (not "condition") behavior.
        /// </summary>
        public bool IsArmed
        {
            get => _isArmed;
            set => Set(ref _isArmed, value);
        }

        public AlertFrequency Frequency
        {
            get => _frequency;
            set => Set(ref _frequency, value);
        }

        public AlertTargetMode TargetMode
        {
            get => _targetMode;
            set => Set(ref _targetMode, value);
        }
    }
}
