using TouhouHeartstone;
using UI;
namespace Game
{
    class MaxGemChangeAnimation : EventAnimation<HearthStoneRule.SetMaxGemEventArg>
    {
        public override bool update(TableManager table, HearthStoneRule.SetMaxGemEventArg eventArg, float time)
        {
            return true;
        }
    }
}
