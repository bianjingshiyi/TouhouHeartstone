using System.Collections.Generic;
using BJSYGameCore.UI;
using UnityEngine;

namespace UI
{
    public class DeckCardList : MonoBehaviour
    {
        public void update((string name, Sprite image, int count)[] viewDatas)
        {
            UIHelper.updateList(_deckCardListRoot, _deckCardItemTemplate, _deckCardList, viewDatas.Length,
                (i, item) =>
                {
                    var listItem = item.GetComponent<CardViewDeckCardListItem>();
                    var viewData = viewDatas[i];
                    listItem.updateView(viewData.name, viewData.image, viewData.count);
                });
        }
        [SerializeField]
        RectTransform _deckCardListRoot;
        [SerializeField]
        RectTransform _deckCardItemTemplate;
        [SerializeField]
        List<RectTransform> _deckCardList = new List<RectTransform>();
    }
}
