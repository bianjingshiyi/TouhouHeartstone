using Game;
using System.Linq;
using TouhouHeartstone;
using UI;
using BJSYGameCore.UI;
namespace Game
{
    class InitReplaceRequestAnimation : RequestAnimation<InitReplaceRequest>
    {
        public override bool display(TableManager table, InitReplaceRequest request)
        {
            if (!table.ui.InitReplaceDialog.isDisplaying)
            {
                table.ui.InitReplaceDialog.display();
                table.ui.InitReplaceDialog.InitReplaceCardList.clearItems();
                table.ui.InitReplaceDialog.InitReplaceCardList.updateItems(table.player.getInit(), (i, c) => i.Card.card == c, (item, card) =>
                {
                    item.Card.card = card;//TODO:如果不这么做会导致下面的换卡逻辑无法使用，这口屎只能暂时吃了。
                    table.setCard(item.Card, card, true);
                    item.MarkImage.enabled = false;
                    item.asButton.onClick.set(() =>
                    {
                        item.MarkImage.enabled = !item.MarkImage.enabled;
                    });
                });
                table.ui.InitReplaceDialog.InitReplaceCardList.sortItems((a, b) => table.player.getInit().indexOf(a.Card.card) - table.player.getInit().indexOf(b.Card.card));
                table.ui.InitReplaceDialog.ConfirmButtonBlack.gameObject.SetActive(true);
                table.ui.InitReplaceDialog.WaitingText.SetActive(false);
                table.ui.InitReplaceDialog.ConfirmButtonBlack.asButton.onClick.set(() =>
                {
                    table.player.cmdInitReplace(table.game, table.ui.InitReplaceDialog.InitReplaceCardList.Where(item => item.MarkImage.enabled).Select(item => item.Card.card).ToArray());
                    table.ui.InitReplaceDialog.ConfirmButtonBlack.gameObject.SetActive(false);
                    table.ui.InitReplaceDialog.WaitingText.SetActive(true);
                });
            }
            return true;
        }
    }
}
