using UI;
using TouhouHeartstone;
using TouhouCardEngine;
namespace Game
{
    static class UIHelper
    {
        public static void setCard(this UI.Card ui, CardDefine card, CardSkinData skin)
        {
            ui.CostPropNumber.asText.text = card.getCost().ToString();

            ui.Image.sprite = skin.image;
            ui.NameText.text = skin.name;
            ui.DescText.text = skin.desc;
            ui.isFaceup = true;

            if (card.type == DEFINE.SERVANT)
            {
                ui.type = CATEGORY.SERVANT;
                ui.AttackPropNumber.asText.text = card.getAttack().ToString();
                ui.LifePropNumber.asText.text = card.getLife().ToString();
            }
            else
                ui.type = CATEGORY.SPELL;
        }
    }
}