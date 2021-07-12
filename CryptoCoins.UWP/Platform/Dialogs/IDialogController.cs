using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCoins.UWP.Platform.Dialogs
{
    public interface IDialogController
    {
        void Hide(object result);
        object Result { get; set; }
    }
}
