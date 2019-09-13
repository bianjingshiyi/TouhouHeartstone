namespace TouhouHeartstone
{
    public class PropertyModifier
    {
        public string propName { get; }
        public PropertyChangeType changeType { get; }
        public object value { get; }
        public PropertyModifier(string propName, object value)
        {
            this.propName = propName;
            this.value = value;
            changeType = PropertyChangeType.set;
        }
        public PropertyModifier(string propName, PropertyChangeType changeType, int value)
        {
            this.propName = propName;
            this.changeType = changeType;
            this.value = value;
        }
    }
}