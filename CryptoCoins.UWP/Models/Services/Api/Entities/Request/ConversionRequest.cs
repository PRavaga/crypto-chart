using System.Collections.Generic;
using CryptoCoins.UWP.Helpers.QueryString;

namespace CryptoCoins.UWP.Models.Services.Api.Entities.Request
{
    public class ConversionRequest
    {
        [QueryParameterList("fsyms", MaxLength = 300)]
        public List<string> CurrenciesFrom { get; set; }
        [QueryParameterList("tsyms")]
        public List<string> CurrenciesTo { get; set; }
    }
}
