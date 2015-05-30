using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRage;
using VRageMath;


namespace Str
{
    /// <summary>
    /// String functions
    /// </summary>
    static class Str 
    {
        /// <summary>
        /// Seaches a string for a matching string
        /// </summary>
        /// <param name="haystack">string to search</param>
        /// <param name="needle">string to match</param>
        /// <param name="caseSensitive">Performs case senistive search. Default = false.</param>
        /// <returns>True if match, false if no match</returns>
        public static bool Contains(string haystack, string needle, bool caseSensitive = false)
        {
            return (haystack.IndexOf(needle, caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase) != -1);
        }
    }
}