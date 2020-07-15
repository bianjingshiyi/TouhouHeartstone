using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TouhouHeartstone;
using BJSYGameCore;
using TouhouCardEngine.Interfaces;
using BJSYGameCore.UI;
using TouhouCardEngine;
using TouhouHeartstone.Builtin;
using Game;
namespace UI
{
    partial class Table : IPointerDownHandler
    {
        [Obsolete]
        public THHGame game
        {
            get { return ui.getManager<TableManager>().game; }
        }
        [Obsolete]
        public THHPlayer player
        {
            get { return ui.getManager<TableManager>().player; }
        }
        partial void onAwake()
        {
            SelfHandList.asButton.onClick.AddListener(() =>
            {
                if (SelfHandList.isExpanded)
                    SelfHandList.shrink();
                else
                    SelfHandList.expand();
            });
        }
        public void setGame(THHGame game, THHPlayer player)
        {
            InitReplaceDialog.hide();
            TurnTipImage.hide();
            SelfHandList.clearItems();
            SelfFieldList.clearItems();
            EnemyFieldList.clearItems();
            EnemyHandList.clearItems();
            AttackArrowImage.hide();
            Fatigue.hide();
        }
        [Obsolete]
        public bool canControl
        {
            get { return ui.getManager<TableManager>().canControl; }
            set { ui.getManager<TableManager>().canControl = value; }
        }
        [SerializeField]
        Projectile _defaultProjectile;
        protected void Update()
        {
            if (game == null)
                return;
            if (game.turnTimer != null && game.turnTimer.remainedTime <= 15)
            {
                TimeoutSlider.display();
                TimeoutSlider.value = game.turnTimer.remainedTime / 15;
            }
            else
                TimeoutSlider.hide();

            if (player == null)
                return;

            if (canControl)
            {
                TurnEndButton.interactable = true;
                TurnEndButton.GetComponent<Image>().color = Color.white;
            }
            else
            {
                TurnEndButton.interactable = false;
                TurnEndButton.GetComponent<Image>().color = Color.gray;
            }

            THHPlayer opponent = game.getOpponent(player);
            if (opponent == null)
                return;
        }
        public void onClickMaster(Master master, PointerEventData pointer)
        {
            if (isSelectingTarget)
            {
                if (_usingCard.isValidTarget(game, master.card))
                {
                    player.cmdUse(game, _usingCard, _usingPosition, master.card);
                    resetUse(false, false);
                }
                else
                {
                    showTip("这不是一个有效的目标！");
                    resetUse(true, true);
                }
            }
        }
        public void onClickServant(Servant servant, PointerEventData pointer)
        {
            if (isSelectingTarget)
            {
                if (_usingCard.isValidTarget(game, servant.card))
                {
                    player.cmdUse(game, _usingCard, _usingPosition, servant.card);
                    resetUse(false, false);
                }
                else
                {
                    showTip("这不是一个有效的目标！");
                    resetUse(true, true);
                }
            }
        }
        #region Skin
        public CardSkinData getSkin(TouhouCardEngine.Card card)
        {
            return getSkin(card.define);
        }
        public CardSkinData getSkin(CardDefine define)
        {
            return parent.game.getManager<CardManager>().getSkin(define.id);
        }
        #endregion
        #region Use
        [SerializeField]
        HandListItem _usingHand;
        public HandListItem usingHand
        {
            get { return _usingHand; }
            set { _usingHand = value; }
        }
        TouhouCardEngine.Card _usingCard;
        [SerializeField]
        int _usingPosition;
        [SerializeField]
        bool _isSelectingTarget = false;
        public bool isSelectingTarget
        {
            get { return _isSelectingTarget; }
            set { _isSelectingTarget = value; }
        }
        public void onDragHand(HandListItem item, PointerEventData pointer)
        {
            if (item.GetComponentInParent<HandList>() != SelfHandList)//你不能用别人的卡
                return;
            usingHand = item;
            if (!canControl)//不是你的回合
            {
                resetUse(true, true);
                return;
            }
            //拖拽卡片
            item.Card.rectTransform.position = pointer.position;
            if (SelfHandList.rectTransform.rect.Contains(SelfHandList.rectTransform.InverseTransformPoint(pointer.position)))
            {
                //如果移动回手牌区域，恢复正常大小
                item.Card.rectTransform.localScale = Vector3.one;
                //移除随从占位
                hideServantPlaceHolder();
            }
            else
            {
                //移动到手牌区以外的地方视作打算使用
                if (!item.Card.card.isUsable(game, player, out string info))
                {
                    //卡牌不可用，停止拖拽并提示
                    showTip(info);
                    resetUse(true, true);
                }
                else
                {
                    //手牌在战场上大小和随从牌一致
                    item.Card.rectTransform.localScale = Vector3.one * .4f / rectTransform.localScale.y;
                    if (item.Card.card.define is ServantCardDefine)
                    {
                        SelfFieldList.defaultItem.rectTransform.SetAsFirstSibling();
                        ServantPlaceHolder.rectTransform.sizeDelta = SelfFieldList.defaultItem.rectTransform.sizeDelta;
                        SelfFieldList.addChild(ServantPlaceHolder.rectTransform);
                        ServantPlaceHolder.display();
                        var servants = SelfFieldList.getItems();
                        int index = 0;
                        if (servants.Length > 0)
                        {
                            //需要选择空位，计算空位
                            for (int i = 0; i < servants.Length; i++)
                            {
                                if (servants[i].rectTransform.position.x < pointer.position.x)
                                    index = i + 1;
                            }
                            ServantPlaceHolder.rectTransform.SetSiblingIndex(index + 1);
                        }
                    }
                }
            }
        }
        public void onReleaseHand(HandListItem item, PointerEventData pointer)
        {
            if (item.GetComponentInParent<HandList>() != SelfHandList)//你不能用别人的卡
                return;
            if (!canControl)//不是你的回合，不生效
            {
                resetUse(true, true);
                return;
            }
            usingHand = item;
            if (SelfHandList.rectTransform.rect.Contains(SelfHandList.rectTransform.InverseTransformPoint(pointer.position)))
            {
                //如果松开，取消使用
                resetUse(true, true);
            }
            else
            {
                if (!item.Card.card.isUsable(game, player, out string info))
                {
                    //卡牌不可用
                    showTip(info);
                    resetUse(true, true);
                }
                else if (item.Card.card.define is ServantCardDefine)
                {
                    //松开鼠标，确认使用随从牌
                    var servants = SelfFieldList.getItems();
                    int index = 0;
                    if (servants.Length > 0)
                    {
                        //需要选择空位，计算空位
                        for (int i = 0; i < servants.Length; i++)
                        {
                            if (servants[i].rectTransform.position.x < pointer.position.x)
                                index = i + 1;
                        }
                        ServantPlaceHolder.rectTransform.SetSiblingIndex(index + 1);
                    }
                    if (item.Card.card.getAvaliableTargets(game) is TouhouCardEngine.Card[] targets && targets.Length > 0)
                    {
                        _usingCard = item.Card.card;
                        _usingPosition = index;
                        //进入选择目标状态，固定手牌到占位上，高亮可以选择的目标
                        item.Card.hide();
                        //显示占位随从
                        ServantPlaceHolder.Servant.display();
                        ServantPlaceHolder.Servant.update(item.Card.card.define, getSkin(item.Card.card));
                        isSelectingTarget = true;
                        selectableTargets = targets.Select(target =>
                        {
                            if (getMaster(target) is Master master)
                                return master as UIObject;
                            else if (getServant(target) is Servant servant)
                                return servant as UIObject;
                            throw new ActorNotFoundException(target);
                        }).ToArray();
                        SelfHandList.shrink();
                    }
                    else
                    {
                        //使用无目标随从牌
                        player.cmdUse(game, item.Card.card, index);
                        resetUse(false, false);
                    }
                }
                else if (item.Card.card.define is SpellCardDefine)
                {
                    if (item.Card.card.getAvaliableTargets(game) is TouhouCardEngine.Card[] targets && targets.Length > 0)
                    {
                        //进入选择目标状态，高亮可以选择的目标
                        item.Card.hide();
                        isSelectingTarget = true;
                        selectableTargets = targets.Select(target =>
                        {
                            if (getMaster(target) is Master master)
                                return master as UIObject;
                            else if (getServant(target) is Servant servant)
                                return servant as UIObject;
                            throw new ActorNotFoundException(target);
                        }).ToArray();
                        SelfHandList.shrink();
                    }
                    else
                    {
                        //使用无目标随从牌
                        player.cmdUse(game, item.Card.card, 0);
                        resetUse(false, false);
                    }
                }
            }
        }
        private void resetUse(bool resetItem, bool resetPlaceHolder)
        {
            if (usingHand != null && resetItem)
            {
                usingHand.Card.display();
                usingHand.Card.rectTransform.localScale = Vector3.one;
                usingHand.Card.rectTransform.localPosition = Vector2.zero;
            }
            if (resetPlaceHolder)
                hideServantPlaceHolder();
            isSelectingTarget = false;
            selectableTargets = null;
        }
        private void hideServantPlaceHolder()
        {
            addChild(ServantPlaceHolder.rectTransform);
            ServantPlaceHolder.Servant.hide();
            ServantPlaceHolder.hide();
        }
        #endregion
        [SerializeField]
        BJSYGameCore.Timer _tipTimer = new BJSYGameCore.Timer();
        public void showTip(string tip)
        {
            TipText.gameObject.SetActive(true);
            TipText.text = tip;
            _tipTimer.start();
        }
        public Servant getServant(TouhouCardEngine.Card card)
        {
            foreach (var item in SelfFieldList)
            {
                if (item.card == card)
                    return item;
            }
            foreach (var item in EnemyFieldList)
            {
                if (item.card == card)
                    return item;
            }
            return null;
        }
        public Master getMaster(TouhouCardEngine.Card card)
        {
            if (SelfMaster.card == card)
                return SelfMaster;
            else if (EnemyMaster.card == card)
                return EnemyMaster;
            else
                return null;
        }
        public UIObject getCharacter(TouhouCardEngine.Card card)
        {
            Master master = getMaster(card);
            if (master == null)
                return getServant(card);
            return master;
        }
        public UIObject[] getCharacters(TouhouCardEngine.Card[] targets)
        {
            return targets.Select(target =>
            {
                if (getMaster(target) is Master master)
                    return master as UIObject;
                else if (getServant(target) is Servant servant)
                    return servant as UIObject;
                throw new ActorNotFoundException(target);
            }).ToArray();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log("点击桌面ui:" + eventData.pointerCurrentRaycast.gameObject);
        }

        [SerializeField]
        UIObject[] _selectableTargets = null;
        public UIObject[] selectableTargets
        {
            get { return _selectableTargets; }
            set { _selectableTargets = value; }
        }
    }
}
