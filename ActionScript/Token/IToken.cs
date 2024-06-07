using ActionScript.Functions;
using ActionScript.Terms;

namespace ActionScript.Token
{
    public interface IToken
    {
        /// <summary>
        /// The line this token belongs to. Used for Debugging purposes.
        /// </summary>
        int Line { get; set; }
    }
}