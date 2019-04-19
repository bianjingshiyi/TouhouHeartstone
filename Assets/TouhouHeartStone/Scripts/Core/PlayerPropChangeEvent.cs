namespace TouhouHeartstone
{
    class PlayerPropChangeEvent : VisibleEvent
    {
        public PlayerPropChangeEvent(Player player, string propName, PropertyChangeType changeType, object value) : base("onPlayerPropChange")
        {
            this.player = player;
            this.propName = propName;
            this.changeType = changeType;
            this.value = value;
        }
        Player player { get; }
        string propName { get; }
        PropertyChangeType changeType { get; }
        object value { get; }
        public override void execute(CardEngine engine)
        {
            if (changeType == PropertyChangeType.add)
            {
                if (!player.dicProp.ContainsKey(propName))
                    player.dicProp[propName] = value;
                else if (player.dicProp[propName] is int)
                    player.dicProp[propName] = (int)player.dicProp[propName] + (int)value;
                else if (player.dicProp[propName] is float)
                    player.dicProp[propName] = (float)player.dicProp[propName] + (float)value;
                else if (player.dicProp[propName] is string)
                    player.dicProp[propName] = (string)player.dicProp[propName] + (string)value;
                else
                    player.dicProp[propName] = value;
            }
            else
                player.dicProp[propName] = value;
        }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new EventWitness("onPlayerPropChange");
            witness.setVar("playerIndex", engine.getPlayerIndex(this.player));
            witness.setVar("propName", propName);
            witness.setVar("changeType", changeType);
            witness.setVar("value", value);
            return witness;
        }
    }
}