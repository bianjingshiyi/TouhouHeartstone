using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine;
using TouhouHeartstone;
using BJSYGameCore.UI;
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
                if (!_isSelectingTarget)
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
        private void Update()
        {
            EventSystem eventSystem = EventSystem.current;
            BaseInputModule module = eventSystem.currentInputModule;
            var input = module.input;
            Table table = GetComponentInParent<Table>();
            if (placingCard != null)
            {
                if (!_isSelectingTarget)
                {
                    //拖拽卡片
                    placingCard.rectTransform.position = input.mousePosition;
                    if (rectTransform.rect.Contains(rectTransform.InverseTransformPoint(input.mousePosition)))
                    {
                        //如果移动回手牌区域，恢复正常大小
                        placingCard.rectTransform.localScale = Vector3.one;
                        if (input.GetMouseButtonUp(0))
                        {
                            //如果松开，停止拖拽
                            stopPlacing();
                        }
                        //移除随从占位
                        removePlaceHolder();
                    }
                    else
                    {
                        //移动到手牌区以外的地方视作打算使用
                        if (!placingCard.card.isUsable(table.game, table.player, out string info))
                        {
                            //卡牌不可用，停止拖拽并提示
                            table.showTip(info);
                            stopPlacing();
                        }
                        else
                        {
                            //手牌在战场上大小和随从牌一致
                            placingCard.rectTransform.localScale = Vector3.one * .4f / rectTransform.localScale.y;
                            if (placingCard.card.define is ServantCardDefine)
                            {
                                var servants = table.SelfFieldList.getItems();
                                int index = 0;
                                if (servants.Length > 0)
                                {
                                    //需要选择空位，添加占位
                                    table.SelfFieldList.addChild(table.ServantPlaceHolder.rectTransform);
                                    for (int i = 0; i < servants.Length; i++)
                                    {
                                        if (servants[i].rectTransform.position.x < input.mousePosition.x)
                                            index = i + 1;
                                    }
                                    table.ServantPlaceHolder.rectTransform.SetSiblingIndex(index);
                                    table.ServantPlaceHolder.display();
                                    defaultItem.rectTransform.SetAsFirstSibling();
                                }
                                if (input.GetMouseButtonUp(0))
                                {
                                    //松开鼠标，确认使用随从牌
                                    if (placingCard.card.isUsable(table.game, table.player, out info))
                                    {
                                        if (placingCard.card.getAvaliableTargets(table.game) is TouhouCardEngine.Card[] targets && targets.Length > 0)
                                        {
                                            //进入选择目标状态，固定手牌到占位上，高亮可以选择的目标
                                            _isSelectingTarget = true;
                                            placingCard.hide();
                                            table.ServantPlaceHolder.Servant.display();
                                            table.ServantPlaceHolder.Servant.update(placingCard.card.define, table.getSkin(placingCard.card));
                                            table.selectableTargets = targets.Select(target =>
                                            {
                                                if (table.getMaster(target) is Master master)
                                                    return master as UIObject;
                                                else if (table.getServant(target) is Servant servant)
                                                    return servant as UIObject;
                                                throw new ActorNotFoundException(target);
                                            }).ToArray();
                                            shrink();
                                        }
                                        else
                                        {
                                            //使用无目标随从牌
                                            table.player.cmdUse(table.game, placingCard.card, index);
                                            stopPlacing();
                                        }
                                    }
                                    else
                                    {
                                        //无法使用随从牌
                                        table.showTip(info);
                                        stopPlacing();
                                    }
                                }
                            }
                        }
                    }
                }
                else if (_isSelectingTarget)
                {
                    if (input.GetMouseButtonUp(0))
                    {
                        List<RaycastResult> raycastList = new List<RaycastResult>();
                        eventSystem.RaycastAll(new PointerEventData(eventSystem) { position = input.mousePosition }, raycastList);
                        foreach (var raycast in raycastList)
                        {
                            if (raycast.gameObject.GetComponentInParent<Master>() is Master master)
                            {
                                if (placingCard.card.isValidTarget(table.game, master.card))
                                {
                                    table.player.cmdUse(table.game, placingCard.card, table.ServantPlaceHolder.transform.GetSiblingIndex(), master.card);
                                }
                                else
                                {
                                    table.showTip("这不是一个有效的目标！");
                                }
                                break;
                            }
                            else if (raycast.gameObject.GetComponentInParent<Servant>() is Servant servant)
                            {
                                if (placingCard.card.isValidTarget(table.game, servant.card))
                                {
                                    table.player.cmdUse(table.game, placingCard.card, table.ServantPlaceHolder.transform.GetSiblingIndex(), servant.card);
                                }
                                else
                                {
                                    table.showTip("这不是一个有效的目标！");
                                }
                                break;
                            }
                        }
                        stopPlacing();
                    }
                }
            }
        }
        [SerializeField]
        Vector2 _dragStartPosition;
        [SerializeField]
        Card _placingCard;
        public Card placingCard
        {
            get { return _placingCard; }
        }
        [SerializeField]
        bool _isSelectingTarget = false;
        public void startPlacing(Vector2 startPosition, Card card)
        {
            _dragStartPosition = startPosition;
            _placingCard = card;
        }
        public void stopPlacing()
        {
            if (_placingCard != null)
            {
                placingCard.display();
                placingCard.rectTransform.localScale = Vector3.one;
                placingCard.rectTransform.localPosition = Vector2.zero;
                _placingCard = null;
                _dragStartPosition = Vector2.zero;
            }
            removePlaceHolder();
            Table table = GetComponentInParent<Table>();
            _isSelectingTarget = false;
            table.selectableTargets = null;
        }
        private void removePlaceHolder()
        {
            Table table = GetComponentInParent<Table>();
            table.addChild(table.ServantPlaceHolder.rectTransform);
            table.ServantPlaceHolder.Servant.hide();
            table.ServantPlaceHolder.hide();
        }
    }
}
