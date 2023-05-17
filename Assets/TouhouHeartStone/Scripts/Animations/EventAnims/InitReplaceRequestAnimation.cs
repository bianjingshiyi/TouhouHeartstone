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
                table.ui.InitReplaceDialog.InitReplaceCardList.updateItems(table.player.getInit(), (i, c) => table.getInitCard(i) == c, (item, card) =>
                {
                    table.setInitCard(item, table.newSnapshot(card));
                    table.setCard(item.Card, table.newSnapshot(card), true);
                    item.MarkImage.enabled = false;
                    item.asButton.onClick.set(() =>
                    {
                        item.MarkImage.enabled = !item.MarkImage.enabled;
                    });
                });
                table.ui.InitReplaceDialog.InitReplaceCardList.sortItems((a, b) => table.player.getInit().indexOf(table.getInitCard(a)) - table.player.getInit().indexOf(table.getInitCard(b)));
                table.ui.InitReplaceDialog.ConfirmButtonBlack.gameObject.SetActive(true);
                table.ui.InitReplaceDialog.WaitingText.SetActive(false);
                table.ui.InitReplaceDialog.ConfirmButtonBlack.asButton.onClick.set(() =>
                {
                    table.player.cmdInitReplace(table.game, table.ui.InitReplaceDialog.InitReplaceCardList.Where(item => item.MarkImage.enabled).Select(item => table.getInitCard(item)).ToArray());
                    table.ui.InitReplaceDialog.ConfirmButtonBlack.gameObject.SetActive(false);
                    table.ui.InitReplaceDialog.WaitingText.SetActive(true);
                });
            }
            return true;
        }
    }
}
