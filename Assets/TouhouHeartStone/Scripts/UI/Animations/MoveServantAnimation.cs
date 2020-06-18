using TouhouHeartstone;
using BJSYGameCore.UI;
namespace UI
{
    class MoveServantAnimation : UIAnimation<THHPlayer.MoveEventArg>
    {
        public MoveServantAnimation(THHPlayer.MoveEventArg eventArg) : base(eventArg)
        {
        }
        public override bool update(Table table)
        {
            if (eventArg.player == table.player)
            {
                var item = table.SelfFieldList.addItem();
                table.SelfFieldList.defaultItem.rectTransform.SetAsFirstSibling();
                item.rectTransform.SetSiblingIndex(eventArg.position + 1);
                item.update(eventArg.player, eventArg.card, table.getSkin(eventArg.card));
            }
            else
            {
                var item = table.EnemyFieldList.addItem();
                table.EnemyFieldList.defaultItem.rectTransform.SetAsFirstSibling();
                item.rectTransform.SetSiblingIndex(eventArg.position + 1);
                item.update(eventArg.player, eventArg.card, table.getSkin(eventArg.card));
            }
            return true;
        }
    }
}
