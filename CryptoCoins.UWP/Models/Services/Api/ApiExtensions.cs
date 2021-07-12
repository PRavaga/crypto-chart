using System;
using System.Threading.Tasks;
using CryptoCoins.UWP.Models.Services.Api.Exceptions;
using Newtonsoft.Json;

namespace CryptoCoins.UWP.Models.Services.Api
{
    public static class ApiExtensions
    {
        public static async Task<T> Retry<T, TException>(Func<Task<T>> thingToTry, int attempts)
            where TException : Exception
        {
            // Start at 1 instead of 0 to allow for final attempt
            for (var i = 1; i < attempts; i++)
            {
                try
                {
                    return await thingToTry();
                }
                catch (TException)
                {
                }
            }

            return await thingToTry(); // Final attempt, let exception bubble up
        }

        public static async Task<T> RetryWithDelay<T, TException>(Func<Task<T>> thingToTry, int attempts, TimeSpan delay)
            where TException : Exception
        {
            // Start at 1 instead of 0 to allow for final attempt
            for (var i = 1; i < attempts; i++)
            {
                try
                {
                    return await thingToTry();
                }
                catch (TException)
                {
                    await Task.Delay(delay);
                }
            }

            return await thingToTry(); // Final attempt, let exception bubble up
        }

        public static async Task<T> HandleServerError<T, TErrorResponse>(this Func<Task<T>> thingToTry, Func<TErrorResponse, T> handler)
        {
            try
            {
                return await thingToTry();
            }
            catch (ServerException e)
            {
                try
                {
                    var errorResponse = JsonConvert.DeserializeObject<TErrorResponse>(e.Response);
                    if (errorResponse != null)
                    {
                        var result = handler(errorResponse);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
                catch (JsonException)
                {
                }
                throw;
            }
        }
    }
}
