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


        public event Func<int, PointerEventData, bool> onItemBeginDrag;
        public event Func<int, PointerEventData, bool> onItemDrag;
        public event Func<int, PointerEventData, bool> onItemEndDrag;


        [SerializeField]
        private ScrollRect _nodeScrollRect;
        [SerializeField]
        private RectTransform _nodeTemplate;
        [SerializeField]
        private RectTransform _nodesRoot;
        [SerializeField]
        private List<RectTransform> _nodes;
        [SerializeField]
        private InputField _categoryFilter;
        [SerializeField]
        private Dropdown _outputFilter;
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