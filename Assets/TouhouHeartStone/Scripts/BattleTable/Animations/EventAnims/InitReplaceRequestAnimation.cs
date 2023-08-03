using TouhouHeartstone;
namespace Game.BattleTable
{
    class InitReplaceRequestAnimation : RequestAnimation<InitReplaceRequest>
    {
        public override bool display(BattleTableManager table, InitReplaceRequest request)
        {
            table.showInitReplaceDialog(request);
            return true;
        }
    }
}
