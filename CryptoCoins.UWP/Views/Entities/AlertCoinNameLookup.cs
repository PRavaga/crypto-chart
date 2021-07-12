using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoCoins.UWP.Models.StorageEntities;
using Telerik.Data.Core;

namespace CryptoCoins.UWP.Views.Entities
{
    public class AlertCoinNameLookup:IKeyLookup
    {
        public object GetKey(object instance)
        {
            return ((AlertModel) instance).FromName;
        }
    }
}
