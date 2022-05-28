using System.Linq;

namespace ViewModels.Extensions
{
    internal static class StringExtensions
    {
        public static bool OnlyDigits(this string? str) => str != null && str.Any(c => c >= '0' && c <= '9');
    }
}
