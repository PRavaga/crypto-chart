using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoCoins.UWP.Models.StorageEntities;

namespace CryptoCoins.UWP.Models.Extensions
{
    public static class AlertModelEx
    {

        public static void SetArmed(this AlertModel alert, double currentValue)
        {
            switch (alert.TargetMode)
            {
                case AlertTargetMode.Above:
                    alert.IsArmed = currentValue < alert.TargetValue;
                    break;
                case AlertTargetMode.Below:
                    alert.IsArmed = currentValue > alert.TargetValue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }



        public static bool IsAlertTriggered(this AlertModel alert, double currentRate)
        {
            if (alert.IsEnabled && alert.IsArmed)
            {
                switch (alert.TargetMode)
                {
                    case AlertTargetMode.Above:
                        if (currentRate > alert.TargetValue)
                        {
                            return true;
                        }
                        break;
                    case AlertTargetMode.Below:
                        if (currentRate < alert.TargetValue)
                        {
                            return true;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return false;
        }

        public static void Fire(this AlertModel alert, double currentRate)
        {
            alert.SetArmed(currentRate);
            if (alert.Frequency == AlertFrequency.OneTime)
            {
                alert.IsEnabled = false;
            }
        }
    }
}
