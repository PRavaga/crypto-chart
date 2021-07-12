using Windows.Web;

namespace CryptoCoins.UWP.Models.Services.Api.Exceptions
{
    public class NoInternetException : NetworkException
    {
        public NoInternetException(WebErrorStatus status) : base(status)
        {
        }

        public NoInternetException() : base(WebErrorStatus.CannotConnect)
        {
        }
    }
}
