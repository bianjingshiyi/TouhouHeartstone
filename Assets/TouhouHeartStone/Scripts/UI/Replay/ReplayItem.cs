using BJSYGameCore;
using BJSYGameCore.UI;
using Game;
using TouhouCardEngine;

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
            _playerAImage.sprite = ui.getManager<CardManager>().getCardSkin(viewData.playerAMasterDefRef).image;
            _playerBImage.sprite = ui.getManager<CardManager>().getCardSkin(viewData.playerBMasterDefRef).image;
            _nameText.text = viewData.name;
        }

        public void setSelected(bool selected)
        {
            _selected.gameObject.SetActive(selected);
        }
    }
    public class ReplayItemViewData
    {
        public DefineReference playerAMasterDefRef;
        public DefineReference playerBMasterDefRef;
        public string name;
    }
}
