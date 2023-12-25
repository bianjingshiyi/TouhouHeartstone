using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.BattleTable.UI
{
    using UIHelper = BJSYGameCore.UI.UIHelper;
    public class KeywordPopUp : MonoBehaviour
    {
        #region 公有方法

        public void update(string[] titles, string[] descs)
        {
            int targetCount = titles.Length;
            UIHelper.updateList(_rootTransform, _itemTemplate, _items, targetCount, (i, rect) =>
            {
                var item = rect.GetComponent<KeywordPopUpItem>();
                item.update(titles[i], descs[i]);
            });

            LayoutRebuilder.ForceRebuildLayoutImmediate(_rootTransform);

            List<RectTransform[]> groups = new List<RectTransform[]>();
            List<RectTransform> currentGroup = new List<RectTransform>();
            float itemHeight = 0;
            foreach (var item in _items)
            {
                itemHeight += item.sizeDelta.y;
                currentGroup.Add(item);
                if (itemHeight > _maxGroupHeight)
                {
                    itemHeight = 0;
                    groups.Add(currentGroup.ToArray());
                    currentGroup.Clear();
                }
            }
            UIHelper.updateList(_rootTransform, _groupTemplate, _groupTransforms, groups.Count, (i, rect) =>
            {
                var group = groups[i];
                foreach (var item in group)
                {
                    item.parent = rect;
                }
            });
        }

        #endregion

        #region 属性字段
        [SerializeField]
        private List<RectTransform> _groupTransforms;
        [SerializeField]
        private List<RectTransform> _items;
        [SerializeField]
        private RectTransform _rootTransform;
        [SerializeField]
        private RectTransform _groupTemplate;
        [SerializeField]
        private RectTransform _itemTemplate;
        [SerializeField]
        private float _maxGroupHeight;
        #endregion
    }
}