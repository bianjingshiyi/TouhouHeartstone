using TouhouHeartstone;
using BJSYGameCore.UI;
namespace UI
{
    class MoveServantAnimation : Animation<THHPlayer.MoveEventArg>
    {
        public MoveServantAnimation(THHPlayer.MoveEventArg eventArg) : base(eventArg)
        {
        }
        public override bool update(Table table)
        {
            if (eventArg.player == table.player)
            {
                var item = table.SelfFieldList.addItem();
                item.rectTransform.SetSiblingIndex(eventArg.position);
                item.update(eventArg.player, eventArg.card, table.getSkin(eventArg.card));
            }
            else
            {
                var item = table.EnemyFieldList.addItem();
                item.rectTransform.SetSiblingIndex(eventArg.position);
                item.update(eventArg.player, eventArg.card, table.getSkin(eventArg.card));
            }
            return true;
        }
    }
}
