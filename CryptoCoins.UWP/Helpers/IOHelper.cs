using System;
using System.IO;
using System.Text.RegularExpressions;

namespace CryptoCoins.UWP.Helpers
{
    public static class IOHelper
    {
        private static readonly char[] InvalidFilenameChars = Path.GetInvalidFileNameChars();
        private static readonly Regex InvalidFilenameCharsRegex = new Regex($"[{Regex.Escape(new string(InvalidFilenameChars))}]");

        public static string ReplaceInvalidFilenameChars(string filename)
        {
            return InvalidFilenameCharsRegex.Replace(filename, match => $"_{Array.IndexOf(InvalidFilenameChars, match.Value[0])}_");
        }
    }
}
