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
            // TODO: 以后重新开放回放的时候要加入这场回放使用的卡池引用列表，不然这俩人物头像没法正确获取
            //_playerAImage.sprite = ui.getManager<CardManager>().getCardSkin(viewData.playerAMasterDefRef).image;
            //_playerBImage.sprite = ui.getManager<CardManager>().getCardSkin(viewData.playerBMasterDefRef).image;
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
