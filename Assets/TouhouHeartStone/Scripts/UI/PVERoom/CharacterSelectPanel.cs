using System;
using System.Collections.Generic;
using System.Linq;
using BJSYGameCore.UI;
using UnityEngine;

namespace UI
{
    public class CharacterSelectPanel : MonoBehaviour
    {
        public void update(CharacterViewData[] viewDatas)
        {
            var rowCount = Mathf.CeilToInt(viewDatas.Length / _iconsPerRow);
            UIHelper.updateList(_rowListRoot, _rowTemplate, _rowList, rowCount,
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
        private float _iconsPerRow = 5;
        [SerializeField]
        private RectTransform _rowListRoot;
        [SerializeField]
        private RectTransform _rowTemplate;
        [SerializeField]
        private List<RectTransform> _rowList = new List<RectTransform>();
    }
    public struct CharacterViewData
    {
        public string name;
        public Sprite image;
    }
}