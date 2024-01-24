using System;
using System.Linq;
using UnityEngine;

namespace UI
{
    public class CharacterSelectPanel : MonoBehaviour
    {
        public CharacterSelectItem getCharacterItem(int index)
        {
            var y = index / _iconsPerRow;
            var row = _rowList.getElement<CharacterSelectRow>(y);
            if (!row)
                return null;
            return row.getCharacterItem(index % _iconsPerRow);
        }
        public void update(CharacterViewData[] viewDatas)
        {
            var rowCount = Mathf.CeilToInt(viewDatas.Length / (float)_iconsPerRow);
            _rowList.updateList(rowCount,
                (index, rect) =>
                {
                    var row = rect.GetComponent<CharacterSelectRow>();
                    row.update(viewDatas.Skip(index * 5).Take(5).ToArray());
                },
                rect =>
                {
                    var row = rect.GetComponent<CharacterSelectRow>();
                    row.onItemClicked += onItemClickedCallback;
                },
                rect =>
                {
                    var row = rect.GetComponent<CharacterSelectRow>();
                    row.onItemClicked -= onItemClickedCallback;
                });
        }
        private void onItemClickedCallback(int index)
        {
            onItemClicked?.Invoke(index);
        }
        public event Action<int> onItemClicked;

        [SerializeField]
        private int _iconsPerRow = 5;
        [SerializeField]
        private ElementList _rowList;
    }
    public struct CharacterViewData
    {
        public string name;
        public Sprite image;
    }
}