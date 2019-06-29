using System;

namespace TouhouHeartstone.Backend
{
    class ContentException : Exception
    {
        public ContentException(string msg) : base(msg)
        {
        }
    }
}