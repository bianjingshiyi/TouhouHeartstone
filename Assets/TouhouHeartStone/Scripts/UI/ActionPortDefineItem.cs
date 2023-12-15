using System;

namespace UI
{
    public enum ActionPortDefineType
    {
        Input,
        Output
    }
    public class ActionPortDefineItem
    {
        public string propertyName;
        public ActionPortDefineType portType;
        public Type type;
        public bool isArray;
        public bool isParams;

        public ActionPortDefineItem(string name, ActionPortDefineType portType, Type type, bool isArray, bool isParams)
        {
            propertyName = name;
            this.portType = portType;
            this.type = type;
            this.isArray = isArray;
            this.isParams = isParams;
        }
        public string GetDisplayName()
        {
            return propertyName;
        }
    }
}
