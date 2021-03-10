using BJSYGameCore;
using BJSYGameCore.UI;
using Game;

namespace UI
{
    partial class ReplayItem : UIObject
    {
        public void update(ReplayItemViewData viewData)
        {
            _playerAImage.sprite = ui.getManager<CardManager>().getSkin(viewData.playerAMasterId).image;
            _playerBImage.sprite = ui.getManager<CardManager>().getSkin(viewData.playerBMasterId).image;
            _nameText.text = viewData.name;
        }
    }
    class ReplayItemViewData
    {
        public int playerAMasterId;
        public int playerBMasterId;
        public string name;
    }
}
