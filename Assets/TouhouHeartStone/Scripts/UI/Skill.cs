using TouhouHeartstone;
using System;
namespace UI
{
    partial class Skill
    {
        public TouhouCardEngine.Card card { get; private set; } = null;
        [Obsolete]
        public void update(Table table, THHPlayer self, THHPlayer player, TouhouCardEngine.Card card, CardSkinData skin)
        {
            this.card = card;

            Image.sprite = skin.image;
            CostText.text = card.getCost().ToString();
            if (card.isUsed())
            {
                IsUsedController = IsUsed.True;
            }
            else
                IsUsedController = IsUsed.False;
            if (player == self
                && card.isUsable(table.game, player, out _)//技能是可用的
                && table.selectableTargets == null//没有在选择目标
                && table.canControl//是自己的回合
                )
            {
                IsUsableController = IsUsable.True;
                asButton.interactable = true;
            }
            else
            {
                IsUsableController = IsUsable.False;
                asButton.interactable = false;
            }
        }
    }
}
