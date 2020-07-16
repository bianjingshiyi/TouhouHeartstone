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
namespace Game
{
    public class TableManager : Manager
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
        public TouhouCardEngine.Card usingCard { get; set; } = null;
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
            updateAnim();
            if (_tipTimer.isExpired())
            {
                _tipTimer.reset();
                ui.TipText.hide();
            }
            else if (_tipTimer.isStarted)
            {
                ui.TipText.setAlpha(1/*_tipTimer.getRemainedTime() / _tipTimer.duration*/);
            }
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
            ui.SelfFieldList.clearItems();
            ui.TurnEndButton.onClick.set(onTurnEndButtonClick);
            ui.TipText.hide();
            initMaster(ui.EnemyMaster);
            ui.EnemyItem.hide();
            ui.EnemyFieldList.clearItems();
            ui.EnemyHandList.clearItems();
            ui.AttackArrowImage.hide();
            ui.Fatigue.hide();
            _animationQueue.Clear();

            if (game != null)
            {
                game.triggers.onEventBefore -= onEventBefore;
                game.triggers.onEventAfter -= onEventAfter;
                game.answers.onRequest -= onRequest;
            }
            this.game = game;
            if (game != null)
            {
                game.triggers.onEventBefore += onEventBefore;
                game.triggers.onEventAfter += onEventAfter;
                game.answers.onRequest += onRequest;
            }
            this.player = player;
        }
        /// <summary>
        /// 游戏核心Request对应处理
        /// </summary>
        /// <param name="obj"></param>
        private void onRequest(IRequest obj)
        {
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
        /// <summary>
        /// 事件前处理
        /// </summary>
        /// <param name="arg"></param>
        private void onEventBefore(IEventArg arg)
        {
            UIAnimation anim;
            switch (arg)
            {
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
            master.LifePropNumber.asText.text = card.getCurrentLife().ToString();
            //if (card.getCurrentLife() == card.getLife())
            //    HpText.color = Color.white;
            //else
            //    HpText.color = Color.red;
            if (card.getAttack() > 0)
            {
                master.AttackPropNumber.asText.text = card.getAttack().ToString();
                master.AttackPropNumber.asText.display();
                master.AttackPropNumber.asText.display();
            }
            else
            {
                master.AttackPropNumber.asText.hide();
                master.AtkImage.hide();
            }
            if (card.getArmor() > 0)
            {
                master.ArmorPropNumber.asText.text = card.getArmor().ToString();
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
                master.onHighlightControllerYellow?.Invoke();
            }
            else if (card.getOwner() == player && game.currentPlayer == player && card.canAttack(game))
            {
                // master.HighlightController = Master.Highlight.Green;
                master.onHighlightControllerGreen?.Invoke();
            }
            else
            {
                // master.HighlightController = Master.Highlight.None;
                master.onHighlightControllerNone?.Invoke();
            }
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
            skill.CostPropNumber.asText.text = card.getCost().ToString();
            if (card.isUsed())
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
        /// <returns></returns>
        public HandListItem createHand(TouhouCardEngine.Card card)
        {
            if (cardHandDic.ContainsKey(card))
            {
                UberDebug.LogErrorChannel("UI", "手牌中已经存在" + card + "对应UI" + cardHandDic[card]);
                return cardHandDic[card];
            }
            UberDebug.LogChannel("UI", "创建手牌UI：" + card);
            HandListItem item;
            if (card.getOwner() == player)
            {
                item = ui.SelfHandList.addItem();
                item.isDragable = true;
                setCard(item.Card, card, true);
            }
            else
            {
                item = ui.EnemyHandList.addItem();
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
                usingHand.Card.onFaceupControllerTrue.Invoke();
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
        /// <summary>
        /// 设置卡片UI
        /// </summary>
        /// <param name="ui"></param>
        /// <param name="card"></param>
        /// <param name="isFaceup"></param>
        public void setCard(UI.Card ui, TouhouCardEngine.Card card, bool isFaceup)
        {
            ui.CostPropNumber.asText.text = card.getCost().ToString();
            if (card.define.type == CardDefineType.SERVANT)
            {
                // ui.TypeController = UI.Card.Type.Servant;
                ui.onTypeControllerServant?.Invoke();
                ui.AttackPropNumber.asText.text = card.getAttack().ToString();
                ui.LifePropNumber.asText.text = card.getLife().ToString();
            }
            else
            {
                // ui.TypeController = UI.Card.Type.Spell;
                ui.onTypeControllerSpell?.Invoke();
            }

            if (isFaceup)
            {
                CardSkinData skin = getSkin(card);
                ui.Image.sprite = skin.image;
                ui.NameText.text = skin.name;
                ui.DescText.text = skin.desc;
                // ui.IsFaceupController = UI.Card.IsFaceup.True;
                ui.onFaceupControllerTrue?.Invoke();
            }
            else
            {
                // ui.IsFaceupController = UI.Card.IsFaceup.False;
                ui.onFaceupControllerFalse?.Invoke();
            }
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
            servant.AttackTextPropNumber.asText.text = card.getAttack().ToString();
            servant.HpTextPropNumber.asText.text = card.getCurrentLife().ToString();

            servant.onDrag.remove(onDragServant);
            if (isSelectable)
            {
                // servant.HighlightController = Servant.Highlight.Yellow;
                servant.onHighlightControllerYellow?.Invoke();
            }
            else if (player == card.getOwner() && game.currentPlayer == player && card.canAttack(game))
            {
                // servant.HighlightController = Servant.Highlight.Green;
                servant.onHighlightControllerGreen?.Invoke();
                servant.onDrag.add(onDragServant);
                servant.onDragEnd.add(onDragEndServant);
            }
            else
            {
                // servant.HighlightController = Servant.Highlight.None;
                servant.onHighlightControllerNone?.Invoke();
            }
            //getChild("Root").getChild("Taunt").gameObject.SetActive(card.isTaunt());
            //getChild("Root").getChild("Shield").gameObject.SetActive(card.isShield());
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
                    setMaster(master, target, true);
                    master.onClick.add(onClickMaster);
                }
                else if (tryGetServant(target, out var servant))
                {
                    setServant(servant, target, true);
                    servant.onClick.add(onClickServant);
                }
            }
        }
        /// <summary>
        /// 关闭所有目标的高亮（可选择）
        /// </summary>
        void removeHighlights()
        {
            setMaster(ui.SelfMaster, player.master, false);
            THHPlayer opponent = game.getOpponent(player);
            setMaster(ui.EnemyMaster, opponent.master, false);
            foreach (var servant in ui.SelfFieldList)
            {
                setServant(servant, getCard(servant), false);
            }
            foreach (var servant in ui.EnemyFieldList)
            {
                setServant(servant, getCard(servant), false);
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
        void onDiscover()
        {

        }
        [SerializeField]
        BJSYGameCore.Timer _tipTimer = new BJSYGameCore.Timer();
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
        void onTurnEndButtonClick()
        {
            resetUse(true, true);
            player.cmdTurnEnd(game);
        }
        #endregion
        #region Animation
        Dictionary<Type, ConstructorInfo> animConstructorDic { get; } = new Dictionary<Type, ConstructorInfo>();
        /// <summary>
        /// 加载动画
        /// </summary>
        /// <param name="assembly"></param>
        public void loadAnim(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface)
                    continue;
                Type baseType = type.BaseType;
                if (!baseType.IsGenericType)
                    continue;
                if (baseType.GetGenericTypeDefinition() != typeof(UIAnimation<>) &&
                    baseType.GetGenericTypeDefinition() != typeof(EventAnimation<>) &&
                    baseType.GetGenericTypeDefinition() != typeof(RequestAnimation<>))
                    continue;
                Type paraType = baseType.GetGenericArguments()[0];
                if (!paraType.IsSubclassOf(typeof(EventArg)) &&
                    !paraType.IsSubclassOf(typeof(Request)))
                    continue;
                setAnim(paraType, type);
            }
        }
        /// <summary>
        /// 设置动画
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="animType"></param>
        public void setAnim(Type eventType, Type animType)
        {
            foreach (ConstructorInfo constructor in animType.GetConstructors())
            {
                var args = constructor.GetParameters();
                if (args.Length < 1 || (args.Length == 1 && args[0].ParameterType == eventType))
                {
                    if (animConstructorDic.ContainsKey(eventType))
                    {
                        UberDebug.LogWarningChannel("UI", "存在冲突的动画" + animType);
                        break;
                    }
                    animConstructorDic.Add(eventType, constructor);
                    break;
                }
            }
        }
        public void setEventAnim<TEvent, TAnim>() where TEvent : IEventArg where TAnim : UIAnimation
        {
            setAnim(typeof(TEvent), typeof(TAnim));
        }
        public void setRequestAnim<TRequest, TAnim>() where TRequest : IRequest where TAnim : UIAnimation
        {
            setAnim(typeof(TRequest), typeof(TAnim));
        }
        /// <summary>
        /// 获取事件对应的动画
        /// </summary>
        /// <param name="eventArg"></param>
        /// <returns></returns>
        public UIAnimation getEventAnim(IEventArg eventArg)
        {
            Type type = eventArg.GetType();
            if (animConstructorDic.ContainsKey(type))
            {
                UIAnimation anim = null;
                if (animConstructorDic[type].GetParameters().Length == 0)
                    anim = animConstructorDic[type].Invoke(new object[0]) as UIAnimation;
                else if (animConstructorDic[type].GetParameters().Length == 1)
                    anim = animConstructorDic[type].Invoke(new object[] { eventArg }) as UIAnimation;
                if (anim is EventAnimation tAnim)
                {
                    tAnim.eventArg = eventArg;
                    tAnim.init(this);
                }
                return anim;
            }
            else
                return null;
        }
        public RequestAnimation getRequestAnim(IRequest request)
        {
            Type type = request.GetType();
            if (animConstructorDic.ContainsKey(type))
            {
                RequestAnimation anim = null;
                if (animConstructorDic[type].GetParameters().Length == 0)
                {
                    anim = animConstructorDic[type].Invoke(new object[0]) as RequestAnimation;
                    anim.request = request;
                }
                return anim;
            }
            else
                return null;
        }
        [SerializeField]
        List<UIAnimation> _animationQueue = new List<UIAnimation>();
        public void addAnim(UIAnimation anim)
        {
            _animationQueue.Add(anim);
        }
        public UIAnimation[] getAnimQueue()
        {
            return _animationQueue.ToArray();
        }
        public void updateAnim()
        {
            if (_animationQueue.Count > 0)
            {
                for (int i = 0; i < _animationQueue.Count; i++)
                {
                    UIAnimation anim = _animationQueue[i];
                    bool isBlocked = false;
                    if (i == 0)
                    {
                        //第一个永远不被阻挡
                    }
                    else
                    {
                        for (int j = 0; j < i; j++)
                        {
                            UIAnimation prevAnim = _animationQueue[j];
                            if (prevAnim.blockAnim(anim))
                            {
                                isBlocked = true;
                                break;
                            }
                        }
                    }
                    if (isBlocked)
                        continue;
                    try
                    {
                        if (anim is TableAnimation tAnim ? tAnim.update(this) : anim.update(ui))
                        {
                            _animationQueue.RemoveAt(i);
                            i--;
                        }
                    }
                    catch (Exception e)
                    {
                        _animationQueue.RemoveAt(i);
                        i--;
                        Debug.LogError("动画" + anim + "播放异常被跳过：" + e);
                    }
                }
            }
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