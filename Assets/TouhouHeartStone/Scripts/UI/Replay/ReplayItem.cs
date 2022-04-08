using BJSYGameCore;
using BJSYGameCore.UI;
using Game;

namespace UI
{
    partial class ReplayItem : UIObject
    {
        public void onAwake()
        {
            autoInit();
        }
        public void updateView(ReplayItemViewData viewData)
        {
            _playerAImage.sprite = ui.getManager<CardManager>().getCardSkin(viewData.playerAMasterId).image;
            _playerBImage.sprite = ui.getManager<CardManager>().getCardSkin(viewData.playerBMasterId).image;
            _nameText.text = viewData.name;
        }

        public void setSelected(bool selected)
        {
            _selected.gameObject.SetActive(selected);
        }
    }
    public class ReplayItemViewData
    {
        public int playerAMasterId;
        public int playerBMasterId;
        public string name;
    }
}
