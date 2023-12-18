namespace UI
{
    public struct ActionNodeValueInfo
    {
        public ActionNodeValueInfo(string name, NodeValueType type, object value) : this(name, type, value, null, type)
        {
        }
        public ActionNodeValueInfo(string displayName, string name, NodeValueType type, object value) : this(displayName, name, type, value, null, type)
        {
        }
        public ActionNodeValueInfo(string name, NodeValueType type, object value, string[] typeNames, NodeValueType assignType) : this(name, name, type, value, typeNames, assignType)
        {
        }
        public ActionNodeValueInfo(string displayName, string name, NodeValueType type, object value, string[] typeNames, NodeValueType assignType) : this(displayName, name, type, value, null, null)
        {
            assignableTypeNames = typeNames;
            assignableTypeValues = assignType.getAssignableValueTypes();
        }
        public ActionNodeValueInfo(string name, NodeValueType type, object value, string[] optionalNames, object[] optionalValues) :
            this(name, name, type, value, optionalNames, optionalValues)
        {
        }
        public ActionNodeValueInfo(string displayName, string name, NodeValueType type, object value, string[] optionalNames, object[] optionalValues)
        {
            this.displayName = displayName;
            this.name = name;
            this.type = type;
            this.value = value;
            this.optionalNames = optionalNames;
            this.optionalValues = optionalValues;
            assignableTypeNames = null;
            assignableTypeValues = null;
        }
        public string displayName;
        public string name;
        public NodeValueType type;
        public object value;
        public string[] optionalNames;
        public object[] optionalValues;
        public string[] assignableTypeNames;
        public NodeValueType[] assignableTypeValues;
    }
}
