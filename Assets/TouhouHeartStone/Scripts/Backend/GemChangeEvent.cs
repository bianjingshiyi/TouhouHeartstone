namespace TouhouHeartstone.Backend
{
    class GemChangeEvent : VisibleEvent
    {
        public GemChangeEvent(Player player, int value) : base("onGemChange")
        {
            this.player = player;
            this.value = value;
        }
        Player player { get; }
        int value { get; }
        public override void execute(CardEngine engine)
        {
            player.setProp("gem", value);
            if (player.getProp<int>("gem") < 0)
                player.setProp("gem", 0);
            else if (player.getProp<int>("gem") > 10)
                player.setProp("gem", 10);
        }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new GemChangeWitness();
            witness.setVar("playerIndex", engine.getPlayerIndex(this.player));
            witness.setVar("value", this.player.getProp<int>("gem"));
            return witness;
        }
    }
    /// <summary>
    /// 宝石数量改变事件
    /// </summary>
    public class GemChangeWitness : EventWitness
    {
        /// <summary>
        /// 宝石数量改变的玩家索引
        /// </summary>
        public int playerIndex
        {
            get { return getVar<int>("playerIndex"); }
        }
        /// <summary>
        /// 宝石改变的数量
        /// </summary>
        public int value
        {
            get { return getVar<int>("value"); }
        }
        public GemChangeWitness() : base("onGemChange")
        {
        }
    }
}