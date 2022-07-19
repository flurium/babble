using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Services.Exceptions
{
    /// <summary>
    /// Just specify our custom exeption.
    /// Doesn't need additional info.
    /// </summary>
    public class InfoException : Exception
    {
        public InfoException(string? message) : base(message)
        { }
    }
}