
using UnityEngine;

namespace UI
{
    partial class HandList
    {
        public bool isExpanded
        {
            get { return animator.GetBool("IsExpanded"); }
            private set { animator.SetBool("IsExpanded", value); }
        }
        public void expand()
        {
            display();
            isExpanded = true;
            Table table = GetComponentInParent<Table>();
            table.BlockerButton.gameObject.SetActive(true);
            table.BlockerButton.GetComponent<RectTransform>().SetSiblingIndex(rectTransform.GetSiblingIndex() - 1);
            table.BlockerButton.onClick.RemoveAllListeners();
            table.BlockerButton.onClick.AddListener(() =>
            {
                shrink();
            });
        }
        public void shrink()
        {
            display();
            isExpanded = false;
            Table table = GetComponentInParent<Table>();
            table.BlockerButton.gameObject.SetActive(false);
        }
    }
}
