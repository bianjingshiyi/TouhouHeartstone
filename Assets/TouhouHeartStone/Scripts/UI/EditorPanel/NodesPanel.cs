using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BJSYGameCore.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class NodesPanel : MonoBehaviour
    {
        #region 公有方法
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
                item.setNode(viewData.name, viewData.icon, viewData.editable);
            },
            rect =>
            {
                var item = rect.GetComponent<NodesPanelItem>();
                item.onBeginDrag += onBeginDragCallback;
                item.onDrag += onDragCallback;
                item.onEndDrag += onEndDragCallback;
                item.onEditClicked += onEditClickedCallback;
                item.onDoubleClicked += onItemDoubleClickedCallback;
            },
            rect =>
            {
                var item = rect.GetComponent<NodesPanelItem>();
                item.onBeginDrag -= onBeginDragCallback;
                item.onDrag -= onDragCallback;
                item.onEndDrag -= onEndDragCallback;
                item.onEditClicked -= onEditClickedCallback;
                item.onDoubleClicked -= onItemDoubleClickedCallback;
            });
        }
        public void setCategoryFilter(string category)
        {
            _categoryFilter.setValueWithoutNotify(category);
        }
        public void setOutputFilter(bool[] outputs)
        {
            _outputFilter.SetValueWithoutNotify(outputs ?? Array.Empty<bool>());
        }
        public void setSearchFilter(string search)
        {
            _searchFilter.SetTextWithoutNotify(search);
        }
        #endregion
        #region 私有方法
        #region 生命周期
        private void Awake()
        {
            _categoryFilter.onValueChanged += onCategoryFilterEndEditCallback;
            _outputFilter.onValueChanged.AddListener(onOutputFilterValueChangedCallback);
            _searchFilter.onEndEdit.AddListener(onSearchFilterEndEditCallback);
            _createActionButton.onClick.AddListener(onCreateActionClickedCallback);
        }
        #endregion
        #region 事件回调
        private async void onBeginDragCallback(NodesPanelItem item, PointerEventData data)
        {
            bool dragged = false;
            if (onItemBeginDrag != null)
            {
                dragged = await onItemBeginDrag(_nodes.IndexOf(item.transform as RectTransform), data);
            }
            if (!dragged)
            {
                _nodeScrollRect.OnBeginDrag(data);
            }
        }
        private async void onDragCallback(NodesPanelItem item, PointerEventData data)
        {
            bool dragged = false;
            if (onItemDrag != null)
            {
                dragged = await onItemDrag(_nodes.IndexOf(item.transform as RectTransform), data);
            }
            if (!dragged)
            {
                _nodeScrollRect.OnDrag(data);
            }
        }
        private async void onEndDragCallback(NodesPanelItem item, PointerEventData data)
        {
            bool dragged = false;
            if (onItemDrag != null)
            {
                dragged = await onItemEndDrag(_nodes.IndexOf(item.transform as RectTransform), data);
            }
            if (!dragged)
            {
                _nodeScrollRect.OnEndDrag(data);
            }
        }
        private void onEditClickedCallback(NodesPanelItem item)
        {
            onItemEditClicked?.Invoke(_nodes.IndexOf(item.transform as RectTransform));
        }
        private void onItemDoubleClickedCallback(NodesPanelItem item)
        {
            onItemDoubleClicked?.Invoke(_nodes.IndexOf(item.transform as RectTransform));
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
        private void onCreateActionClickedCallback()
        {
            onCreateActionClicked?.Invoke();
        }
        #endregion
        #endregion
        #region 事件
        public event Func<int, PointerEventData, Task<bool>> onItemBeginDrag;
        public event Func<int, PointerEventData, Task<bool>> onItemDrag;
        public event Func<int, PointerEventData, Task<bool>> onItemEndDrag;
        public event Action<int> onItemEditClicked;
        public event Action<int> onItemDoubleClicked;

        public event Action<string> onCategoryFilterEndEdit;
        public event Action<bool[]> onOutputFilterValueChanged;
        public event Action<string> onSearchFilterEndEdit;
        public event Action onCreateActionClicked;
        #endregion

        #region 属性字段
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
        [SerializeField]
        private Button _createActionButton;
        #endregion
    }

    public struct NodeViewData
    {
        public Sprite icon;
        public string name;
        public bool editable;
        public NodeViewData(string name, Sprite icon, bool editable)
        {
            this.name = name;
            this.icon = icon;
            this.editable = editable;
        }
    }
}