using TouhouHeartstone;
namespace UI
{
    class CreateTokenAnimation : Animation<THHPlayer.CreateTokenEventArg>
    {
        public CreateTokenAnimation(THHPlayer.CreateTokenEventArg eventArg) : base(eventArg)
        {
        }
        public override bool update(Table table)
        {
            Servant servant;
            if (eventArg.player == table.player)
                servant = table.SelfFieldList.addItem();
            else
                servant = table.EnemyFieldList.addItem();
            servant.rectTransform.SetSiblingIndex(eventArg.position);
            servant.update(eventArg.player, eventArg.card, table.getSkin(eventArg.card));
            return true;
        }
    }
}
