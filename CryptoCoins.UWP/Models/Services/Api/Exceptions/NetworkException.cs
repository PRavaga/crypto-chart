using Windows.Web;

namespace CryptoCoins.UWP.Models.Services.Api.Exceptions
{
    public class NetworkException : ApiException
    {
        public NetworkException(WebErrorStatus status) : base($"Network exception: {status}")
        {
        }
    }
}
