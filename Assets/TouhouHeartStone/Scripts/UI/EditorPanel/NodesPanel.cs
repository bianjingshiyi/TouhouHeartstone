using System;
using System.Collections.Generic;
using BJSYGameCore.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class NodesPanel : MonoBehaviour
    {
        public void initCategories(string[] names, string[] values)
        {
            _categoryFilter.setDropdownValues(names, values);
            if (values != null && values.Length > 0)
            {
                _categoryFilter.setValueWithoutNotify(values[0]);
            }
        }
        public void initTypes(string[] names)
        {
            _outputFilter.ClearOptions();
            _outputFilter.AddOptions(new List<string>(names));
        }
        public void updateList(NodeViewData[] nodes)
        {
            UIHelper.updateList(_nodesRoot, _nodeTemplate, _nodes, nodes.Length, (i, rect) =>
            {
                var item = rect.GetComponent<NodesPanelItem>();
                var viewData = nodes[i];
                item.setNode(viewData.name, viewData.icon);
            },
            rect =>
            {
                var item = rect.GetComponent<NodesPanelItem>();
                item.onBeginDrag += onBeginDragCallback;
                item.onDrag += onDragCallback;
                item.onEndDrag += onEndDragCallback;
            },
            rect =>
            {
                var item = rect.GetComponent<NodesPanelItem>();
                item.onBeginDrag -= onBeginDragCallback;
                item.onDrag -= onDragCallback;
                item.onEndDrag -= onEndDragCallback;
            });
        }

        private void Awake()
        {
            _categoryFilter.onValueChanged += onCategoryFilterEndEditCallback;
            _outputFilter.onValueChanged.AddListener(onOutputFilterValueChangedCallback);
            _searchFilter.onEndEdit.AddListener(onSearchFilterEndEditCallback);
        }
        private void onBeginDragCallback(NodesPanelItem item, PointerEventData data)
        {
            var dragged = onItemBeginDrag?.Invoke(_nodes.IndexOf(item.transform as RectTransform), data) ?? false;
            if (!dragged)
            {
                _nodeScrollRect.OnBeginDrag(data);
            }
        }
        private void onDragCallback(NodesPanelItem item, PointerEventData data)
        {
            var dragged = onItemDrag?.Invoke(_nodes.IndexOf(item.transform as RectTransform), data) ?? false;
            if (!dragged)
            {
                _nodeScrollRect.OnDrag(data);
            }
        }
        private void onEndDragCallback(NodesPanelItem item, PointerEventData data)
        {
            var dragged = onItemEndDrag?.Invoke(_nodes.IndexOf(item.transform as RectTransform), data) ?? false;
            if (!dragged)
            {
                _nodeScrollRect.OnEndDrag(data);
            }
        }
        private void onCategoryFilterEndEditCallback(string value)
        {
            onCategoryFilterEndEdit?.Invoke(value);
        }
        private void onOutputFilterValueChangedCallback(bool[] value)
        {
            onOutputFilterValueChanged?.Invoke(value);
        }
        private void onSearchFilterEndEditCallback(string value)
        {
            onSearchFilterEndEdit?.Invoke(value);
        }


        public event Func<int, PointerEventData, bool> onItemBeginDrag;
        public event Func<int, PointerEventData, bool> onItemDrag;
        public event Func<int, PointerEventData, bool> onItemEndDrag;

        public event Action<string> onCategoryFilterEndEdit;
        public event Action<bool[]> onOutputFilterValueChanged;
        public event Action<string> onSearchFilterEndEdit;

        [SerializeField]
        private ScrollRect _nodeScrollRect;
        [SerializeField]
        private RectTransform _nodeTemplate;
        [SerializeField]
        private RectTransform _nodesRoot;
        [SerializeField]
        private List<RectTransform> _nodes;
        [SerializeField]
        private InputDropdown _categoryFilter;
        [SerializeField]
        private MultiSelectDropdown _outputFilter;
        [SerializeField]
        private InputField _searchFilter;
    }

    public struct NodeViewData
    {
        public Sprite icon;
        public string name;
        public NodeViewData(string name, Sprite icon)
        {
            this.name = name;
            this.icon = icon;
        }
    }
}