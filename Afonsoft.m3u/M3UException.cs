using System;

namespace Afonsoft.m3u
{
    /// <summary>
    /// M3UException
    /// </summary>
    public class M3UException : Exception
    {
        /// <summary>
        /// M3UException
        /// </summary>
        public M3UException()
        {
        }

        /// <summary>
        /// M3UException
        /// </summary>
        /// <param name="message"></param>
        public M3UException(string message) : base(message)
        {
        }

        /// <summary>
        /// M3UException
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public M3UException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}