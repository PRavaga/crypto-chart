using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCoins.UWP.Models.Services.Api.Exceptions
{
    public class NoDataForCurrencyException:ServerException
    {
        public NoDataForCurrencyException(int statusCode, string response) : base(statusCode, response)
        {
        }
    }
}
