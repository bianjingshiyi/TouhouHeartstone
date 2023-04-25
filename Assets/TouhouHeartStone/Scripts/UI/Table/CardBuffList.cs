using System.Collections.Generic;
using System.Linq;
using BJSYGameCore.UI;
using UnityEngine;

namespace UI
{
    public class CardBuffList : MonoBehaviour
    {
        public void updateBuffs(CardBuffListItemViewData[] viewDatas)
        {
            int count = viewDatas.Length > 0 ? 1 : 0;
            count = _itemCountPerColumn > 0 ? Mathf.CeilToInt((float)viewDatas.Length / _itemCountPerColumn) : count;
            UIHelper.updateList(_columnRoot, _columnTemplate, _columnList, count, (index, rect) =>
            {
                var item = rect.GetComponent<CardBuffListColumn>();
                var buffDatas = _itemCountPerColumn > 0 ? viewDatas.Skip(index * _itemCountPerColumn).Take(_itemCountPerColumn).ToArray() : viewDatas;
                item.updateBuffs(buffDatas);
            });
        }
        [SerializeField]
        int _itemCountPerColumn = 5;
        [SerializeField]
        RectTransform _columnRoot;
        [SerializeField]
        RectTransform _columnTemplate;
        [SerializeField]
        List<RectTransform> _columnList;
    }
}
