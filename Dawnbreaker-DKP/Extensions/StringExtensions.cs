using System.Collections.Generic;
using System.Linq;

namespace Dawnbreaker_DKP.Extensions
{
    public static class StringExtensions
    {
        public static bool ContainsStandardized(this IEnumerable<string> list, string item)
        {
            if (list == null || item == null) return false;
            return list.Select(x => x.ToUpper().Trim()).Contains(item.ToUpper().Trim());
        }
    }
}
