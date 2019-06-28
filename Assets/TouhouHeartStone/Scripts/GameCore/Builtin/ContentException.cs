using System;

namespace TouhouHeartstone.Backend.Builtin
{
    class ContentException : Exception
    {
        public ContentException(string msg) : base(msg)
        {
        }
    }
}