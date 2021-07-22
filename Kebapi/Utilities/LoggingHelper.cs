using System.Runtime.CompilerServices;

namespace Kebapi
{
    /// <summary>
    /// Some helpers for logging. 
    /// </summary>
    public static class LoggingHelper
    {
        /// <summary>
        /// When invoked inside a method, returns the method name. Obtains the 
        /// name from an Attribute automatically provided by
        /// System.Runtime.CompilerServices. 
        /// </summary>
        /// <param name="methodName">Not required. Method name will be obtained 
        /// from Attribute.</param>
        /// <returns>Name of the method where MethodName() is called.</returns>
        public static string MethodName([CallerMemberName] string methodName = null)
        {
            return methodName;
        }

        /// <summary>
        /// Vanity string formatter paired with ExitMsg. Intended to log the 
        /// start of some process. Helpful in human debugging.
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns>s1 and s2 formatted as an 'enter' message.</returns>
        public static string EnterMsg(string s1, string s2)
        {
            return $"{s1} > {s2}";
        }

        /// <summary>
        /// Vanity string formatter paired with EnterMsg. Intended to log the 
        /// end of some process. Helpful in human debugging.
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns>s1 and s2 formatted as an 'exit' message.</returns>
        public static string ExitMsg(string s1, string s2)
        {
            return $"{s1} < {s2}";
        }
    }

    
}
