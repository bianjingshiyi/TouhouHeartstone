namespace TouhouHeartstone
{
    class GamePropChangeEvent : VisibleEvent
    {
        public GamePropChangeEvent(string propName, PropertyChangeType changeType, object value) : base("onGamePropChange")
        {
            this.propName = propName;
            this.changeType = changeType;
            this.value = value;
        }
        string propName { get; }
        PropertyChangeType changeType { get; }
        object value { get; }
        public override void execute(CardEngine engine)
        {
            if (changeType == PropertyChangeType.add)
            {
                if (engine.dicVar[propName] is int)
                    engine.dicVar[propName] = (int)engine.dicVar[propName] + (int)value;
                else if (engine.dicVar[propName] is float)
                    engine.dicVar[propName] = (float)engine.dicVar[propName] + (float)value;
                else if (engine.dicVar[propName] is string)
                    engine.dicVar[propName] = (string)engine.dicVar[propName] + (string)value;
            }
            else
                engine.dicVar[propName] = value;
        }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new EventWitness("onGamePropChange");
            witness.setVar("propName", propName);
            witness.setVar("changeType", changeType);
            witness.setVar("value", value);
            return witness;
        }
    }
}