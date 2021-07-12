using System.ComponentModel;
using CryptoCoins.UWP.ViewModels;
using Newtonsoft.Json;

namespace CryptoCoins.UWP.Models.UserPreferences
{
    public class DisplayPreference
    {
        [DefaultValue(ViewMode.Grid)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public ViewMode DashboardViewMode { get; set; }

        [DefaultValue(PortfolioViewMode.Chart)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public PortfolioViewMode PortfolioViewMode { get; set; }

        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool IsAlertsEnabled { get; set; }

        public string PortfolioTargetCurrency { get; set; }
        public FeaturedPreference PortfolioChartPreference { get; set; }
    }
}
