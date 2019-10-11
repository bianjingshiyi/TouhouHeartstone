using System;

namespace TouhouHeartstone
{
    class ContentException : Exception
    {
        public ContentException(string msg) : base(msg)
        {
        }
    }
}