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
            UIHelper.updateList(_columnRoot, _columnTemplate, _columnList, Mathf.CeilToInt((float)viewDatas.Length / _itemCountPerColumn), (index, rect) =>
            {
                var item = rect.GetComponent<CardBuffListColumn>();
                item.updateBuffs(viewDatas.Skip(index * _itemCountPerColumn).Take(_itemCountPerColumn).ToArray());
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
