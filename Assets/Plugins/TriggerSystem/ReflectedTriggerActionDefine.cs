using System.Reflection;

namespace TriggerSystem
{
    public class ReflectedTriggerActionDefine
    {
        public string name
        {
            get { return method.DeclaringType.FullName + '.' + method.Name; }
        }
        MethodInfo method { get; set; }
        public string editorName { get; private set; }
        public string editorDesc { get; private set; }
        public ReflectedTriggerActionDefine(TriggerActionAttribute attribute, MethodInfo method)
        {
            this.method = method;
            editorName = attribute.editorName;
            editorDesc = attribute.editorDesc;
        }
    }
}