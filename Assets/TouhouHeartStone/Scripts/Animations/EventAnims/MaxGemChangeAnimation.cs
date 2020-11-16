using TouhouHeartstone;
using UI;
namespace Game
{
    class MaxGemChangeAnimation : EventAnimation<THHPlayer.SetMaxGemEventArg>
    {
        public override bool update(TableManager table, THHPlayer.SetMaxGemEventArg eventArg)
        {
            return true;
        }
    }
}
