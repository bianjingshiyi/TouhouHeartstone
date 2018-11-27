namespace TouhouHeartstone.Backend
{
    /// <summary>
    /// 这个炉石规则是测试用的。
    /// </summary>
    class HeartStoneRule : Rule
    {
        public override void onInit(Game game)
        {
        }
        public override int getFirstPlayer(Game game)
        {
            return 0;
        }
    }
}