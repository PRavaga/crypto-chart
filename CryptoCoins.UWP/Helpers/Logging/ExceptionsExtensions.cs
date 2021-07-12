using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace CryptoCoins.UWP.Helpers.Logging
{
    public static class ExceptionsExtensions
    {
        public static string ExpandException(this Exception exception)
        {
            var sb = new StringBuilder();
            ExpandExceptionInner(exception, sb);
            return sb.ToString();
        }

        private static void ExpandExceptionInner(Exception exception, StringBuilder sb)
        {
            if (exception.InnerException != null)
            {
                var aggregateException = exception as AggregateException;
                if (aggregateException != null)
                {
                    foreach (var innerException in aggregateException.InnerExceptions)
                    {
                        ExpandExceptionInner(innerException, sb);
                    }
                }
                else
                {
                    ExpandExceptionInner(exception.InnerException, sb);
                }
            }
            if (exception is FileLoadException ioException)
            {
                sb.AppendLine($"Filename: {ioException.FileName}");
            }

            sb.AppendLine(string.Format(CultureInfo.CurrentCulture, "{0}: {1}", exception.GetType().FullName, exception.Message));
        }
    }
}
