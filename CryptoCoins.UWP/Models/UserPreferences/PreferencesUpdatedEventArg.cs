namespace CryptoCoins.UWP.Models.UserPreferences
{
    public class PreferencesUpdatedEventArg
    {
        public PreferencesUpdatedEventArg(UpdateAction action)
        {
            Action = action;
        }

        public UpdateAction Action { get; }
    }
}
