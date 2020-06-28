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
        [SerializeField]
        bool _canControl = true;
        public bool canControl
        {
            get { return _canControl; }
            set { _canControl = value; }
        }
        public TouhouCardEngine.Card usingCard { get; set; } = null;
        [SerializeField]
        int _usingPosition;
        public int usingPosition
        {
            get { return _usingPosition; }
            set { _usingPosition = value; }
        }
        [SerializeField]
        bool _isSelectingTarget = false;
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
        }
        public void setGame(THHGame game, THHPlayer player)
        {
            ui.InitReplaceDialog.hide();
            ui.TurnTipImage.hide();
            ui.SelfHandList.clearItems();
            ui.SelfFieldList.clearItems();
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
            if (player != null)
            {
                ui.SelfSkill.asButton.onClick.RemoveAllListeners();
                ui.TurnEndButton.onClick.RemoveAllListeners();
            }
            this.player = player;
            //if (player != null)
            //{
            //    table.SelfSkill.asButton.onClick.AddListener(() =>
            //    {
            //        if (selectableTargets != null)
            //            return;
            //        player.cmdUse(game, SelfSkill.card, 0);
            //    });
            //    table.TurnEndButton.onClick.AddListener(() =>
            //    {
            //        player.cmdTurnEnd(game);

            //        //SelfHandList.stopPlacing(true);
            //        resetUse(true, true);
            //        selectableTargets = null;
            //    });
            //}
        }

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

        private void onEventAfter(IEventArg arg)
        {

        }
        #region UI
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
        public void setMaster(Master master, TouhouCardEngine.Card card, bool isSelectable = false)
        {
            CardSkinData skin = getSkin(card);
            master.Image.sprite = skin.image;
            master.HpText.text = card.getCurrentLife().ToString();
            //if (card.getCurrentLife() == card.getLife())
            //    HpText.color = Color.white;
            //else
            //    HpText.color = Color.red;
            if (card.getAttack() > 0)
            {
                master.AttackText.text = card.getAttack().ToString();
                master.AttackText.display();
                master.AtkImage.display();
            }
            else
            {
                master.AttackText.hide();
                master.AtkImage.hide();
            }
            if (card.getArmor() > 0)
            {
                master.ArmorText.text = card.getArmor().ToString();
                master.ArmorText.display();
                master.ArmorImage.display();
            }
            else
            {
                master.ArmorText.hide();
                master.ArmorImage.hide();
            }

            if (isSelectable)
                master.HighlightController = Master.Highlight.Yellow;
            else if (card.getOwner() == player && game.currentPlayer == player && card.canAttack(game))
                master.HighlightController = Master.Highlight.Green;
            else
                master.HighlightController = Master.Highlight.None;
        }
        public void setSkill(Skill skill, TouhouCardEngine.Card card)
        {
            CardSkinData skin = getSkin(card);
            skill.Image.sprite = skin.image;
            skill.CostText.text = card.getCost().ToString();
            //if (card.isUsed())
            //{
            //    skill.IsUsedController = Skill.IsUsed.True;
            //}
            //else
            //    skill.IsUsedController = Skill.IsUsed.False;
            //if (player == this.player
            //    && card.isUsable(game, player, out _)//技能是可用的
            //    && table.selectableTargets == null//没有在选择目标
            //    && table.canControl//是自己的回合
            //    )
            //{
            //    skill.IsUsableController = Skill.IsUsable.True;
            //    skill.asButton.interactable = true;
            //}
            //else
            //{
            //    skill.IsUsableController = Skill.IsUsable.False;
            //    skill.asButton.interactable = false;
            //}
        }
        Dictionary<TouhouCardEngine.Card, HandListItem> cardHandDic { get; } = new Dictionary<TouhouCardEngine.Card, HandListItem>();
        public HandListItem createHand(TouhouCardEngine.Card card)
        {
            if (cardHandDic.ContainsKey(card))
            {
                UberDebug.LogErrorChannel("UI", "手牌中已经存在" + card + "对应UI" + cardHandDic[card]);
                return cardHandDic[card];
            }
            UberDebug.LogChannel("UI", "创建手牌UI：" + card);
            HandListItem item;
            bool isDragable;
            if (card.getOwner() == player)
            {
                item = ui.SelfHandList.addItem();
                isDragable = true;
                item.onDrag -= onDrag;
                item.onEndDrag -= onEndDrag;
                item.onDrag += onDrag;
                item.onEndDrag += onEndDrag;
                setCard(item.Card, card, true);
            }
            else
            {
                item = ui.EnemyHandList.addItem();
                isDragable = false;
                item.onDrag -= onDrag;
                item.onEndDrag -= onEndDrag;
                setCard(item.Card, card, false);
            }
            void onDrag(HandListItem hand, PointerEventData pointer)
            {
                if (!isDragable)
                    return;
                if (!canControl)//无法进行控制
                {
                    resetUse(true, true);
                    return;
                }
                usingCard = getCard(item);
                //拖拽卡片
                item.Card.rectTransform.position = pointer.position;
                if (ui.SelfHandList.rectTransform.rect.Contains(ui.SelfHandList.rectTransform.InverseTransformPoint(pointer.position)))
                {
                    //如果移动回手牌区域，恢复正常大小
                    item.Card.rectTransform.localScale = Vector3.one;
                    //移除随从占位
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
                        item.Card.rectTransform.localScale = Vector3.one * .4f / ui.SelfHandList.rectTransform.localScale.y;
                        if (usingCard.define is ServantCardDefine)
                        {
                            ui.SelfFieldList.defaultItem.rectTransform.SetAsFirstSibling();
                            ui.ServantPlaceHolder.rectTransform.sizeDelta = ui.SelfFieldList.defaultItem.rectTransform.sizeDelta;
                            ui.SelfFieldList.addChild(ui.ServantPlaceHolder.rectTransform);
                            ui.ServantPlaceHolder.display();
                            var servants = ui.SelfFieldList.getItems();
                            int index = 0;
                            if (servants.Length > 0)
                            {
                                //需要选择空位，计算空位
                                for (int i = 0; i < servants.Length; i++)
                                {
                                    if (servants[i].rectTransform.position.x < pointer.position.x)
                                        index = i + 1;
                                }
                                ui.ServantPlaceHolder.rectTransform.SetSiblingIndex(index + 1);
                            }
                        }
                    }
                }
            }
            void onEndDrag(HandListItem hand, PointerEventData pointer)
            {
                if (item.GetComponentInParent<HandList>() != ui.SelfHandList)//你不能用别人的卡
                    return;
                if (!canControl)//不是你的回合，不生效
                {
                    resetUse(true, true);
                    return;
                }
                usingCard = getCard(item);
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
                        var servants = ui.SelfFieldList.getItems();
                        int index = 0;
                        if (servants.Length > 0)
                        {
                            //需要选择空位，计算空位
                            for (int i = 0; i < servants.Length; i++)
                            {
                                if (servants[i].rectTransform.position.x < pointer.position.x)
                                    index = i + 1;
                            }
                            ui.ServantPlaceHolder.rectTransform.SetSiblingIndex(index + 1);
                        }
                        if (usingCard.getAvaliableTargets(game) is TouhouCardEngine.Card[] targets && targets.Length > 0)
                        {
                            usingPosition = index;
                            //进入选择目标状态，固定手牌到占位上，高亮可以选择的目标
                            item.Card.hide();
                            //显示占位随从
                            ui.ServantPlaceHolder.Servant.display();
                            setServant(ui.ServantPlaceHolder.Servant, usingCard.define);
                            isSelectingTarget = true;
                            foreach (var target in targets)
                            {
                                if (tryGetMaster(target, out var master))
                                {
                                    setMaster(master, target, true);
                                    master.onClick.add(onSelectMaster);
                                }
                                else if (tryGetServant(target, out var servant))
                                {
                                    setServant(servant, target, true);
                                    servant.onClick.add(onSelectServant);
                                }
                            }
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
                        if (usingCard.getAvaliableTargets(game) is TouhouCardEngine.Card[] targets && targets.Length > 0)
                        {
                            //进入选择目标状态，高亮可以选择的目标
                            item.Card.hide();
                            isSelectingTarget = true;
                            foreach (var target in targets)
                            {
                                if (tryGetMaster(target, out var master))
                                {
                                    setMaster(master, target, true);
                                    master.onClick.add(onSelectMaster);
                                }
                                else if (tryGetServant(target, out var servant))
                                {
                                    setServant(servant, target, true);
                                    servant.onClick.add(onSelectServant);
                                }
                            }
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
            }
            cardHandDic.Add(card, item);
            return item;
        }
        void onSelectMaster(Master master, PointerEventData pointer)
        {
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
        void onSelectServant(Servant servant, PointerEventData pointer)
        {
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
        private void resetUse(bool resetItem, bool resetPlaceHolder)
        {
            if (usingCard != null && resetItem)
            {
                HandListItem usingHand = getHand(usingCard);
                usingHand.Card.display();
                usingHand.Card.rectTransform.localScale = Vector3.one;
                usingHand.Card.rectTransform.localPosition = Vector2.zero;
            }
            if (resetPlaceHolder)
                hideServantPlaceHolder();
            isSelectingTarget = false;
            //selectableTargets = null;
        }
        public HandListItem getHand(TouhouCardEngine.Card card)
        {
            if (cardHandDic.ContainsKey(card))
                return cardHandDic[card];
            throw new ActorNotFoundException(card);
        }
        public TouhouCardEngine.Card getCard(HandListItem item)
        {
            foreach (var pair in cardHandDic)
            {
                if (pair.Value == item)
                    return pair.Key;
            }
            return null;
        }
        public TouhouCardEngine.Card getCard(Master master)
        {
            if (master == ui.SelfMaster)
                return player.master;
            else
                return game.getOpponent(player).master;
        }
        public void setCard(UI.Card ui, TouhouCardEngine.Card card, bool isFaceup)
        {
            ui.CostPropNumber.asText.text = card.getCost().ToString();
            if (card.define.type == CardDefineType.SERVANT)
            {
                ui.TypeController = UI.Card.Type.Servant;
                ui.AttackPropNumber.asText.text = card.getAttack().ToString();
                ui.LifePropNumber.asText.text = card.getLife().ToString();
            }
            else
            {
                ui.TypeController = UI.Card.Type.Spell;
            }

            if (isFaceup)
            {
                CardSkinData skin = getSkin(card);
                ui.Image.sprite = skin.image;
                ui.NameText.text = skin.name;
                ui.DescText.text = skin.desc;
                ui.IsFaceupController = UI.Card.IsFaceup.True;
            }
            else
                ui.IsFaceupController = UI.Card.IsFaceup.False;
        }
        Dictionary<TouhouCardEngine.Card, Servant> cardServantDic { get; } = new Dictionary<TouhouCardEngine.Card, Servant>();
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
            servant.rectTransform.SetSiblingIndex(position + 1);
            setServant(servant, card);
            cardServantDic.Add(card, servant);
            return servant;
        }
        public Servant getServant(TouhouCardEngine.Card card)
        {
            if (cardServantDic.ContainsKey(card))
                return cardServantDic[card];
            throw new ActorNotFoundException(card);
        }
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
        public void setServant(Servant servant, TouhouCardEngine.Card card, bool isSelectable = false)
        {
            CardSkinData skin = getSkin(card);
            if (skin != null)
            {
                servant.Image.sprite = skin.image;
            }
            servant.AttackText.text = card.getAttack().ToString();
            servant.HpText.text = card.getCurrentLife().ToString();

            servant.onDrag.remove(onDragServant);
            if (isSelectable)
                servant.HighlightController = Servant.Highlight.Yellow;
            else if (player == card.getOwner() && game.currentPlayer == player && card.canAttack(game))
            {
                servant.HighlightController = Servant.Highlight.Green;
                servant.onDrag.add(onDragServant);
                servant.onDragEnd.add(onDragEndServant);
            }
            else
                servant.HighlightController = Servant.Highlight.None;
            //getChild("Root").getChild("Taunt").gameObject.SetActive(card.isTaunt());
            //getChild("Root").getChild("Shield").gameObject.SetActive(card.isShield());
        }
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
                servant.rectTransform.localScale = Vector3.one * 1.1f;
                //显示指针
                ui.AttackArrowImage.display();
                ui.AttackArrowImage.rectTransform.position = servant.rectTransform.position;
                //移动指针
                ui.AttackArrowImage.rectTransform.eulerAngles = new Vector3(0, 0,
                    Vector2.Angle(servant.rectTransform.position, pointer.position));
                ui.AttackArrowImage.rectTransform.up = ((Vector3)pointer.position - servant.rectTransform.position).normalized;
                ui.AttackArrowImage.rectTransform.sizeDelta = new Vector2(
                    ui.AttackArrowImage.rectTransform.sizeDelta.x,
                    Vector2.Distance(servant.rectTransform.position, pointer.position) / ui.getCanvas().transform.localScale.y);
                //高亮标记所有目标
                highlightTargets(game.findAllCardsInField(c => card.isAttackable(game, player, c, out _)));
            }
            else
            {
                cancelAttack();
            }
        }
        /// <summary>
        /// 高亮所有目标
        /// </summary>
        /// <param name="targets"></param>
        void highlightTargets(TouhouCardEngine.Card[] targets)
        {
            foreach (var target in targets)
            {
                if (tryGetMaster(target, out var targetMaster))
                {
                    setMaster(targetMaster, target, true);
                }
                else if (tryGetServant(target, out var targetServant))
                {
                    setServant(targetServant, target, true);
                }
            }
        }
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
            if (pointer.pointerCurrentRaycast.gameObject != null)
            {
                if (pointer.pointerCurrentRaycast.gameObject.GetComponentInParent<Servant>() is Servant targetServant)
                {
                    if (card.isAttackable(game, player, getCard(targetServant), out var tip))
                    {
                        player.cmdAttack(game, card, getCard(targetServant));
                    }
                    else
                        ui.showTip(tip);
                }
                else if (pointer.pointerCurrentRaycast.gameObject.GetComponentInParent<Master>() is Master targetMaster)
                {
                    if (card.isAttackable(game, player, getCard(targetMaster), out var tip))
                    {
                        player.cmdAttack(game, card, getCard(targetMaster));
                    }
                    else
                        ui.showTip(tip);
                }
            }
            //取消选中和攻击
            cancelAttack();
        }
        private void cancelAttack()
        {
            //rectTransform.localScale = Vector3.one;
            ui.AttackArrowImage.hide();
            removeHighlights();
        }
        public void setServant(Servant servant, CardDefine card)
        {
            CardSkinData skin = getSkin(card);
            if (skin != null)
            {
                servant.Image.sprite = skin.image;
            }
            servant.AttackText.text = card.getAttack().ToString();
            servant.HpText.text = card.getLife().ToString();
        }
        public TouhouCardEngine.Card getCard(Servant servant)
        {
            foreach (var pair in cardServantDic)
            {
                if (pair.Value == servant)
                    return pair.Key;
            }
            return null;
        }
        private void hideServantPlaceHolder()
        {
            ui.addChild(ui.ServantPlaceHolder.rectTransform);
            ui.ServantPlaceHolder.Servant.hide();
            ui.ServantPlaceHolder.hide();
        }
        [SerializeField]
        BJSYGameCore.Timer _tipTimer = new BJSYGameCore.Timer();
        public void showTip(string tip)
        {
            ui.TipText.gameObject.SetActive(true);
            ui.TipText.text = tip;
            _tipTimer.start();
        }
        #endregion
        #region Animation
        Dictionary<Type, ConstructorInfo> animConstructorDic { get; } = new Dictionary<Type, ConstructorInfo>();
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
                    if (anim is TableAnimation tAnim ? tAnim.update(this) : anim.update(ui))
                    {
                        _animationQueue.RemoveAt(i);
                        i--;
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
                throw new SkinNotFoundException("没有找到" + card + "对应的皮肤");
        }
        public CardSkinData getSkin(CardDefine define)
        {
            CardSkinData skin = getManager<CardManager>().getSkin(define.id);
            if (skin != null)
                return skin;
            else
                throw new SkinNotFoundException("没有找到" + define + "对应的皮肤");
        }
        #endregion
    }
}