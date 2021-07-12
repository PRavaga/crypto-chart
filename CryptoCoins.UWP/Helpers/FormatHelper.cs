using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCoins.UWP.Helpers
{
    public static class FormatHelper
    {
        public static string ToEngineeringNotation(double d)
        {
            double exponent = Math.Log10(Math.Abs(d));
            if (Math.Abs(d) >= 1)
            {
                switch ((int)Math.Floor(exponent))
                {
                    case 0:
                    case 1:
                    case 2:
                        return d.ToString();
                    case 3:
                    case 4:
                    case 5:
                        return (d / 1e3).ToString("F3") + "K";
                    case 6:
                    case 7:
                    case 8:
                        return (d / 1e6).ToString("F3") + "M";
                    case 9:
                    case 10:
                    case 11:
                        return (d / 1e9).ToString("F3") + "B";
                    case 12:
                    case 13:
                    case 14:
                        return (d / 1e12).ToString("F3") + "T";
                    case 15:
                    case 16:
                    case 17:
                        return (d / 1e15).ToString() + "P";
                    case 18:
                    case 19:
                    case 20:
                        return (d / 1e18).ToString() + "E";
                    case 21:
                    case 22:
                    case 23:
                        return (d / 1e21).ToString() + "Z";
                    default:
                        return (d / 1e24).ToString() + "Y";
                }
            }
            else if (Math.Abs(d) > 0)
            {
                switch ((int)Math.Floor(exponent))
                {
                    case -1:
                    case -2:
                    case -3:
                        return (d * 1e3).ToString() + "m";
                    case -4:
                    case -5:
                    case -6:
                        return (d * 1e6).ToString() + "μ";
                    case -7:
                    case -8:
                    case -9:
                        return (d * 1e9).ToString() + "n";
                    case -10:
                    case -11:
                    case -12:
                        return (d * 1e12).ToString() + "p";
                    case -13:
                    case -14:
                    case -15:
                        return (d * 1e15).ToString() + "f";
                    case -16:
                    case -17:
                    case -18:
                        return (d * 1e15).ToString() + "a";
                    case -19:
                    case -20:
                    case -21:
                        return (d * 1e15).ToString() + "z";
                    default:
                        return (d * 1e15).ToString() + "y";
                }
            }
            else
            {
                return "0";
            }
        }
    }
}
