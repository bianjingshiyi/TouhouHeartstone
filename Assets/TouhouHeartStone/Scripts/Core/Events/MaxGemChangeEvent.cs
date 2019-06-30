namespace TouhouHeartstone
{
    class MaxGemChangeEvent : VisibleEvent
    {
        public MaxGemChangeEvent(Player player, int value) : base("onMaxGemChange")
        {
            this.player = player;
            this.value = value;
        }
        Player player { get; }
        int value { get; }
        public override void execute(CardEngine engine)
        {
            player.setProp("maxGem", value);
            if (player.getProp<int>("maxGem") < 0)
                player.setProp("maxGem", 0);
            else if (player.getProp<int>("maxGem") > 10)
                player.setProp("maxGem", 10);
        }
        public override EventWitness getWitness(CardEngine engine, Player player)
        {
            EventWitness witness = new MaxGemChangeWitness();
            witness.setVar("playerIndex", engine.getPlayerIndex(this.player));
            witness.setVar("value", this.player.getProp<int>("maxGem"));
            return witness;
        }
    }
    /// <summary>
    /// 最大宝石数量改变事件
    /// </summary>
    public class MaxGemChangeWitness : EventWitness
    {
        /// <summary>
        /// 最大宝石数量改变的玩家索引
        /// </summary>
        public int playerIndex
        {
            get { return getVar<int>("playerIndex"); }
        }
        /// <summary>
        /// 改变后的最大宝石数量
        /// </summary>
        public int value
        {
            get { return getVar<int>("value"); }
        }
        public MaxGemChangeWitness() : base("onMaxGemChange")
        {
        }
    }
}