using System;

namespace Ngsoft.Demo.Public.Api.Exceptions
{
    [Serializable]
    public class PublicException : Exception
    {
        public object Error { get; }

        public PublicException(string message) : base(message) { }

        public PublicException(string message, object error) : base(message)
        {
            Error = error;
        }
    }
}
