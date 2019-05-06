namespace TouhouHeartstone.Frontend.Model
{
    public class UseCardEventArgs : System.EventArgs, ICardID, IPlayer
    {
        public int CardDID { get; set ; }
        public int CardRID { get; set; }
        public int PlayerID { get; set; }

        public override string ToString()
        {
            return "无参数";
        }
    }
    public class UseCardWithTargetArgs : UseCardEventArgs
    {
        public int TargetCardRuntimeID { get; }
        public UseCardWithTargetArgs(int runtimeID)
        {
            TargetCardRuntimeID = runtimeID;
        }
        public override string ToString()
        {
            return $"目标卡: {TargetCardRuntimeID}";
        }
    }

    public class UseCardWithPositionArgs : UseCardEventArgs
    {
        public int Position { get; }
        public UseCardWithPositionArgs(int position)
        {
            Position = position;
        }
        public override string ToString()
        {
            return $"位置: {Position}";
        }
    }

    public class UseCardWithTargetPositionArgs : UseCardEventArgs
    {
        public int Position { get; }
        public int TargetCardRuntimeID { get; }
        public UseCardWithTargetPositionArgs(int position, int runtimeID)
        {
            TargetCardRuntimeID = runtimeID;
            Position = position;
        }
        public override string ToString()
        {
            return $"位置: {Position}，目标卡: {TargetCardRuntimeID}";
        }
    }
}
