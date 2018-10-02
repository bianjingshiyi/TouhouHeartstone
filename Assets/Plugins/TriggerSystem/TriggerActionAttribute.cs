using System;

namespace TriggerSystem
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class TriggerActionAttribute : Attribute
    {
        public string editorName { get; private set; }
        public string editorDesc { get; private set; }
        public TriggerActionAttribute(string editorName, string editorDesc)
        {
            this.editorName = editorName;
            this.editorDesc = editorDesc;
        }
    }
}