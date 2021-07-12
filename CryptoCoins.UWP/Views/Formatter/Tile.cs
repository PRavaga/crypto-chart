using CryptoCoins.UWP.Helpers;

namespace CryptoCoins.UWP.Views.Formatter
{
    public static class Tile
    {
        public static string GetLocalizedPinText(bool isPinned)
        {
            return isPinned ? "DashboardPage_ContextMenuUnpinTile".GetLocalized() : "DashboardPage_ContextMenuPinTile".GetLocalized();
        }

        public static string GetIcon(bool isPinned)
        {
            return isPinned ? "\uE77A" : "\uE718";
        }
    }
}
