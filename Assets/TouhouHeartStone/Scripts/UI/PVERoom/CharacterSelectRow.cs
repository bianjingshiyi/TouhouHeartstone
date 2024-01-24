using System;
using UnityEngine;

namespace UI
{
    public class CharacterSelectRow : MonoBehaviour
    {
        public CharacterSelectItem getCharacterItem(int index)
        {
            return _itemList.getElement<CharacterSelectItem>(index);
        }
        public void update(CharacterViewData[] viewDatas)
        {
            _itemList.updateList(viewDatas.Length,
                (i, rect) =>
                {
                    var item = rect.GetComponent<CharacterSelectItem>();
                    item.update(viewDatas[i]);
                },
                rect =>
                {
                    var item = rect.GetComponent<CharacterSelectItem>();
                    item.onClick += onItemClickedCallback;
                },
                rect =>
                {
                    var item = rect.GetComponent<CharacterSelectItem>();
                    item.onClick -= onItemClickedCallback;
                });
        }
        private void onItemClickedCallback(CharacterSelectItem item)
        {
            onItemClicked?.Invoke(_itemList.indexOf(item));
        }
        public event Action<int> onItemClicked;
        [SerializeField]
        private ElementList _itemList;
    }
}