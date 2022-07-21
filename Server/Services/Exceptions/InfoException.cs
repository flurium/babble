﻿namespace Server.Services.Exceptions
{
    /// <summary>
    /// Just specify our custom exeption.
    /// Doesn't need additional info.
    /// </summary>
    public class InfoException : Exception
    {
        public InfoException(string message) : base(message)
        { }
    }
}