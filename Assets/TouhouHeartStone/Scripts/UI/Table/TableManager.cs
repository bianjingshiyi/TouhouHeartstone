using UnityEngine;
using BJSYGameCore;
using UI;
using TouhouHeartstone;
using System;
using System.Collections.Generic;
using TouhouCardEngine.Interfaces;
using System.Reflection;
using TouhouCardEngine;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Card = TouhouCardEngine.Card;
using UCard = UI.Card;
using BJSYGameCore.UI;
using TouhouHeartstone.Builtin;
using MukioI18n;

namespace Game
{
    public partial class TableManager : Manager
    {
        [Header("Config")]
        [SerializeField]
        float _skillDragThreshold = 75;
        public float skillDragThreshold => _skillDragThreshold;
        [SerializeField]
        AnimationCurve _moveToFieldCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        public AnimationCurve handToFieldCurve => _moveToFieldCurve;
        [SerializeField]
        AnimationCurve _fieldMoveCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        public AnimationCurve fieldMoveCurve => _fieldMoveCurve;
        [SerializeField]
        AnimationCurve _cardShowCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.2f, 1));
        public AnimationCurve cardShowCurve => _cardShowCurve;
        [SerializeField]
        AnimationCurve _fieldToDeckCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.6f, 1));
        public AnimationCurve fieldToDeckCurve => _fieldToDeckCurve;
        [SerializeField]
        Gradient timeoutGradient = new Gradient();
        [SerializeField]
        UI.Card _cardPrefab;
        public UI.Card cardPrefab => _cardPrefab;
        [SerializeField]
        Vector2 _tipBoxOffset = new Vector2(20, 20);
        [Header("State")]
        [SerializeField]
        Table _table;
        public Table ui
        {
            get { return _table; }
            set { _table = value; }
        }
        public CardEngine game { get; private set; } = null;
        public Player player { get; private set; } = null;
        /// <summary>
        /// 当前回合的玩家（仅限动画显示）。
        /// </summary>
        public Player currentAnimPlayer { get; set; }
        ActionEventCache eventCache { get; } = new ActionEventCache();

        LangManager lang => getManager<LangManager>();

        [SerializeField]
        bool _canControl = true;
        /// <summary>
        /// 当前是否可以控制（是否是自己的回合）
        /// </summary>
        public bool canControl
        {
            get { return _canControl; }
            set { _canControl = value; }
        }
        /// <summary>
        /// 当前正在使用的卡片的引用
        /// </summary>
        public Card usingCard { get; set; } = null;
        [SerializeField]
        int _usingPosition = -1;
        public int usingPosition
        {
            get { return _usingPosition; }
            set { _usingPosition = value; }
        }
        [SerializeField]
        bool _isSelectingTarget = false;
        /// <summary>
        /// 是否进入目标选择模式
        /// </summary>
        public bool isSelectingTarget
        {
            get { return _isSelectingTarget; }
            set { _isSelectingTarget = value; }
        }
        HandToFieldAnim _handToFieldAnim = null;
        public Card attackingCard { get; set; } = null;
        public SimpleAnim simpleAnim = null;



        protected override void onAwake()
        {
            base.onAwake();
            if (ui == null)
                ui = this.findInstance<Table>();
            loadAnim(GetType().Assembly);
        }
        protected void Update()
        {
            if (_showCardTimer.isExpired())
            {
                // 显示卡面
                if (_servant != null)
                    displayLargeCard(_pointer.position.x < Screen.width / 2, getCard(_servant));
                _showCardTimer.reset();
            }

            if (_tipTimer.isExpired())
            {
                _tipTimer.reset();
                ui.TipText.hide();
            }
            else if (_tipTimer.isStarted)
            {
                ui.TipText.setAlpha(1/*_tipTimer.getRemainedTime() / _tipTimer.duration*/);
            }



            if (game == null)
                return;

            // patch: 我不知道怎么改AutoCompo所以先Patch一个
            var clock = ui.TurnEndButton.GetComponent<TurnEndClock>();

            if (game.getTurnTimer() != null)
            {
                clock?.SetCountdown(game.getTurnTimer().remainedTime);

                if (game.getTurnTimer().remainedTime <= 15)
                {
                    ui.TimeoutSlider.display();
                    ui.TimeoutSlider.value = game.getTurnTimer().remainedTime / 15;
                    ui.TimeoutSlider.fillRect.GetComponent<Image>().color = timeoutGradient.Evaluate(1 - ui.TimeoutSlider.value);
                }
            }
            else
            {
                clock?.SetCountdown(0);
                ui.TimeoutSlider.hide();
            }

            clock?.SetTurnState(canControl);

            //随从牌使用预览
            if (_handToFieldAnim != null)
            {
                if (_handToFieldAnim.update(this, Time.time))
                {
                    _handToFieldAnim = null;
                    if (tryGetHand(usingCard, out var hand))
                        hand.Card.hide();
                    else
                        throw new ActorNotFoundException(usingCard);
                    //显示占位随从
                    ui.ServantPlaceHolder.Servant.display();
                    try
                    {
                        setServant(ui.ServantPlaceHolder.Servant, usingCard.define);
                    }
                    catch (NullReferenceException e)
                    {
                        Debug.LogError(e);
                    }
                }
            }
            //游戏事件处理
            eventCache.poll();
            //动画
            updateAnim();
        }
        /// <summary>
        /// 设置游戏对象和玩家并初始化UI
        /// </summary>
        /// <param name="game"></param>
        /// <param name="player"></param>
        public void setGame(CardEngine game, Player player)
        {
            ui.onClickNoWhere.set(onClickNoWhere);
            ui.InitReplaceDialog.hide();
            ui.TurnTipImage.hide();
            initMaster(ui.SelfMaster);
            ui.SelfGem.Text.text = "0";
            ui.SelfSkill.asButton.onClick.set(onClickSkill);
            ui.SelfSkill.onDrag.set(onDragSkill);
            ui.SelfSkill.onDragEnd.set(onDragSkillEnd);
            ui.SelfSkill.onPointerEnter.set(onSelfSkillPointerEnter);
            ui.SelfSkill.onPointerExit.set(onSkillPointerExit);
            ui.SelfSkill.onPointerMove.set(onSkillPointerMove);
            ui.SelfItem.hide();
            ui.SelfItem.onPointerEnter.set(onItemPointerEnter);
            ui.SelfItem.onPointerExit.set(onItemPointerExit);
            ui.SelfHandList.clearItems();
            ui.SelfHandList.asButton.onClick.set(() =>
            {
                if (ui.SelfHandList.isExpanded)
                    ui.SelfHandList.shrink();
                else
                    ui.SelfHandList.expand();
            });
            ui.SelfFieldList.clearItems();
            ui.SelfDeck.asButton.onClick.set(onDeckClick);
            ui.SelfDeck.onPointerEnter.set(onDeckPointerEnter);
            ui.SelfDeck.onPointerExit.set(onDeckPointerExit);
            ui.SelfDeck.onPointerMove.set(onDeckPointerMove);
            ui.SelfGraveDeck.asButton.onClick.set(onGraveClick);
            ui.SelfGraveDeck.onPointerEnter.set(onGravePointerEnter);
            ui.SelfGraveDeck.onPointerExit.set(onGravePointerExit);
            ui.SelfGraveDeck.onPointerMove.set(onDeckPointerMove);
            ui.TurnEndButton.onClick.set(onTurnEndButtonClick);
            ui.SelfMaster.ArmorPropNumber.text.text = "0";
            ui.LargeCard.hide();
            ui.TipText.hide();
            initMaster(ui.EnemyMaster);
            ui.EnemyGem.Text.text = "0";
            ui.EnemyMaster.ArmorPropNumber.text.text = "0";
            ui.EnemyItem.hide();
            ui.EnemyItem.onPointerEnter.set(onItemPointerEnter);
            ui.EnemyItem.onPointerExit.set(onItemPointerExit);
            ui.EnemyFieldList.clearItems();
            ui.EnemyHandList.clearItems();
            ui.EnemySkill.onPointerEnter.set(onEnemySkillPointerEnter);
            ui.EnemySkill.onPointerExit.set(onSkillPointerExit);
            ui.EnemySkill.onPointerMove.set(onSkillPointerMove);
            ui.AttackArrowImage.hide();
            ui.Fatigue.hide();
            ui.Discover.ButtonButtonBlack.asButton.onClick.set(onDiscoverHideButtonClick);
            ui.Discover.hide();
            ui.PilePanel.hide();
            ui.TipBox.hide();
            eventCache.clear();
            _animationQueue.Clear();
            DestroyInvokeCard();
            if (game != null)
            {
                game.triggers.onEventBefore -= onEventBeforeHandler;
                game.triggers.onEventAfter -= onEventAfterHandler;
                game.answers.onRequest -= onRequestHandler;
                game.answers.onResponse -= onResponseHandler;
            }
            this.game = game;
            if (game != null)
            {
                game.triggers.onEventBefore += onEventBeforeHandler;
                game.triggers.onEventAfter += onEventAfterHandler;
                game.answers.onRequest += onRequestHandler;
                game.answers.onResponse += onResponseHandler;
            }
            this.player = player;
        }

        void onRequestHandler(IRequest request)
        {
            eventCache.cache(onRequest, request);
        }
        /// <summary>
        /// 游戏核心Request对应处理
        /// </summary>
        /// <param name="obj"></param>
        private void onRequest(IRequest obj)
        {
            if (!obj.playersId.Contains(player.id))
                return;
            UIAnimation anim;
            switch (obj)
            {
                default:
                    anim = getRequestAnim(obj);
                    break;
            }
            if (anim != null)
                addAnim(anim);
            else if (!(obj is FreeActRequest))
                UberDebug.LogWarningChannel("UI", "没有与" + obj + "相应的动画");
        }
        void onResponseHandler(IResponse response)
        {
            eventCache.cache(onResponse, response);
        }
        void onResponse(IResponse response)
        {
            switch (response)
            {
                case DiscoverResponse _:
                    closeDiscoverDialog();
                    break;
                default:
                    break;
            }
        }
        void onEventBeforeHandler(IEventArg arg)
        {
            eventCache.cache(onEventBefore, arg);
        }
        /// <summary>
        /// 事件前处理
        /// </summary>
        /// <param name="arg"></param>
        private void onEventBefore(IEventArg arg)
        {
            UIAnimation anim;
            switch (arg)
            {
                case Card.AddModiEventArg addMod:
                    anim = new AddModiAnim() { eventArg = addMod };
                    break;
                default:
                    anim = getEventAnim(arg);
                    break;
            }
            if (anim != null)
                addAnim(anim);
        }
        private void onEventAfterHandler(IEventArg arg)
        {
            eventCache.cache(onEventAfter, arg);
        }
        /// <summary>
        /// 事件后处理
        /// </summary>
        /// <param name="arg"></param>
        private void onEventAfter(IEventArg arg)
        {

        }
        #region UI
        void onClickNoWhere(Table table, PointerEventData pointer)
        {
            tryCancelUse();
        }

        private bool tryCancelUse()
        {
            if (usingCard != null && usingCard.define.isServant() && isSelectingTarget)
            {
                resetUse(true, true);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 尝试获取一张卡对应的Master
        /// </summary>
        /// <param name="card"></param>
        /// <param name="master"></param>
        /// <returns></returns>
        public bool tryGetMaster(Card card, out Master master)
        {
            if (player.getMaster() == card)
            {
                master = ui.SelfMaster;
                return true;
            }
            else if (game.getOpponent(player).getMaster() == card)
            {
                master = ui.EnemyMaster;
                return true;
            }
            else
            {
                master = null;
                return false;
            }
        }

        /// <summary>
        /// 删除强制退出过程之中产生的揭示动画
        /// </summary>
        /// <param name="master"></param>
        public void DestroyInvokeCard()
        {
            Transform[] Cards = this.ui.InvokeDisPlayRectTransform.GetComponentsInChildren<Transform>();
            for (int i = 1; i < this.ui.InvokeDisPlayRectTransform.GetComponentsInChildren<Transform>().Count(); i++)
            {
                Destroy(Cards[i].gameObject);
            }
        }

        /// <summary>
        /// 初始化Master的事件
        /// </summary>
        /// <param name="master"></param>

        public void initMaster(Master master)
        {
            master.onClick.add(onClickMaster);
            master.Head.clearSecret();
            master.LifePropNumber.WhiteText.invokeBeforeAnim();
            master.asAnimator.SetBool("isShackle", false);
            master.onCanAttackFalse.invokeBeforeAnim();
            master.HighlightImage.hide();
            master.AttackPropNumber.hide();
            master.asAnimator.SetBool("hasArmor", false);
            master.HealImage.hide();
            master.Image.color = new Color(1, 1, 1, 1);
            ResetPos(master);
            master.rectTransform.localScale = new Vector3(1, 1, 1);
        }
        /// <summary>
        /// 用于重置非正常退出后的英雄位置
        /// </summary>
        /// <param name="ReMaster"></param>
        public void ResetPos(Master ReMaster)
        {
            ReMaster.GetComponentInChildren<PositionLerp>()._time = 0.0f;
            ReMaster.GetComponentInChildren<PositionLerp>().enabled = false;
            ReMaster.rootRectTransform.anchoredPosition = new Vector3(0, 0, 0);
        }
        /// <summary>
        /// 设置Master的UI
        /// </summary>
        /// <param name="master"></param>
        /// <param name="card"></param>
        /// <param name="isSelectable"></param>
        public async void setMaster(Master master, Card card, bool isSelectable = false)
        {
            CardSkinData skin = getSkin(card);
            master.LifePropNumber.text.text = card.getCurrentLife(game).ToString();
            if (card.getAttack(game) > 0)
            {
                master.AttackPropNumber.text.text = card.getAttack(game).ToString();
                master.asAnimator.SetBool("ShowAttack", true);
                master.AttackPropNumber.text.display();

            }
            else
            {
                master.AttackPropNumber.text.hide();
                master.asAnimator.SetBool("ShowAttack", false);
            }
            if (card.getArmor(game) > 0)
            {
                master.ArmorPropNumber.text.text = card.getArmor(game).ToString();
                master.ArmorPropNumber.display();
            }
            else
            {
                master.ArmorPropNumber.hide();
            }

            if (isSelectable)
            {
                // master.HighlightController = Master.Highlight.Yellow;
                master.onSelectableTrue.invokeBeforeAnim();
                master.onSelectableTrue.invokeAfterAnim();
            }
            else
            {
                master.onSelectableFalse.invokeBeforeAnim();
                master.onSelectableFalse.invokeAfterAnim();
            }
            if (card.getOwner() == player && currentAnimPlayer == player && card.canAttack(game))
            {
                // master.HighlightController = Master.Highlight.Green;
                master.onCanAttackTrue.invokeBeforeAnim();
                master.onCanAttackTrue.invokeAfterAnim();
                master.onClick.add(onClickMaster);
                master.onDrag.add(onDragMaster);
                master.onDragEnd.add(onDragEndMaster);
            }
            else if (currentAnimPlayer == player)
            {
                // master.HighlightController = Master.Highlight.None;
                master.onCanAttackFalse.invokeBeforeAnim();
                master.onCanAttackFalse.invokeAfterAnim();
            }

            if (card.isShield(game))
                master.ShieldImage.display();
            else
                master.ShieldImage.hide();
            if (card.isFreeze(game))
            {
                master.onShackleTrue.invokeBeforeAnim();
                master.onShackleTrue.invokeAfterAnim();
            }
            else
            {
                master.onShackleFalse.invokeBeforeAnim();
                master.onShackleFalse.invokeAfterAnim();
            }
            if (skin.image != null)
            {
                master.Image.sprite = skin.image;
            }
            else
            {
                var cards = getManager<CardManager>();
                var poolRef = cards.getCardPoolReferenceOfCard(card.define);
                master.Image.sprite = await cards.loadCardImage(poolRef, card.define.id);
            }
        }

        public Item GetRelatedItem(Card card) => card.owner == currentAnimPlayer ? ui.SelfItem : ui.EnemyItem;


        public bool tryGetItem(Card card, out Item item)
        {
            if (card == null)
            {
                item = null;
                return false;
            }
            if (_selfItemCard == card)
            {
                item = ui.SelfItem;
                return true;
            }
            else if (_enemyItemCard == card)
            {
                item = ui.EnemyItem;
                return true;
            }
            else
            {
                item = null;
                return false;
            }
        }
        public async void setItem(Item item, Card card)
        {
            if (card == null)
            {
                item.hide();
                if (item == ui.SelfItem)
                    _selfItemCard = null;
                else if (item == ui.EnemyItem)
                    _enemyItemCard = null;
                return;
            }
            item.display();
            if (item == ui.SelfItem)
                _selfItemCard = card;
            else if (item == ui.EnemyItem)
                _enemyItemCard = card;

            if (card.getOwner() == currentAnimPlayer)
            {
                if (item.animator.gameObject.activeSelf)
                {
                    item.onItemON.invokeBeforeAnim();
                    item.onItemON.invokeAfterAnim();
                }
            }
            else
            {
                if (item.animator.gameObject.activeSelf)
                {
                    item.onItemOff.invokeBeforeAnim();
                    item.onItemOff.invokeAfterAnim();
                }
            }

            CardSkinData skin = getSkin(card);
            if (skin.image != null)
            {
                item.Image.sprite = skin.image;
            }
            else
            {
                var cards = getManager<CardManager>();
                var poolRef = cards.getCardPoolReferenceOfCard(card.define);
                item.Image.sprite = await cards.loadCardImage(poolRef, card.define.id);
            }
        }
        private void onItemPointerExit(Item item, PointerEventData pointer)
        {
            ui.TipBox.hide();
        }

        private void onItemPointerEnter(Item item, PointerEventData pointer)
        {
            if (item.card != null)
                ui.TipBox.display(getSkin(item.card).desc, pointer.position + _tipBoxOffset);
        }

        void displayArrow(RectTransform source, Vector2 from, Vector2 to)
        {
            ui.AttackArrowImage.display();
            ui.AttackArrowImage.rectTransform.SetSiblingIndex(source.GetSiblingIndex() - 1);
            ui.AttackArrowImage.rectTransform.position = from;
            ui.AttackArrowImage.rectTransform.up = (to - from).normalized;
            ui.AttackArrowImage.rectTransform.setHeight(Vector3.Distance(from, to) / ui.getCanvas().transform.localScale.y);
        }
        /// <summary>
        /// 隐藏箭头
        /// </summary>
        void hideArrow()
        {
            ui.AttackArrowImage.hide();
        }
        /// <summary>
        /// 当前手牌列表
        /// </summary>
        Dictionary<Card, HandListItem> cardHandDic { get; } = new Dictionary<Card, HandListItem>();
        /// <summary>
        /// 创建一张手牌
        /// </summary>
        /// <param name="card"></param>
        /// <param name="position">默认为-1也就是最右手</param>
        /// <returns></returns>
        public HandListItem createHand(Card card, int position = -1)
        {
            if (cardHandDic.ContainsKey(card))
            {
                if (cardHandDic[card] != null)
                {
                    UberDebug.LogErrorChannel("UI", "手牌中已经存在" + card + "对应UI" + cardHandDic[card]);
                    return cardHandDic[card];
                }
                else
                    cardHandDic[card] = null;
            }
            UberDebug.LogTraceChannel("UI", "创建手牌UI：" + card);
            HandListItem item;
            if (card.getOwner() == player)
            {
                item = ui.SelfHandList.addItem();
                if (position >= 0)
                {
                    ui.SelfHandList.defaultItem.rectTransform.SetAsFirstSibling();
                    item.rectTransform.SetSiblingIndex(position + 1);
                }
                item.isDragable = true;
                setCard(item.Card, card, true);
            }
            else
            {
                item = ui.EnemyHandList.addItem();
                if (position >= 0)
                {
                    ui.EnemyHandList.defaultItem.rectTransform.SetAsFirstSibling();
                    item.rectTransform.SetSiblingIndex(position + 1);
                }
                item.isDragable = false;
                item.Card.setBoolFalse("isFaceUp");
                setCard(item.Card, card, false);
            }
            item.gameObject.name = card.ToString();
            item.onDrag.set(onDragHand);
            item.onEndDrag.set(onDragHandEnd);
            item.onPointerEnter.set(onHandPointerEnter);
            item.onPointerExit.set(onHandPointerExit);
            item.onPointerMove.set(onHandPointerMove);
            cardHandDic.Add(card, item);
            return item;
        }

        /// <summary>
        /// 手牌拖拽事件
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="pointer"></param>
        void onDragHand(HandListItem hand, PointerEventData pointer)
        {
            if (!hand.isDragable)
                return;
            if (!canControl)//无法进行控制
            {
                resetUse(true, true);
                return;
            }
            Card card = getCard(hand);
            if (usingCard != null && usingCard != card)
            {
                resetUse(true, true);
                return;
            }
            usingCard = card;
            //拖拽卡片
            hand.Card.rectTransform.position = pointer.position;
            if (ui.SelfHandList.rectTransform.rect.Contains(ui.SelfHandList.rectTransform.InverseTransformPoint(pointer.position)))
            {
                //如果移动回手牌区域，恢复正常大小
                hand.Card.rectTransform.localScale = Vector3.one;
                //移除随从占位
                if (usingCard.isServant())
                    hideServantPlaceHolder();
            }
            else
            {
                //移动到手牌区以外的地方视作打算使用
                if (!usingCard.isUsable(game, player, out string info))
                {
                    //卡牌不可用，停止拖拽并提示
                    showTip(info);
                    resetUse(true, true);
                }
                else
                {
                    //手牌在战场上大小和随从牌一致
                    hand.Card.rectTransform.localScale = Vector3.one * .4f / ui.SelfHandList.rectTransform.localScale.y;
                    if (usingCard.define.isServant())
                    {
                        //如果手牌是随从，那么在场上的时候会有一个占位符，预览这个随从放下去的位置。
                        ui.SelfFieldList.defaultItem.rectTransform.SetAsFirstSibling();
                        ui.ServantPlaceHolder.rectTransform.sizeDelta = ui.SelfFieldList.defaultItem.rectTransform.sizeDelta;
                        ui.SelfFieldList.addChild(ui.ServantPlaceHolder.rectTransform);
                        ui.ServantPlaceHolder.display();
                        ui.ServantPlaceHolder.rectTransform.SetSiblingIndex(calcIndexInField(pointer.position) + 1);
                    }
                    else if (usingCard.define.isSpell())
                    {
                        if (usingCard.isNeedTarget(game))
                        {
                            if (!isSelectingTarget)
                            {
                                isSelectingTarget = true;//TODO:这个地方一定要重写，怎么能这样的？isSelectingTarget就应该被usingCard取代
                                highlightTargets(usingCard.getAvaliableTargets(game));//高亮所有可选目标
                                ui.SelfHandList.shrink();//手牌缩小
                            }
                            hand.Card.hide();//将那张牌暂时隐藏
                            if (tryGetMaster(usingCard.getOwner().getMaster(), out var castMaster))
                                //拖出箭头
                                displayArrow(castMaster.rectTransform, castMaster.rectTransform.position, pointer.position);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 计算随从空位位置
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private int calcIndexInField(Vector2 position)
        {
            int index = 0;
            var servants = ui.SelfFieldList.getItems();
            if (servants.Length > 0)
            {
                //需要选择空位，计算空位
                for (int i = 0; i < servants.Length; i++)
                {
                    if (servants[i].rectTransform.position.x < position.x)
                        index = i + 1;
                }
            }
            return index;
        }
        /// <summary>
        /// 手牌拖拽结束
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="pointer"></param>
        void onDragHandEnd(HandListItem hand, PointerEventData pointer)
        {
            if (hand.GetComponentInParent<HandList>() != ui.SelfHandList)//你不能用别人的卡
                return;
            if (!canControl)//不是你的回合，不生效
            {
                resetUse(true, true);
                return;
            }
            usingCard = getCard(hand);
            if (ui.SelfHandList.rectTransform.rect.Contains(ui.SelfHandList.rectTransform.InverseTransformPoint(pointer.position)))
            {
                //如果松开，取消使用
                resetUse(true, true);
            }
            else
            {
                if (!usingCard.isUsable(game, player, out string info))
                {
                    //卡牌不可用
                    showTip(info);
                    resetUse(true, true);
                }
                else if (usingCard.isServant())
                {
                    //松开鼠标，确认使用随从牌
                    int index = calcIndexInField(pointer.position);
                    ui.ServantPlaceHolder.rectTransform.SetSiblingIndex(index + 1);
                    if (usingCard.getAvaliableTargets(game) is Card[] targets && targets.Length > 0)
                    {
                        usingPosition = index;
                        //进入选择目标状态，固定手牌到占位上，高亮可以选择的目标
                        _handToFieldAnim = new HandToFieldAnim(this, hand, ui.SelfFieldList, index);
                        isSelectingTarget = true;
                        highlightTargets(targets);
                        ui.SelfSkill.isUsable = false;
                        ui.SelfHandList.shrink();
                    }
                    else
                    {
                        //使用无目标随从牌
                        player.cmdUse(game, usingCard, index);
                        resetUse(false, false);
                    }
                }
                else if (usingCard.isSpell())
                {
                    if (usingCard.isNeedTarget(game))
                    {
                        //获取鼠标停留的那张卡，就是目标
                        if (isOnTarget(pointer, out var target))
                        {
                            if (usingCard.isValidTarget(game, target))
                            {
                                //合法目标，生效
                                player.cmdUse(game, usingCard, 0, target);
                                resetUse(true, false);
                            }
                            else
                            {
                                //非法目标，跳出非法信息
                                showTip(info);
                                resetUse(true, true);
                                hand.Card.display();
                            }
                        }
                        else
                        {
                            cancelAttack();
                            resetUse(true, true);
                            hand.Card.display();
                        }

                    }
                    else
                    {
                        //使用无目标随从牌
                        player.cmdUse(game, usingCard, 0);
                        resetUse(false, false);
                    }
                }
                else if (usingCard.isItem())
                {
                    if (usingCard.isNeedTarget(game))
                    {
                        usingPosition = 0;
                        //进入选择目标状态，固定手牌到占位上，高亮可以选择的目标
                        isSelectingTarget = true;
                        highlightTargets(usingCard.getAvaliableTargets(game));
                        ui.SelfSkill.isUsable = false;
                        ui.SelfHandList.shrink();
                    }
                    else
                    {
                        //使用无目标随从牌
                        player.cmdUse(game, usingCard, 0);
                        resetUse(false, false);
                    }
                }
            }
            //removeHighlights();
        }
        /// <summary>
        /// 点击Master卡处理
        /// </summary>
        /// <param name="master"></param>
        /// <param name="pointer"></param>
        void onClickMaster(Master master, PointerEventData pointer)
        {
            // （目标选择模式）
            if (isSelectingTarget)
            {
                if (usingCard.isValidTarget(game, getCard(master)))
                {
                    player.cmdUse(game, usingCard, usingPosition, getCard(master));
                    resetUse(false, false);
                }
                else
                {
                    showTip(TAB_THIS_INVALIDATION);
                    resetUse(true, true);
                }
            }
        }
        [TranslateMsg]
        private const string TAB_THIS_INVALIDATION = "这不是一个有效的目标！";

        /// <summary>
        /// 点击随从卡
        /// </summary>
        /// <param name="servant"></param>
        /// <param name="pointer"></param>
        void onClickServant(ServantItem servant, PointerEventData pointer)
        {
            // （目标选择模式）
            if (isSelectingTarget)
            {
                if (usingCard.isValidTarget(game, getCard(servant)))
                {
                    player.cmdUse(game, usingCard, usingPosition, getCard(servant));
                    resetUse(false, false);
                }
                else
                {
                    showTip(TAB_THIS_INVALIDATION);
                    resetUse(true, true);
                }
            }
        }
        /// <summary>
        /// 重置使用卡牌状态
        /// </summary>
        /// <param name="resetItem">是否重置手牌</param>
        /// <param name="resetPlaceHolder">是否重置随从占位</param>
        private void resetUse(bool resetItem, bool resetPlaceHolder)
        {
            if (usingCard != null && resetItem)
            {
                HandListItem usingHand = getHand(usingCard);
                usingHand.Card.display();
                setCard(usingHand.Card, usingCard, true);
                usingHand.Card.rectTransform.localScale = Vector3.one;
                usingHand.Card.rectTransform.localPosition = Vector2.zero;
            }
            _handToFieldAnim = null;
            usingCard = null;
            if (resetPlaceHolder)
                hideServantPlaceHolder();
            hideArrow();
            isSelectingTarget = false;
            removeHighlights();
        }
        /// <summary>
        /// 获取UI手牌引用
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public HandListItem getHand(Card card)
        {
            if (cardHandDic.ContainsKey(card))
                return cardHandDic[card];
            throw new ActorNotFoundException(card);
        }
        /// <summary>
        /// 尝试获取UI手牌引用
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public bool tryGetHand(Card card, out HandListItem hand)
        {
            if (cardHandDic.ContainsKey(card))
            {
                if (cardHandDic[card] != null)
                {
                    hand = cardHandDic[card];
                    return true;
                }
                else
                    cardHandDic.Remove(card);
            }
            hand = null;
            return false;
        }

        /// <summary>
        /// 移除一个UI手牌。
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public bool removeHand(Card card)
        {
            if (cardHandDic.ContainsKey(card))
            {
                var item = cardHandDic[card];
                cardHandDic.Remove(card);

                if (card.getOwner() == player)
                {
                    ui.SelfHandList.removeItem(item);
                }
                else
                {
                    ui.EnemyHandList.removeItem(item);
                }
            }
            return false;
        }
        /// <summary>
        /// 手牌悬停事件
        /// </summary>
        /// <param name="handList"></param>
        /// <param name="pointer"></param>
        [TranslateMsg]
        private const String TAB_CARD_NUMBER = "{0}张手牌";
        void onHandPointerEnter(HandListItem handList, PointerEventData pointer)
        {
            handList.onHoverCard.invokeBeforeAnim();
            handList.onHoverCard.invokeAfterAnim();
            if (!ui.SelfHandList.isExpanded &&
                player.getHand().count > 0)
            {
                ui.TipBox.display(lang._(TAB_CARD_NUMBER, getCard(handList).getOwner().getHand().count), pointer.position + _tipBoxOffset);
            }
        }
        void onHandPointerMove(HandListItem handList, Vector2 pointer)
        {
            ui.TipBox.updatePos(pointer);
        }
        void onHandPointerExit(HandListItem handList, PointerEventData pointer)
        {
            handList.onexitHoverCard.invokeBeforeAnim();
            handList.onexitHoverCard.invokeAfterAnim();
            ui.TipBox.hide();
        }
        /// <summary>
        /// 从UI手牌引用获取对应的牌
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Card getCard(HandListItem item)
        {
            foreach (var pair in cardHandDic)
            {
                if (pair.Value == item)
                    return pair.Key;
            }
            return null;
        }
        /// <summary>
        /// 从Master获取对应的牌
        /// </summary>
        /// <param name="master"></param>
        /// <returns></returns>
        public Card getCard(Master master)
        {
            if (master == ui.SelfMaster)
                return player.getMaster();
            else
                return game.getOpponent(player).getMaster();
        }
        public UI.Card createCard(Card card, Vector3 position, float height = -1, bool isFaceUp = true)
        {
            var ui = Instantiate(cardPrefab, position, Quaternion.identity, this.ui.FieldsImage.rectTransform);
            setCard(ui, card, isFaceUp);
            if (height < 0)
                return ui;
            ui.transform.localScale = Vector3.one * height / ui.rectTransform.rect.height;
            return ui;
        }

        public UI.Card createInvokeCard(Card card, Vector3 position, float height = -1, bool isFaceUp = true)
        {
            var ui = Instantiate(cardPrefab, position, Quaternion.identity, this.ui.InvokeDisPlayRectTransform);
            setCard(ui, card, isFaceUp);
            if (height < 0)
                return ui;
            ui.transform.localScale = Vector3.one * height / ui.rectTransform.rect.height;
            return ui;
        }

        private Dictionary<String, String> _TranslateTagDic;
        public Dictionary<String, String> TranslateTagDic
        {
            get
            {
                if (_TranslateTagDic == null)
                {
                    _TranslateTagDic = new Dictionary<string, string>();
                    var T = typeof(TranslateTags);
                    FieldInfo[] fields = T.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance);
                    for (int i = 0; i < fields.Length; i++)
                    {
                        _TranslateTagDic.Add(fields[i].Name, fields[i].GetValue(null).ToString());
                        //Debug.Log("这里是字典结果" + fields[i].Name + fields[i].GetValue(null).ToString());
                    }
                }
                return _TranslateTagDic;
            }
            set { _TranslateTagDic = value; }
        }
        /// <summary>
        /// 设置卡片UI
        /// </summary>
        /// <param name="ui"></param>
        /// <param name="card"></param>
        /// <param name="isFaceup"></param>
        public async void setCard(UCard ui, Card card, bool isFaceup = true)
        {
            CardSkinData skin = getSkin(card);
            ui.setCard(game, card, skin, isFaceup);
            if (skin.image == null)
            {
                var cards = getManager<CardManager>();
                var poolRef = cards.getCardPoolReferenceOfCard(card.define);
                ui.setImage(await cards.loadCardImage(poolRef, card.define.id));
            }
            //if (card.define.type == DEFINE.SERVANT)
            //{
            //    ui.type = CATEGORY.SERVANT;
            //    //ui.servantCardFace.Charactertype = card.define.getCharacterID();
            //    ui.servantCardFace.image.sprite = skin.image;
            //    ui.servantCardFace.nameText.text = skin.name;
            //    if ((card.define as ServantCardDefine).tags.FirstOrDefault() != null)
            //        ui.servantCardFace.raceNameText.text = lang._(TranslateTagDic[(card.define as ServantCardDefine).tags.FirstOrDefault()]);
            //    else
            //        ui.servantCardFace.raceNameText.text = null;
            //    ui.servantCardFace.subNameText.text = skin.subname;
            //    ui.servantCardFace.descText.text = CardDescHelper.replace(skin.desc, game, player, card);
            //    ui.servantCardFace.costPropNumber.text.text = card.getCost(game).ToString();
            //    ui.servantCardFace.attackPropNumber.text.text = card.getAttack(game).ToString();
            //    ui.servantCardFace.lifePropNumber.text.text = card.getLife(game).ToString();
            //}
            //else if (card.define.type == DEFINE.SPELL)
            //{
            //    ui.type = CATEGORY.SPELL;
            //    ui.spellCardFace.image.sprite = skin.image;
            //    ui.spellCardFace.nameText.text = skin.name;
            //    ui.spellCardFace.descText.text = CardDescHelper.replace(skin.desc, game, player, card);
            //    ui.spellCardFace.costPropNumber.text.text = card.getCost(game).ToString();
            //}
            //else if (card.define.type == DEFINE.ITEM)
            //{
            //    ui.type = CATEGORY.ITEM;
            //    ui.itemCardFace.image.sprite = skin.image;
            //    ui.itemCardFace.nameText.text = skin.name;
            //    ui.itemCardFace.descText.text = CardDescHelper.replace(skin.desc, game, player, card);
            //    ui.itemCardFace.costPropNumber.text.text = card.getCost(game).ToString();
            //    ui.itemCardFace.attackPropNumber.text.text = card.getAttack(game).ToString();
            //    ui.itemCardFace.lifePropNumber.text.text = card.getLife(game).ToString();
            //}
            //else
            //    throw new NotImplementedException();
        }
        public async void setCard(UCard ui, CardDefine define, int cost, int attack, int life, bool isFaceUp = true)
        {
            CardSkinData skin = getSkin(define);
            ui.setCard(define, skin, isFaceUp);
            if (skin.image == null)
            {
                var cards = getManager<CardManager>();
                var poolRef = cards.getCardPoolReferenceOfCard(define);
                ui.setImage(await getManager<CardManager>().loadCardImage(poolRef, define.id));
            }
            //if (define.type == DEFINE.SERVANT)
            //{
            //    ui.type = CATEGORY.SERVANT;
            //    ui.servantCardFace.image.sprite = skin.image;
            //    ui.servantCardFace.nameText.text = skin.name;
            //    if ((define as ServantCardDefine).tags.FirstOrDefault() != null)
            //        ui.servantCardFace.raceNameText.text = lang._(TranslateTagDic[(define as ServantCardDefine).tags.FirstOrDefault()]);
            //    else
            //        ui.servantCardFace.raceNameText.text = null;
            //    ui.servantCardFace.subNameText.text = skin.subname;
            //    ui.servantCardFace.descText.text = skin.desc;
            //    ui.servantCardFace.costPropNumber.text.text = cost.ToString();
            //    ui.servantCardFace.attackPropNumber.text.text = attack.ToString();
            //    ui.servantCardFace.lifePropNumber.text.text = life.ToString();
            //}
            //else if (define.type == DEFINE.SPELL)
            //{
            //    ui.type = CATEGORY.SPELL;
            //    ui.spellCardFace.image.sprite = skin.image;
            //    ui.spellCardFace.nameText.text = skin.name;
            //    ui.spellCardFace.descText.text = skin.desc;
            //    ui.spellCardFace.costPropNumber.text.text = cost.ToString();
            //}
            //else if (define.type == DEFINE.ITEM)
            //{
            //    ui.type = CATEGORY.ITEM;
            //    ui.itemCardFace.image.sprite = skin.image;
            //    ui.itemCardFace.nameText.text = skin.name;
            //    ui.itemCardFace.descText.text = skin.desc;
            //    ui.itemCardFace.costPropNumber.text.text = cost.ToString();
            //    ui.itemCardFace.attackPropNumber.text.text = attack.ToString();
            //    ui.itemCardFace.lifePropNumber.text.text = life.ToString();
            //}
            //else
            //    throw new NotImplementedException();
        }
        public void setCard(UCard ui, CardSnapshot snapshot, bool isFaceup = true)
        {
            setCard(ui, snapshot.define, snapshot.cost, snapshot.attack, snapshot.life, isFaceup);
        }
        Dictionary<Card, ServantItem> cardServantDic { get; } = new Dictionary<Card, ServantItem>();
        /// <summary>
        /// 创建一个随从
        /// </summary>
        /// <param name="card"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public ServantItem createServant(Card card, int position, int attack = -1, int life = -1)
        {
            ServantItem servant;
            if (card.getOwner() == player)
            {
                servant = ui.SelfFieldList.addItem();
                ui.SelfFieldList.defaultItem.rectTransform.SetAsFirstSibling();
            }
            else
            {
                servant = ui.EnemyFieldList.addItem();
                ui.EnemyFieldList.defaultItem.rectTransform.SetAsFirstSibling();
            }
            UberDebug.LogChannel(servant, "UI", "为" + card + "创建" + servant);
            servant.gameObject.name = card.ToString();
            servant.rectTransform.SetSiblingIndex(position + 1);
            setServant(servant, card, false, attack, life);
            servant.onClick.add(onClickServant);
            servant.onDrag.add(onDragServant);
            servant.onDragEnd.add(onDragEndServant);
            servant.onPointerEnter.set(onEnterServant);
            servant.onPointerExit.set(onExitServant);
            cardServantDic.Add(card, servant);
            return servant;
        }
        /// <summary>
        /// 从卡获取对应的随从UI
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public ServantItem getServant(Card card)
        {
            if (cardServantDic.ContainsKey(card))
                return cardServantDic[card];
            throw new ActorNotFoundException(card);
        }
        /// <summary>
        /// 尝试从卡获取对应的随从UI
        /// </summary>
        /// <param name="card"></param>
        /// <param name="servant"></param>
        /// <returns></returns>
        public bool tryGetServant(Card card, out ServantItem servant)
        {
            if (cardServantDic.ContainsKey(card))
            {
                if (cardServantDic[card] != null)
                {
                    servant = cardServantDic[card];
                    return true;
                }
                cardServantDic.Remove(card);
                servant = null;
                return false;
            }
            else
            {
                servant = null;
                return false;
            }
        }



        /// <summary>
        /// 设置随从
        /// </summary>
        /// <param name="servant"></param>
        /// <param name="card"></param>
        /// <param name="isSelectable"></param>
        public async void setServant(ServantItem servant, Card card, bool isSelectable = false, int attack = -1, int life = -1)
        {
            CardSkinData skin = getSkin(card);
            servant.attackPropNumber.text.text = attack < 0 ? card.getAttack(game).ToString() : attack.ToString();
            servant.lifePropNumber.text.text = life < 0 ? card.getCurrentLife(game).ToString() : life.ToString();

            servant.onDrag.remove(onDragServant);
            //if (isSelectable)
            //{
            //    servant.onSelectableTrue.invokeBeforeAnim();
            //    servant.onSelectableTrue.invokeAfterAnim();
            //}
            //else
            //{
            //    servant.onSelectableFalse.invokeBeforeAnim();
            //    servant.onSelectableFalse.invokeAfterAnim();
            //}
            if (player == card.getOwner() && currentAnimPlayer == player && card.canAttack(game))
            {
                // servant.HighlightController = Servant.Highlight.Green;
                servant.onCanAttackTrue.invokeBeforeAnim();
                servant.onCanAttackTrue.invokeAfterAnim();
                servant.onDrag.add(onDragServant);
                servant.onDragEnd.add(onDragEndServant);
            }
            else
            {
                // servant.HighlightController = Servant.Highlight.None;
                servant.onCanAttackFalse.invokeBeforeAnim();
                servant.onCanAttackFalse.invokeAfterAnim();
            }
            if (card.isTaunt(game))
            {
                servant.onTauntTrue.invokeBeforeAnim();
                servant.onTauntTrue.invokeAfterAnim();
            }
            else
            {
                servant.onTauntFalse.invokeBeforeAnim();
                servant.onTauntFalse.invokeAfterAnim();
            }
            if (card.isShield(game))
            {
                servant.onShieldTrue.invokeBeforeAnim();
                servant.onShieldTrue.invokeAfterAnim();
            }
            else
            {
                servant.onShieldFalse.invokeBeforeAnim();
                servant.onShieldFalse.invokeAfterAnim();
            }
            if (card.isStealth(game))
            {
                servant.onStealthTrue.invokeBeforeAnim();
                servant.onStealthTrue.invokeAfterAnim();
            }
            else
            {
                servant.onStealthFalse.invokeBeforeAnim();
                servant.onStealthFalse.invokeAfterAnim();
            }
            if (card.hasDeathRattle(game))
            {
                servant.onDyingWishTrue.invokeBeforeAnim();
                servant.onDyingWishTrue.invokeAfterAnim();
            }
            else
            {
                servant.onDyingWishFalse.invokeBeforeAnim();
                servant.onDyingWishFalse.invokeAfterAnim();
            }
            if (card.hasKeyword(game, KEYWORD.POISONOUS))
            {
                servant.onLethalTrue.invokeBeforeAnim();
                servant.onLethalTrue.invokeAfterAnim();
            }
            else
            {
                servant.onLethalFalse.invokeBeforeAnim();
                servant.onLethalFalse.invokeAfterAnim();
            }
            if (card.hasKeyword(game, KEYWORD.DRAIN))
            {
                servant.onLifeStealTrue.invokeBeforeAnim();
                servant.onLifeStealTrue.invokeAfterAnim();
            }
            else
            {
                servant.onLifeStealFalse.invokeBeforeAnim();
                servant.onLifeStealFalse.invokeAfterAnim();
            }
            if (card.isFreeze(game))
            {
                servant.onShackleTrue.invokeBeforeAnim();
                servant.onShackleTrue.invokeAfterAnim();
            }
            else
            {
                servant.onShackleFalse.invokeBeforeAnim();
                servant.onShackleFalse.invokeAfterAnim();
            }
            if (card.isSilenced(game))
            {
                servant.onSilenceTrue.invokeBeforeAnim();
                servant.onSilenceTrue.invokeAfterAnim();
            }
            else
            {
                servant.onSilenceFalse.invokeBeforeAnim();
                servant.onSilenceFalse.invokeAfterAnim();
            }
            //if (card.hasKeyword(game, KEYWORD.STEALTH))
            //    servant.stealthImage.display();
            //else
            //    servant.stealthImage.hide();
            //if (card.hasKeyword(game, KEYWORD.FREEZE))
            //    servant.freezeImage.display();
            //else
            //    servant.freezeImage.hide();
            //if (card.hasKeyword(game, KEYWORD.SHIELD))
            //    servant.shieldImage.display();
            //else
            //    servant.shieldImage.hide();
            //getChild("Root").getChild("Shield").gameObject.SetActive(card.isShield());
            if (skin != null && skin.image != null)
            {
                servant.image.sprite = skin.image;
            }
            else
            {
                var cards = getManager<CardManager>();
                var poolRef = cards.getCardPoolReferenceOfCard(card.define);
                servant.image.sprite = await cards.loadCardImage(poolRef, card.define.id);
            }
        }
        [SerializeField]
        BJSYGameCore.Timer _showCardTimer = new BJSYGameCore.Timer() { duration = 0.25f };
        ServantItem _servant;
        PointerEventData _pointer;
        void onEnterServant(ServantItem servant, PointerEventData pointer)
        {
            _showCardTimer.start();
            _servant = servant;
            _pointer = pointer;
            //displayLargeCard(pointer.position.x < Screen.width / 2, getCard(servant));
        }
        void onExitServant(ServantItem servant, PointerEventData pointer)
        {
            _showCardTimer.reset();
            hideLargeCard();
        }
        /// <summary>
        /// 随从拖拽处理
        /// </summary>
        /// <param name="servant"></param>
        /// <param name="pointer"></param>
        void onDragServant(ServantItem servant, PointerEventData pointer)
        {
            if (!canControl)//不是你的回合
                return;
            Card card = getCard(servant);
            if (card.getOwner() != player)//不是你的卡
                return;
            if (!card.canAttack(game))
                return;
            //拉动距离也应该有一个阈值
            if (Vector2.Distance(servant.rectTransform.position, pointer.position) > servant.attackThreshold)
            {
                //播放一个变大的动画？
                //servant.rectTransform.localScale = Vector3.one * 1.1f;
                //显示指针
                displayArrow(ui.SelfFieldList.rectTransform, servant.rectTransform.position, pointer.position);
                //高亮标记所有目标
                highlightTargets(game.findAllCardsInField(c => card.isAttackable(game, player, c, out _)));
            }
            else
            {
                cancelAttack();
            }
        }
        /// <summary>
        /// 高亮所有指定的目标（标记为可选择）
        /// </summary>
        /// <param name="targets"></param>
        void highlightTargets(Card[] targets)
        {
            foreach (var target in targets)
            {
                if (tryGetMaster(target, out var master))
                {
                    master.onSelectableTrue.invokeBeforeAnim();
                    master.onSelectableTrue.invokeAfterAnim();
                    master.onClick.add(onClickMaster);
                }
                else if (tryGetServant(target, out var servant))
                {
                    servant.onSelectableTrue.invokeBeforeAnim();
                    servant.onSelectableTrue.invokeAfterAnim();
                    servant.onClick.add(onClickServant);
                }
            }
        }
        /// <summary>
        /// 关闭所有目标的高亮（可选择）
        /// </summary>
        void removeHighlights()
        {
            ui.SelfMaster.onSelectableFalse.invokeBeforeAnim();
            ui.SelfMaster.onSelectableFalse.invokeAfterAnim();
            ui.EnemyMaster.onSelectableFalse.invokeBeforeAnim();
            ui.EnemyMaster.onSelectableFalse.invokeAfterAnim();
            foreach (var servant in ui.SelfFieldList)
            {
                servant.onSelectableFalse.invokeBeforeAnim();
                servant.onSelectableFalse.invokeAfterAnim();
            }
            foreach (var servant in ui.EnemyFieldList)
            {
                servant.onSelectableFalse.invokeBeforeAnim();
                servant.onSelectableFalse.invokeAfterAnim();
            }
        }
        /// <summary>
        /// 随从拖拽结束处理
        /// </summary>
        /// <param name="servant"></param>
        /// <param name="pointer"></param>
        void onDragEndServant(ServantItem servant, PointerEventData pointer)
        {
            if (!canControl)//不是你的回合
                return;
            Card card = getCard(servant);
            if (card.owner != player)//不是你的卡
                return;
            if (!card.canAttack(game))//不能攻击
                return;
            //如果在随从或英雄上面
            if (isOnTarget(pointer, out var target))
            {
                if (card.isAttackable(game, player, target, out var tip))
                    player.cmdAttack(game, card, target);
                else
                    showTip(tip);
            }
            //取消选中和攻击
            cancelAttack();
        }
        /// <summary>
        /// 英雄拖拽处理
        /// </summary>
        /// <param name="master"></param>
        /// <param name="pointer"></param>
        void onDragMaster(Master master, PointerEventData pointer)
        {
            if (!canControl)//不是你的回合
                return;
            TouhouCardEngine.Card card = getCard(master);
            if (card.getOwner() != player)//不是你的英雄
                return;
            if (!card.canAttack(game))
                return;
            //拉动距离也应该有一个阈值
            if (Vector2.Distance(master.rectTransform.position, pointer.position) > master.attackThreshold)
            {
                //播放一个变大的动画？
                //servant.rectTransform.localScale = Vector3.one * 1.1f;
                //显示指针
                displayArrow(master.rectTransform, master.rectTransform.position, pointer.position);
                //高亮标记所有目标
                highlightTargets(game.findAllCardsInField(c => card.isAttackable(game, player, c, out _)));
            }
            else
            {
                cancelAttack();
            }
        }
        /// <summary>
        /// 英雄拖拽结束处理
        /// </summary>
        /// <param name="master"></param>
        /// <param name="pointer"></param>
        void onDragEndMaster(Master master, PointerEventData pointer)
        {
            if (!canControl)//不是你的回合
                return;
            Card card = getCard(master);
            if (card.owner != player)//不是你的英雄
                return;
            if (!card.canAttack(game))//不能攻击
                return;
            //如果在随从上面
            if (isOnTarget(pointer, out var target))
            {
                if (card.isAttackable(game, player, target, out var tip))
                    player.cmdAttack(game, card, target);
                else
                    showTip(tip);
            }
            //取消选中和攻击
            cancelAttack();
        }
        /// <summary>
        /// 判断鼠标是否在某张卡上，并返回卡
        /// </summary>
        /// <param name="pointer"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        bool isOnTarget(PointerEventData pointer, out TouhouCardEngine.Card card)
        {
            if (pointer.pointerCurrentRaycast.gameObject != null)
            {
                if (pointer.pointerCurrentRaycast.gameObject.GetComponentInParent<ServantItem>() is ServantItem targetServant)
                {
                    card = getCard(targetServant);
                    return card != null;
                }
                else if (pointer.pointerCurrentRaycast.gameObject.GetComponentInParent<Master>() is Master targetMaster)
                {
                    card = getCard(targetMaster);
                    return card != null;
                }
            }
            card = null;
            return false;
        }
        /// <summary>
        /// 取消攻击
        /// </summary>
        public void cancelAttack()
        {
            //缩小动画
            //rectTransform.localScale = Vector3.one;
            ui.AttackArrowImage.hide();
            removeHighlights();
        }
        /// <summary>
        /// 设置随从定义
        /// </summary>
        /// <param name="servant"></param>
        /// <param name="cardDefine"></param>
        public async void setServant(ServantItem servant, CardDefine cardDefine)
        {
            CardSkinData skin = getSkin(cardDefine);
            servant.attackPropNumber.text.text = cardDefine.getAttack().ToString();
            servant.lifePropNumber.text.text = cardDefine.getLife().ToString();
            if (cardDefine.hasKeyword(KEYWORD.TAUNT))
            {
                servant.onTauntTrue.invokeBeforeAnim();
                servant.onTauntTrue.invokeAfterAnim();
            }
            else
            {
                servant.onTauntFalse.invokeBeforeAnim();
                servant.onTauntFalse.invokeAfterAnim();
            }
            if (skin != null && skin.image != null)
            {
                servant.image.sprite = skin.image;
            }
            else
            {
                var cards = getManager<CardManager>();
                var poolRef = cards.getCardPoolReferenceOfCard(cardDefine);
                servant.image.sprite = await cards.loadCardImage(poolRef, cardDefine.id);
            }
        }
        /// <summary>
        /// 从随从获取对应的卡
        /// </summary>
        /// <param name="servant"></param>
        /// <returns></returns>
        public Card getCard(ServantItem servant)
        {
            foreach (var pair in cardServantDic)
            {
                if (pair.Value == servant)
                    return pair.Key;
            }
            return null;
        }

        ///// <summary>
        ///// 获取一张卡上的数字
        ///// </summary>
        //public bool tryGetAllPropNumber(Card card, out PropNumber[] numbers)
        //{
        //    if (tryGetMaster(card, out var master))
        //    {
        //        PropNumber[] props = { master.LifePropNumber, master.AttackPropNumber };
        //        numbers = props;
        //        return true;
        //    }
        //    if (tryGetHand(card, out var hand))
        //    {
        //        if (card.define is ServantCardDefine)
        //        {
        //            PropNumber[] props = { hand.Card.CostPropNumber, hand.Card.LifePropNumber, hand.Card.AttackPropNumber };
        //            numbers = props;
        //            return true;
        //        }
        //        if (card.define is SpellCardDefine)
        //        {
        //            PropNumber[] props = { hand.Card.CostPropNumber };
        //            numbers = props;
        //            return true;
        //        }
        //        if (card.define is ItemCardDefine)
        //        {
        //            PropNumber[] props = { hand.Card.CostPropNumber, hand.Card.LifePropNumber, hand.Card.AttackPropNumber };
        //            numbers = props;
        //            return true;
        //        }
        //    }
        //    if (tryGetItem(card, out var item))
        //    {
        //        PropNumber[] props = { item.DurabilityPropNumber, item.AttackPropNumber };
        //        numbers = props;
        //        return true;
        //    }
        //    if (tryGetSkill(card, out var skill))
        //    {
        //        PropNumber[] props = { skill.CostPropNumber };
        //        numbers = props;
        //        return true;
        //    }
        //    if (tryGetServant(card, out var servant))
        //    {
        //        PropNumber[] props = { servant.HpTextPropNumber, servant.AttackTextPropNumber };
        //        numbers = props;
        //        return true;
        //    }
        //    numbers = null;
        //    return false;
        //}

        /// <summary>
        /// 隐藏随从的占位符
        /// </summary>
        private void hideServantPlaceHolder()
        {
            ui.addChild(ui.ServantPlaceHolder.rectTransform);
            ui.ServantPlaceHolder.Servant.hide();
            ui.ServantPlaceHolder.hide();
        }



        public void displayDiscoverDialog(DefineReference[] cardRefs, string title)
        {
            ui.Discover.display();
            if (string.IsNullOrEmpty(title))
                ui.Discover.Text.text = StringTable.HINT_SELECT_A_CARD;
            else
                ui.Discover.Text.text = title;
            LayoutRebuilder.ForceRebuildLayoutImmediate(ui.Discover.Text.rectTransform);
            ui.Discover.HoriCardList.clearItems();
            foreach (var cardRef in cardRefs)
            {
                var item = ui.Discover.HoriCardList.addItem();
                var cardDefine = game.getDefine(cardRef.cardPoolId, cardRef.defineId);
                setCard(item.Card, cardDefine, cardDefine.getCost(), cardDefine.getAttack(), cardDefine.getLife(),
                    true);
                item.asButton.onClick.set(() =>
                {
                    game.answers.answer(player.id, new DiscoverResponse()
                    {
                        cardDefRef = cardDefine.getDefineRef()
                    });
                    closeDiscoverDialog();
                });
            }
        }
        public void displayDiscoverDialog(int[] cardIds, string title)
        {
            ui.Discover.display();
            if (string.IsNullOrEmpty(title))
                ui.Discover.Text.text = StringTable.HINT_SELECT_A_CARD;
            else
                ui.Discover.Text.text = title;
            LayoutRebuilder.ForceRebuildLayoutImmediate(ui.Discover.Text.rectTransform);
            ui.Discover.HoriCardList.clearItems();
            foreach (var cardId in cardIds)
            {
                var item = ui.Discover.HoriCardList.addItem();
                var card = game.getCard(cardId);
                if (card != null)
                {
                    setCard(item.Card, card, true);
                    item.asButton.onClick.set(() =>
                    {
                        game.answers.answer(player.id, new DiscoverResponse()
                        {
                            cardId = card.id
                        });
                        closeDiscoverDialog();
                    });
                }
            }
        }
        [TranslateMsg]
        private const String TAB_SHOW = "显示";
        [TranslateMsg]
        private const String TAB_HIDE = "隐藏";
        void onDiscoverHideButtonClick()
        {
            if (ui.Discover.PanelImage.gameObject.activeSelf)
            {
                ui.Discover.ButtonButtonBlack.asButton.setText(lang._(TAB_SHOW));
                ui.Discover.PanelImage.hide();
            }
            else
            {
                ui.Discover.ButtonButtonBlack.asButton.setText(lang._(TAB_HIDE));
                ui.Discover.PanelImage.display();
                LayoutRebuilder.ForceRebuildLayoutImmediate(ui.Discover.Text.rectTransform);
                DiscoverRequest request = game.answers.getRequest<DiscoverRequest>(player.id);
                if (request.cardRefArray != null)
                    displayDiscoverDialog(request.cardRefArray, ui.Discover.Text.text);
                else
                    displayDiscoverDialog(request.cardIdArray, ui.Discover.Text.text);
            }
        }
        public void closeDiscoverDialog()
        {
            ui.Discover.hide();
        }

        [TranslateMsg]
        private const String TAB_YOUR_BOX = "你的牌库";
        [TranslateMsg]
        private const String TAB_BOX_HAS = "牌库里有{0}张卡牌";
        [TranslateMsg]
        private const String TAB_YOUR_NO_HAVE_CARD = "你已经没有卡牌了";
        void onDeckClick()
        {
            ui.PilePanel.display(lang._(TAB_YOUR_BOX));
            ui.PilePanel.VerticalOverflowCardList.clearItems();
            foreach (var card in player.getDeck().OrderBy(c => c.define.getCost()))
            {
                var item = ui.PilePanel.VerticalOverflowCardList.addItem();
                setCard(item.Card, card, true);
            }
        }
        void onDeckPointerEnter(UI.Deck deck, PointerEventData pointer)
        {
            if (player.getDeck().count > 0)
                ui.TipBox.display(lang._(TAB_BOX_HAS, player.getDeck().count), pointer.position + _tipBoxOffset);
            else
                ui.TipBox.display(lang._(TAB_YOUR_NO_HAVE_CARD), pointer.position + _tipBoxOffset);
        }
        void onDeckPointerExit(UI.Deck deck, PointerEventData pointer)
        {
            ui.TipBox.hide();
        }
        [TranslateMsg]
        private const String TAB_YOUR_GRAVEYARD = "你的墓地";

        void onGraveClick()
        {
            ui.PilePanel.display(lang._(TAB_YOUR_GRAVEYARD));
            ui.PilePanel.VerticalOverflowCardList.clearItems();
            foreach (var card in player.getGrave())
            {
                var item = ui.PilePanel.VerticalOverflowCardList.addItem();
                setCard(item.Card, card, true);
            }
        }
        [TranslateMsg]
        private const String TAB_GRAVEYARD_HAS = "墓地里有{0}张卡牌";
        [TranslateMsg]
        private const String TAB_GRAVEYARD_NULL = "墓地是空的";
        void onGravePointerEnter(UI.Deck deck, PointerEventData pointer)
        {
            if (player.getGrave().count > 0)
                ui.TipBox.display(lang._(TAB_GRAVEYARD_HAS, player.getGrave().count), pointer.position + _tipBoxOffset);
            else
                ui.TipBox.display(lang._(TAB_GRAVEYARD_NULL), pointer.position + _tipBoxOffset);
        }

        private void onDeckPointerMove(UI.Deck arg1, Vector2 arg2)
        {
            ui.TipBox.updatePos(arg2);
        }

        void onGravePointerExit(UI.Deck deck, PointerEventData pointer)
        {
            ui.TipBox.hide();
        }
        [SerializeField]
        List<Projectile> _projectileList = new List<Projectile>();
        public Projectile createProjectile(Projectile prefab, RectTransform from, RectTransform to)
        {
            Projectile projectile = Instantiate(prefab, from.position, Quaternion.LookRotation(to.position - from.position, Vector3.forward), ui.FieldsImage.transform);
            projectile.transform.SetSiblingIndex(to.getBranch(ui.FieldsImage.transform).GetSiblingIndex() + 1);
            _projectileList.Add(projectile);
            return projectile;
        }
        [SerializeField]
        BJSYGameCore.Timer _tipTimer = new BJSYGameCore.Timer() { duration = 2 };
        /// <summary>
        /// 显示文本提示
        /// </summary>
        /// <param name="tip">翻译的**源文本**</param>
        public void showTip(string tip)
        {
            ui.TipText.gameObject.SetActive(true);
            ui.TipText.text = lang._(tip);
            _tipTimer.start();
        }
        public void displayLargeCard(bool isRight, Card card)
        {
            if (isRight)
                ui.LargeCard.rectTransform.localPosition = new Vector3(250, 0);
            else
                ui.LargeCard.rectTransform.localPosition = new Vector3(-250, 0);
            ui.LargeCard.display();
            if (tryGetServant(card, out var servant))
                setServant(servant, card);
            setCard(ui.LargeCard, card, true);
        }
        public void hideLargeCard()
        {
            ui.LargeCard.hide();
        }
        void onTurnEndButtonClick()
        {
            resetUse(true, true);
            player.cmdTurnEnd(game);
        }
        #endregion
        #region Skin
        public CardSkinData getSkin(Card card)
        {
            CardSkinData skin = getManager<CardManager>().getCardSkin(card.define);
            if (skin != null)
                return skin;
            else
            {
                game.logger.logError("没有找到" + card + "对应的皮肤");
                return new CardSkinData()
                {
                    id = card.define.id,
                    desc = card.define.ToString() + "（这张卡随时可能爆炸！）",
                    image = getManager<CardManager>().getDefaultSprite().Result,
                    name = card.define.GetType().Name
                };
            }
        }
        public CardSkinData getSkin(CardDefine define)
        {
            CardSkinData skin = getManager<CardManager>().getCardSkin(define);
            if (skin != null)
                return skin;
            else
            {
                game.logger.logError("没有找到" + define + "对应的皮肤");
                return new CardSkinData()
                {
                    id = define.id,
                    desc = define.ToString() + "（这张卡随时可能爆炸！）",
                    image = getManager<CardManager>().getDefaultSprite().Result,
                    name = define.GetType().Name
                };
            }
        }
        #endregion
        #region 属性字段
        Card _selfItemCard;
        Card _enemyItemCard;
        #endregion
    }
}