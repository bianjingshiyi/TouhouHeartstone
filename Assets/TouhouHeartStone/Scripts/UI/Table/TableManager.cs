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
namespace Game
{
    public partial class TableManager : Manager
    {
        public THHGame game { get; private set; } = null;
        public THHPlayer player { get; private set; } = null;
        [SerializeField]
        Table _table;
        public Table ui
        {
            get { return _table; }
            set { _table = value; }
        }
        [Header("Config")]
        [SerializeField]
        float _skillDragThreshold = 75;
        public float skillDragThreshold => _skillDragThreshold;
        [SerializeField]
        AnimationCurve _moveToFieldCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        public AnimationCurve handToFieldCurve => _moveToFieldCurve;
        [SerializeField]
        Gradient timeoutGradient = new Gradient();
        [SerializeField]
        UI.Card _cardPrefab;
        public UI.Card cardPrefab => _cardPrefab;
        [Header("State")]
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
        public TouhouCardEngine.Card attackingCard { get; set; } = null;
        protected override void onAwake()
        {
            base.onAwake();
            if (ui == null)
                ui = this.findInstance<Table>();
            loadAnim(GetType().Assembly);
        }
        protected void Update()
        {
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
            if (game.turnTimer != null && game.turnTimer.remainedTime <= 15)
            {
                ui.TimeoutSlider.display();
                ui.TimeoutSlider.value = game.turnTimer.remainedTime / 15;
                ui.TimeoutSlider.fillRect.GetComponent<Image>().color = timeoutGradient.Evaluate(1 - ui.TimeoutSlider.value);
            }
            else
                ui.TimeoutSlider.hide();
            updateAnim();
        }
        /// <summary>
        /// 设置游戏对象和玩家并初始化UI
        /// </summary>
        /// <param name="game"></param>
        /// <param name="player"></param>
        public void setGame(THHGame game, THHPlayer player)
        {
            ui.onClickNoWhere.set(onClickNoWhere);
            ui.InitReplaceDialog.hide();
            ui.TurnTipImage.hide();
            initMaster(ui.SelfMaster);
            ui.SelfSkill.asButton.onClick.set(onClickSkill);
            ui.SelfSkill.onDrag.set(onDragSkill);
            ui.SelfSkill.onDragEnd.set(onDragSkillEnd);
            ui.SelfItem.hide();
            ui.SelfHandList.clearItems();
            ui.SelfHandList.asButton.onClick.set(() =>
            {
                if (ui.SelfHandList.isExpanded)
                    ui.SelfHandList.shrink();
                else
                    ui.SelfHandList.expand();
            });
            ui.SelfFieldList.clearItems();
            ui.TurnEndButton.onClick.set(onTurnEndButtonClick);
            ui.TipText.hide();
            initMaster(ui.EnemyMaster);
            ui.EnemyItem.hide();
            ui.EnemyFieldList.clearItems();
            ui.EnemyHandList.clearItems();
            ui.AttackArrowImage.hide();
            ui.Fatigue.hide();
            ui.Discover.Button.onClick.set(onDiscoverHideButtonClick);
            ui.Discover.hide();
            _animationQueue.Clear();

            if (game != null)
            {
                game.triggers.onEventBefore -= onEventBefore;
                game.triggers.onEventAfter -= onEventAfter;
                game.answers.onRequest -= onRequest;
                game.answers.onResponse -= onResponse;
            }
            this.game = game;
            if (game != null)
            {
                game.triggers.onEventBefore += onEventBefore;
                game.triggers.onEventAfter += onEventAfter;
                game.answers.onRequest += onRequest;
                game.answers.onResponse += onResponse;
            }
            this.player = player;
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
            else
                UberDebug.LogWarningChannel("UI", "没有与" + obj + "相应的动画");
        }
        void onResponse(IResponse response)
        {
            if (response.playerId != player.id)
                return;
            switch (response)
            {
                case DiscoverResponse _:
                    closeDiscoverDialog();
                    break;
                default:
                    break;
            }
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
                case TouhouCardEngine.Card.AddModiEventArg addMod:
                    anim = new AddModiAnim() { eventArg = addMod };
                    break;
                default:
                    anim = getEventAnim(arg);
                    break;
            }
            if (anim != null)
                addAnim(anim);
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
            if (usingCard != null && usingCard.define is ServantCardDefine && isSelectingTarget)
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
        public bool tryGetMaster(TouhouCardEngine.Card card, out Master master)
        {
            if (player.master == card)
            {
                master = ui.SelfMaster;
                return true;
            }
            else if (game.getOpponent(player).master == card)
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
        /// 初始化Master的事件
        /// </summary>
        /// <param name="master"></param>
        public void initMaster(Master master)
        {
            master.onClick.add(onClickMaster);
        }
        /// <summary>
        /// 设置Master的UI
        /// </summary>
        /// <param name="master"></param>
        /// <param name="card"></param>
        /// <param name="isSelectable"></param>
        public void setMaster(Master master, TouhouCardEngine.Card card, bool isSelectable = false)
        {
            CardSkinData skin = getSkin(card);
            master.Image.sprite = skin.image;
            master.LifePropNumber.asText.text = card.getCurrentLife(game).ToString();
            //if (card.getCurrentLife() == card.getLife())
            //    HpText.color = Color.white;
            //else
            //    HpText.color = Color.red;
            if (card.getAttack(game) > 0)
            {
                master.AttackPropNumber.asText.text = card.getAttack(game).ToString();
                master.AttackPropNumber.asText.display();
                master.AttackPropNumber.asText.display();
            }
            else
            {
                master.AttackPropNumber.asText.hide();
                master.AtkImage.hide();
            }
            if (card.getArmor(game) > 0)
            {
                master.ArmorPropNumber.asText.text = card.getArmor(game).ToString();
                master.ArmorPropNumber.display();
                master.ArmorImage.display();
            }
            else
            {
                master.ArmorPropNumber.asText.hide();
                master.ArmorImage.hide();
            }

            if (isSelectable)
            {
                // master.HighlightController = Master.Highlight.Yellow;
                master.onSelectableTrue.beforeAnim.Invoke();
                master.onSelectableTrue.afterAnim.Invoke();
            }
            else
            {
                master.onSelectableFalse.beforeAnim.Invoke();
                master.onSelectableFalse.afterAnim.Invoke();
            }
            if (card.getOwner() == player && game.currentPlayer == player && card.canAttack(game))
            {
                // master.HighlightController = Master.Highlight.Green;
                master.onCanAttackTrue.beforeAnim.Invoke();
                master.onCanAttackTrue.afterAnim.Invoke();
            }
            else
            {
                // master.HighlightController = Master.Highlight.None;
                master.onCanAttackFalse.beforeAnim.Invoke();
                master.onCanAttackFalse.afterAnim.Invoke();
            }
        }
        public bool tryGetItem(Card card, out Item item)
        {
            if (player.item == card)
            {
                item = ui.SelfItem;
                return false;
            }
            else if (game.getOpponent(player).item == card)
            {
                item = ui.EnemyItem;
                return true;
            }
            item = null;
            return false;
        }
        public void setItem(Item item, Card card)
        {
            if (card == null)
            {
                item.hide();
                return;
            }
            item.display();
            CardSkinData skin = getSkin(card);
            item.Image.sprite = skin.image;

            item.AttackPropNumber.asText.text = card.getAttack(game).ToString();
            item.DurabilityPropNumber.asText.text = card.getCurrentLife(game).ToString();
        }
        /// <summary>
        /// 设置Skill的UI
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="card"></param>
        public void setSkill(Skill skill, TouhouCardEngine.Card card)
        {
            CardSkinData skin = getSkin(card);
            skill.Image.sprite = skin.image;
            skill.CostPropNumber.asText.text = card.getCost(game).ToString();
            if (card.isUsed(game))
            {
                // skill.IsUsedController = Skill.IsUsed.True;
                skill.isUsed = true;
            }
            else
            {
                // skill.IsUsedController = Skill.IsUsed.False;
                skill.isUsed = false;
            }
            if (card.isUsable(game, player, out _) &&//技能是可用的
                !isSelectingTarget &&//没有在选择目标
                canControl//是自己的回合
                )
            {
                // skill.IsUsableController = Skill.IsUsable.True;
                skill.isUsable = true;
                skill.asButton.interactable = true;
            }
            else
            {
                // skill.IsUsableController = Skill.IsUsable.False;
                skill.isUsable = false;
                skill.asButton.interactable = false;
            }
        }
        /// <summary>
        /// 技能点击处理
        /// </summary>
        void onClickSkill()
        {
            if (!canControl)
                return;
            if (tryCancelUse())
                return;
            if (usingCard != null)//已经在用别的牌了，不能点技能
                return;
            if (player.skill.isUsable(game, player, out var info))
            {
                if (!player.skill.isNeedTarget(game, out _))
                    player.cmdUse(game, player.skill);
            }
            else
            {
                showTip(info);
            }
        }
        /// <summary>
        /// 技能拖拽
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="pointer"></param>
        void onDragSkill(Skill skill, PointerEventData pointer)
        {
            if (!canControl)
                return;
            // Stage2: 当前正在拖拽，判断距离以决定是否退出目标选择模式
            if (usingCard == player.skill)
            {
                if (Vector3.Distance(skill.rectTransform.position, pointer.position) < skillDragThreshold)
                    cancelSkill();
                else
                    displayArrow(skill.rectTransform.position, pointer.position);
                return;
            }
            // Stage1: 当前没有拖拽，进入目标选择模式
            if (usingCard != null)
                return;//已经在用别的牌了
            if (Vector3.Distance(skill.rectTransform.position, pointer.position) < skillDragThreshold)
                return;
            if (player.skill.isUsable(game, player, out var info))
            {
                if (player.skill.isNeedTarget(game, out var targets))
                {
                    usingCard = player.skill;
                    highlightTargets(targets);
                    displayArrow(skill.rectTransform.position, pointer.position);
                }
            }
            else
                showTip(info);
        }
        /// <summary>
        /// 技能结束拖拽
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="pointer"></param>
        void onDragSkillEnd(Skill skill, PointerEventData pointer)
        {
            if (!canControl)
            {
                cancelSkill();
                return;
            }
            if (usingCard != null && usingCard != player.skill)
                return;//在用其他卡片
            if (usingCard != player.skill)
            {
                cancelSkill();
                return;
            }
            if (isOnTarget(pointer, out var target))
            {
                if (player.skill.isUsable(game, player, out var info) && player.skill.isValidTarget(game, target))
                    player.cmdUse(game, player.skill, 0, target);
                else
                    showTip(info);
            }
            cancelSkill();
        }
        /// <summary>
        /// 取消正在执行的技能UI
        /// </summary>
        private void cancelSkill()
        {
            usingCard = null;
            removeHighlights();
            hideArrow();
        }
        /// <summary>
        /// 显示箭头
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        void displayArrow(Vector2 from, Vector2 to)
        {
            ui.AttackArrowImage.display();
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
        Dictionary<TouhouCardEngine.Card, HandListItem> cardHandDic { get; } = new Dictionary<TouhouCardEngine.Card, HandListItem>();
        /// <summary>
        /// 创建一张手牌
        /// </summary>
        /// <param name="card"></param>
        /// <param name="position">默认为-1也就是最右手</param>
        /// <returns></returns>
        public HandListItem createHand(TouhouCardEngine.Card card, int position = -1)
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
            UberDebug.LogChannel("UI", "创建手牌UI：" + card);
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
                setCard(item.Card, card, false);
            }
            item.gameObject.name = card.ToString();
            item.onDrag.set(onDragHand);
            item.onEndDrag.set(onDragHandEnd);
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
            TouhouCardEngine.Card card = getCard(hand);
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
                    if (usingCard.define is ServantCardDefine)
                    {
                        //如果手牌是随从，那么在场上的时候会有一个占位符，预览这个随从放下去的位置。
                        ui.SelfFieldList.defaultItem.rectTransform.SetAsFirstSibling();
                        ui.ServantPlaceHolder.rectTransform.sizeDelta = ui.SelfFieldList.defaultItem.rectTransform.sizeDelta;
                        ui.SelfFieldList.addChild(ui.ServantPlaceHolder.rectTransform);
                        ui.ServantPlaceHolder.display();
                        ui.ServantPlaceHolder.rectTransform.SetSiblingIndex(calcIndexInField(pointer.position) + 1);
                    }
                    else if (usingCard.define is SpellCardDefine)
                    {
                        if (usingCard.isNeedTarget(game, out var targets))
                        {
                            if (!isSelectingTarget)
                            {
                                isSelectingTarget = true;//TODO:这个地方一定要重写，怎么能这样的？isSelectingTarget就应该被usingCard取代
                                highlightTargets(targets);
                                ui.SelfHandList.shrink();
                            }
                            hand.Card.hide();
                            if (tryGetMaster(usingCard.getOwner().master, out var castMaster))
                                displayArrow(castMaster.rectTransform.position, pointer.position);
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
                else if (usingCard.define is ServantCardDefine)
                {
                    //松开鼠标，确认使用随从牌
                    int index = calcIndexInField(pointer.position);
                    ui.ServantPlaceHolder.rectTransform.SetSiblingIndex(index + 1);
                    if (usingCard.getAvaliableTargets(game) is TouhouCardEngine.Card[] targets && targets.Length > 0)
                    {
                        usingPosition = index;
                        //进入选择目标状态，固定手牌到占位上，高亮可以选择的目标
                        addAnim(new HandToFieldAnim(this, hand, ui.SelfFieldList, index));
                        TouhouCardEngine.Card localUsingCard = usingCard;
                        addAnim(new CodeAnim(() =>
                        {
                            hand.Card.hide();
                            //显示占位随从
                            ui.ServantPlaceHolder.Servant.display();
                            try
                            {
                                setServant(ui.ServantPlaceHolder.Servant, localUsingCard.define);
                            }
                            catch (NullReferenceException e)
                            {
                                Debug.LogError(e);
                            }
                        }));
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
                else if (usingCard.define is SpellCardDefine)
                {
                    if (usingCard.isNeedTarget(game, out _))
                    {
                        //进入选择目标状态，高亮可以选择的目标
                        if (isOnTarget(pointer, out var target))
                        {
                            if (usingCard.isUsable(game, player, out info) && usingCard.isValidTarget(game, target))
                                player.cmdUse(game, usingCard, 0, target);
                            else
                                showTip(info);
                        }
                        resetUse(true, false);
                    }
                    else
                    {
                        //使用无目标随从牌
                        player.cmdUse(game, usingCard, 0);
                        resetUse(false, false);
                    }
                }
            }
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
                    showTip("这不是一个有效的目标！");
                    resetUse(true, true);
                }
            }
        }
        /// <summary>
        /// 点击随从卡
        /// </summary>
        /// <param name="servant"></param>
        /// <param name="pointer"></param>
        void onClickServant(Servant servant, PointerEventData pointer)
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
                    showTip("这不是一个有效的目标！");
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
                usingHand.Card.isFaceup = true;
                usingHand.Card.rectTransform.localScale = Vector3.one;
                usingHand.Card.rectTransform.localPosition = Vector2.zero;
            }
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
        public HandListItem getHand(TouhouCardEngine.Card card)
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
        public HandListItem tryGetHand(TouhouCardEngine.Card card)
        {
            var hand = getHand(card);
            if (hand == null)
                throw new ActorNotFoundException(card);
            return hand;
        }
        public bool tryGetHand(TouhouCardEngine.Card card, out HandListItem hand)
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
        /// 从UI手牌引用获取对应的牌
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public TouhouCardEngine.Card getCard(HandListItem item)
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
        public TouhouCardEngine.Card getCard(Master master)
        {
            if (master == ui.SelfMaster)
                return player.master;
            else
                return game.getOpponent(player).master;
        }
        public UI.Card createCard(Card card)
        {
            var ui = Instantiate(cardPrefab, this.ui.FieldsImage.rectTransform);
            setCard(ui, card, true);
            return ui;
        }
        /// <summary>
        /// 设置卡片UI
        /// </summary>
        /// <param name="ui"></param>
        /// <param name="card"></param>
        /// <param name="isFaceup"></param>
        public void setCard(UI.Card ui, Card card, bool isFaceup)
        {
            ui.CostPropNumber.asText.text = card.getCost(game).ToString();

            if (isFaceup)//因为isFaceup控制了很多GameObject的Active，所以它必须放在最上面。
            {
                CardSkinData skin = getSkin(card);
                ui.Image.sprite = skin.image;
                ui.NameText.text = skin.name;
                ui.DescText.text = CardDescHelper.replace(skin.desc, game, player, card);
                ui.isFaceup = true;
            }
            else
            {
                ui.isFaceup = false;
            }

            if (card.define.type == CardDefineType.SERVANT)
            {
                ui.type = CardCategory.SERVANT;
                ui.AttackPropNumber.asText.text = card.getAttack(game).ToString();
                ui.LifePropNumber.asText.text = card.getLife(game).ToString();
            }
            else
                ui.type = CardCategory.SPELL;
        }
        Dictionary<TouhouCardEngine.Card, Servant> cardServantDic { get; } = new Dictionary<TouhouCardEngine.Card, Servant>();
        /// <summary>
        /// 创建一个随从
        /// </summary>
        /// <param name="card"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public Servant createServant(TouhouCardEngine.Card card, int position)
        {
            Servant servant;
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
            setServant(servant, card);
            servant.onClick.add(onClickServant);
            servant.onDrag.add(onDragServant);
            servant.onDragEnd.add(onDragEndServant);
            servant.onExitServant.set(onExitServant);
            servant.onEnterServant.set(onEnterServant);
            cardServantDic.Add(card, servant);
            return servant;
        }
        /// <summary>
        /// 从卡获取对应的随从UI
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public Servant getServant(TouhouCardEngine.Card card)
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
        public bool tryGetServant(TouhouCardEngine.Card card, out Servant servant)
        {
            if (cardServantDic.ContainsKey(card))
            {
                servant = cardServantDic[card];
                return true;
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
        public void setServant(Servant servant, TouhouCardEngine.Card card, bool isSelectable = false)
        {
            CardSkinData skin = getSkin(card);
            if (skin != null)
            {
                servant.Image.sprite = skin.image;
            }
            servant.AttackTextPropNumber.asText.text = card.getAttack(game).ToString();
            servant.HpTextPropNumber.asText.text = card.getCurrentLife(game).ToString();

            servant.onDrag.remove(onDragServant);
            if (isSelectable)
            {
                servant.onSelectableTrue.beforeAnim.Invoke();
                servant.onSelectableTrue.afterAnim.Invoke();
            }
            else
            {
                servant.onSelectableFalse.beforeAnim.Invoke();
                servant.onSelectableFalse.afterAnim.Invoke();
            }
            if (player == card.getOwner() && game.currentPlayer == player && card.canAttack(game))
            {
                // servant.HighlightController = Servant.Highlight.Green;
                servant.onCanAttackTrue.beforeAnim.Invoke();
                servant.onCanAttackTrue.afterAnim.Invoke();
                servant.onDrag.add(onDragServant);
                servant.onDragEnd.add(onDragEndServant);
            }
            else
            {
                // servant.HighlightController = Servant.Highlight.None;
                servant.onCanAttackFalse.beforeAnim.Invoke();
                servant.onCanAttackFalse.afterAnim.Invoke();
            }
            if (card.isTaunt(game))
            {
                servant.onTauntTrue.beforeAnim.Invoke();
                servant.onTauntTrue.afterAnim.Invoke();
            }
            else
            {
                servant.onTauntFalse.beforeAnim.Invoke();
                servant.onTauntFalse.afterAnim.Invoke();
            }
            //getChild("Root").getChild("Shield").gameObject.SetActive(card.isShield());
        }
        void onEnterServant(Servant servant, PointerEventData pointer)
        {
            displayLargeCard(pointer.position.x < Screen.width / 2, getCard(servant));
        }
        void onExitServant(Servant servant, PointerEventData pointer)
        {
            hideLargeCard();
        }
        /// <summary>
        /// 随从拖拽处理
        /// </summary>
        /// <param name="servant"></param>
        /// <param name="pointer"></param>
        void onDragServant(Servant servant, PointerEventData pointer)
        {
            if (!canControl)//不是你的回合
                return;
            TouhouCardEngine.Card card = getCard(servant);
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
                displayArrow(servant.rectTransform.position, pointer.position);
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
        void highlightTargets(TouhouCardEngine.Card[] targets)
        {
            foreach (var target in targets)
            {
                if (tryGetMaster(target, out var master))
                {
                    master.onSelectableTrue.beforeAnim.Invoke();
                    master.onSelectableTrue.afterAnim.Invoke();
                    master.onClick.add(onClickMaster);
                }
                else if (tryGetServant(target, out var servant))
                {
                    servant.onSelectableTrue.beforeAnim.Invoke();
                    servant.onSelectableTrue.afterAnim.Invoke();
                    servant.onClick.add(onClickServant);
                }
            }
        }
        /// <summary>
        /// 关闭所有目标的高亮（可选择）
        /// </summary>
        void removeHighlights()
        {
            ui.SelfMaster.onSelectableFalse.beforeAnim.Invoke();
            ui.SelfMaster.onSelectableFalse.afterAnim.Invoke();
            ui.EnemyMaster.onSelectableFalse.beforeAnim.Invoke();
            ui.EnemyMaster.onSelectableFalse.afterAnim.Invoke();
            foreach (var servant in ui.SelfFieldList)
            {
                servant.onSelectableFalse.beforeAnim.Invoke();
                servant.onSelectableFalse.afterAnim.Invoke();
            }
            foreach (var servant in ui.EnemyFieldList)
            {
                servant.onSelectableFalse.beforeAnim.Invoke();
                servant.onSelectableFalse.afterAnim.Invoke();
            }
        }
        /// <summary>
        /// 随从拖拽结束处理
        /// </summary>
        /// <param name="servant"></param>
        /// <param name="pointer"></param>
        void onDragEndServant(Servant servant, PointerEventData pointer)
        {
            if (!canControl)//不是你的回合
                return;
            TouhouCardEngine.Card card = getCard(servant);
            if (card.owner != player)//不是你的卡
                return;
            if (!card.canAttack(game))//不能攻击
                return;
            //如果在随从上面
            if (isOnTarget(pointer, out var target))
            {
                if (card.isAttackable(game, player, target, out var tip))
                    player.cmdAttack(game, card, target);
                else
                    ui.showTip(tip);
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
                if (pointer.pointerCurrentRaycast.gameObject.GetComponentInParent<Servant>() is Servant targetServant)
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
        private void cancelAttack()
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
        /// <param name="card"></param>
        public void setServant(Servant servant, CardDefine card)
        {
            CardSkinData skin = getSkin(card);
            if (skin != null)
            {
                servant.Image.sprite = skin.image;
            }
            servant.AttackTextPropNumber.asText.text = card.getAttack().ToString();
            servant.HpTextPropNumber.asText.text = card.getLife().ToString();
            if (card.getProp<bool>("taunt"))
            {
                servant.onTauntTrue.beforeAnim.Invoke();
                servant.onTauntTrue.afterAnim.Invoke();
            }
            else
            {
                servant.onTauntFalse.beforeAnim.Invoke();
                servant.onTauntFalse.afterAnim.Invoke();
            }
        }
        /// <summary>
        /// 从随从获取对应的卡
        /// </summary>
        /// <param name="servant"></param>
        /// <returns></returns>
        public TouhouCardEngine.Card getCard(Servant servant)
        {
            foreach (var pair in cardServantDic)
            {
                if (pair.Value == servant)
                    return pair.Key;
            }
            return null;
        }
        /// <summary>
        /// 隐藏随从的占位符
        /// </summary>
        private void hideServantPlaceHolder()
        {
            ui.addChild(ui.ServantPlaceHolder.rectTransform);
            ui.ServantPlaceHolder.Servant.hide();
            ui.ServantPlaceHolder.hide();
        }
        public void displayDiscoverDialog(int[] cardIdArray, string title)
        {
            ui.Discover.display();
            ui.Discover.Text.text = title;
            ui.Discover.HoriCardList.clearItems();
            foreach (var cardId in cardIdArray)
            {
                var card = game.getCard(cardId);
                var item = ui.Discover.HoriCardList.addItem();
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
        void onDiscoverHideButtonClick()
        {
            if (ui.Discover.PanelImage.gameObject.activeSelf)
            {
                ui.Discover.Button.setText("显示");
                ui.Discover.PanelImage.hide();
            }
            else
            {
                ui.Discover.Button.setText("隐藏");
                ui.Discover.PanelImage.display();
                displayDiscoverDialog(game.answers.getRequest<DiscoverRequest>(player.id).cardIdArray, ui.Discover.Text.text);
            }
        }
        public void closeDiscoverDialog()
        {
            ui.Discover.hide();
        }
        [SerializeField]
        BJSYGameCore.Timer _tipTimer = new BJSYGameCore.Timer() { duration = 3 };
        /// <summary>
        /// 显示文本提示
        /// </summary>
        /// <param name="tip"></param>
        public void showTip(string tip)
        {
            ui.TipText.gameObject.SetActive(true);
            ui.TipText.text = tip;
            _tipTimer.start();
        }
        public void displayLargeCard(bool isRight, TouhouCardEngine.Card card)
        {
            if (isRight)
                ui.LargeCard.rectTransform.localPosition = new Vector3(250, 0);
            else
                ui.LargeCard.rectTransform.localPosition = new Vector3(-250, 0);
            ui.LargeCard.display();
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
        public CardSkinData getSkin(TouhouCardEngine.Card card)
        {
            CardSkinData skin = getManager<CardManager>().getSkin(card.define.id);
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
            CardSkinData skin = getManager<CardManager>().getSkin(define.id);
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
    }
}