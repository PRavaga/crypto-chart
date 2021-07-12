using System;
using System.Diagnostics;

namespace CryptoCoins.UWP.Views.Formatter
{
    public static class Value
    {
        public static string FormatNumber(double value, int maxLength)
        {
            return FormatNumber(value, maxLength, int.MaxValue);
        }

        public static string FormatNumber(double value, int maxLength, int maxFractionLength)
        {
            const int MinLength = 4;
            Debug.Assert(maxLength >= MinLength);
            var lg10 = Math.Log10(Math.Abs(value));
            if (double.IsInfinity(lg10))
            {
                return value.ToString("F2");
            }
            var intLength = Math.Max(1, (int) Math.Ceiling(lg10));
            //Check if the value is too big so we need to use e+ notation to fit the length
            if (intLength > maxLength)
            {
                var targetLength = maxLength - 3;
                var exp = intLength - targetLength;
                if (exp >= 10)
                {
                    exp++;
                }
                var mantissa = value / Math.Pow(10, exp);
                return $"{Math.Round(mantissa)}e+{exp}";
            }
            //Check if the value is too small so we need to use e- notation to fit the length
            if (value < 1)
            {
                var fractionLength = Math.Max(0, Math.Ceiling(-lg10));
                var totalLength = fractionLength + 2;
                if (totalLength > maxLength)
                {
                    var exp = fractionLength;
                    var mantissa = value * Math.Pow(10, exp);
                    var targetTotalLength = maxLength - (exp >= 10 ? 4 : 3);
                    var targetFractionLength = targetTotalLength - 2;
                    if (targetFractionLength < 0)
                    {
                        //In case we have no space for fraction
                        targetFractionLength = 0;
                    }
                    return $"{Math.Round(mantissa, targetFractionLength)}e-{exp}";
                }
            }
            var minLength = (int) Math.Ceiling(-lg10) + 2;
            if (value < 1 && minLength > maxLength)
            {
                //-1 for '.'
                var reduceE = minLength - (maxLength - 3) - 1;
                var reduced = Math.Round(value * Math.Pow(10, reduceE));
                return $"{reduced}e-{reduceE}";
            }
            var prec = Math.Min(maxLength - intLength - 1, maxFractionLength);
            //Check for 0 <= prec <= 15 otherwise Round with throw an exception
            if (prec < 0)
            {
                prec = 0;
            } else if (prec > 15)
            {
                prec = 15;
            }
            var resultValue = Math.Round(value, prec);
            var result = resultValue.ToString($"F{prec}");
            for (var i = prec; i > 2; i--)
            {
                if (result[result.Length - 1] == '0')
                {
                    result = result.Substring(0, result.Length - 1);
                }
            }
            return result;
        }

        [Conditional("DEBUG")]
        private static void Verify(double value, int outLength, string expected)
        {
            var formated = FormatNumber(value, outLength);
            Debug.Assert(string.Equals(formated, expected, StringComparison.Ordinal));
        }

        [Conditional("DEBUG")]
        public static void Tests()
        {
            Verify(4234.043, 4, "4234");
            Verify(234_234_432_323.3234, 5, "2e+11");
            Verify(234_234_432_323.3234, 6, "234e+9");
            Verify(.000_01, 7, "0.00001");
            Verify(.000_01, 6, "1e-5");
            Verify(.000_011, 6, "1.1e-5");
            Verify(.000_011_1, 6, "1.1e-5");
            Verify(324.5, 6, "324.50");
            Verify(3244.5, 6, "3244.5");
            Verify(32442.5, 6, "32442");
            Verify(32442.5, 9, "32442.50");
        }
    }
}
