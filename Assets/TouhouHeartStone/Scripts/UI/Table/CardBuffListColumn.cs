using System.Collections.Generic;
using BJSYGameCore.UI;
using UnityEngine;

namespace UI
{
    public class CardBuffListColumn : MonoBehaviour
    {
        public void updateBuffs(CardBuffListItemViewData[] viewDatas)
        {
            UIHelper.updateList(_itemRoot, _itemTemplate, _itemList, viewDatas.Length, (index, rect) =>
            {
                var item = rect.GetComponent<CardBuffListItem>();
                item.setBuff(viewDatas[index]);
            });
        }

        [SerializeField]
        RectTransform _itemRoot;
        [SerializeField]
        RectTransform _itemTemplate;
        [SerializeField]
        List<RectTransform> _itemList;
    }
}
