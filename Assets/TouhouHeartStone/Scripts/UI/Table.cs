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
    partial class Table
    {
        public THHGame game { get; private set; } = null;
        public THHPlayer player { get; private set; } = null;
        Dictionary<Type, ConstructorInfo> animConstructorDic { get; } = new Dictionary<Type, ConstructorInfo>();
        partial void onAwake()
        {
            initAnim();

            SelfHandList.asButton.onClick.AddListener(() =>
            {
                if (SelfHandList.isExpanded)
                    SelfHandList.shrink();
                else
                    SelfHandList.expand();
            });
        }

        public void initAnim()
        {
            animConstructorDic.Clear();
            foreach (Type type in typeof(Animation).Assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface)
                    continue;
                Type baseType = type.BaseType;
                if (!baseType.IsGenericType)
                    continue;
                if (baseType.GetGenericTypeDefinition() != typeof(Animation<>))
                    continue;
                Type paraType = baseType.GetGenericArguments()[0];
                if (!paraType.IsSubclassOf(typeof(EventArg)))
                    continue;
                foreach (ConstructorInfo constructor in type.GetConstructors())
                {
                    var args = constructor.GetParameters();
                    if (args.Length < 1 || (args.Length == 1 && args[0].ParameterType == paraType))
                    {
                        animConstructorDic.Add(paraType, constructor);
                        break;
                    }
                }
            }
        }
        public Animation getAnim(EventArg eventArg)
        {
            Type type = eventArg.GetType();
            if (animConstructorDic.ContainsKey(type))
            {
                if (animConstructorDic[type].GetParameters().Length == 0)
                    return animConstructorDic[type].Invoke(new object[0]) as Animation;
                else if (animConstructorDic[type].GetParameters().Length == 1)
                    return animConstructorDic[type].Invoke(new object[] { eventArg }) as Animation;
                return null;
            }
            else
                return null;
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
            _animationQueue.Clear();

            if (game != null)
            {
                game.triggers.onEventBefore -= onEventBefore;
                game.triggers.onEventAfter -= onEventAfter;
            }
            this.game = game;
            if (game != null)
            {
                game.triggers.onEventBefore += onEventBefore;
                game.triggers.onEventAfter += onEventAfter;
            }
            if (player != null)
            {
                SelfSkill.asButton.onClick.RemoveAllListeners();
                TurnEndButton.onClick.RemoveAllListeners();
            }
            this.player = player;
            if (player != null)
            {
                SelfSkill.asButton.onClick.AddListener(() =>
                {
                    if (selectableTargets != null)
                        return;
                    player.cmdUse(game, SelfSkill.card, 0);
                });
                TurnEndButton.onClick.AddListener(() =>
                {
                    player.cmdTurnEnd(game);

                    //SelfHandList.stopPlacing(true);
                    resetUse(true, true);
                    selectableTargets = null;
                });
            }
        }
        [SerializeField]
        List<Animation> _animationQueue = new List<Animation>();
        private void onEventBefore(IEventArg arg)
        {
            switch (arg)
            {
                case THHGame.InitEventArg init:
                    _animationQueue.Add(new InitAnimation(init));
                    break;
                case THHPlayer.InitReplaceEventArg initReplace:
                    _animationQueue.Add(new InitReplaceAnimation(initReplace));
                    break;
                case THHGame.StartEventArg start:
                    _animationQueue.Add(new StartAnimation(start));
                    break;
                case THHGame.TurnStartEventArg turnStart:
                    _animationQueue.Add(new TurnStartAnimation(turnStart));
                    break;
                case THHPlayer.DrawEventArg draw:
                    _animationQueue.Add(new DrawAnimation(draw));
                    break;
                case THHPlayer.SetGemEventArg setGem:
                    _animationQueue.Add(new SetGemAnimation(setGem));
                    break;
                case THHPlayer.SetMaxGemEventArg setMaxGem:
                    _animationQueue.Add(new SetMaxGemAnimation(setMaxGem));
                    break;
                case THHPlayer.MoveEventArg move:
                    _animationQueue.Add(new MoveServantAnimation(move));
                    break;
                case THHPlayer.UseEventArg use:
                    if (use.card.define.type == CardDefineType.SERVANT)
                        _animationQueue.Add(new UseServantAnimation(use));
                    else if (use.card.define.type == CardDefineType.SPELL)
                        _animationQueue.Add(new UseSpellAnimation(use));
                    break;
                case THHCard.HealEventArg heal:
                    _animationQueue.Add(new HealAnimation(heal));
                    break;
                //case THHPlayer.CreateTokenEventArg createToken:
                //    _animationQueue.Add(new CreateTokenAnimation(createToken));
                //    break;
                case THHCard.AttackEventArg attack:
                    _animationQueue.Add(new ServantAttackAnimation(attack));
                    break;
                case THHCard.DamageEventArg damage:
                    _animationQueue.Add(new DamageAnimation(damage));
                    break;
                case THHCard.DeathEventArg death:
                    _animationQueue.Add(new DeathAnimation(death));
                    break;
                case THHPlayer.ActiveEventArg active:
                    foreach (var target in active.targets)
                    {
                        if (target is TouhouCardEngine.Card card)
                        {
                            _animationQueue.Add(new SelectTargetAnimation(active));
                        }
                    }
                    break;
                case THHGame.TurnEndEventArg turnEnd:
                    _animationQueue.Add(new TurnEndAnimation(turnEnd));
                    break;
                case THHGame.GameEndEventArg gameEnd:
                    _animationQueue.Add(new GameEndAnimation(gameEnd));
                    break;
                default:
                    game.logger?.log("UI", "没有动画的事件：" + arg);
                    break;
            }
        }
        [SerializeField]
        bool _canControl = true;
        public bool canControl
        {
            get { return _canControl; }
            set { _canControl = value; }
        }
        [SerializeField]
        Projectile _defaultProjectile;
        private void onEventAfter(IEventArg arg)
        {

        }
        protected void Update()
        {
            if (_tipTimer.isExpired())
            {
                _tipTimer.reset();
                TipText.gameObject.SetActive(false);
            }
            else if (_tipTimer.isStarted)
            {
                TipText.color = new Color(TipText.color.r, TipText.color.g, TipText.color.b, 1/*_tipTimer.getRemainedTime() / _tipTimer.duration*/);
            }

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
            if (player.skill != null)
            {
                SelfSkill.update(this, player, player, player.skill, getSkin(player.skill));
                SelfSkill.display();
            }
            else
                SelfSkill.hide();
            SelfGem.Text.text = player.gem.ToString();
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
            if (opponent.skill != null)
            {
                EnemySkill.update(this, player, opponent, opponent.skill, getSkin(opponent.skill));
                EnemySkill.display();
            }
            else
                EnemySkill.hide();
            EnemyGem.Text.text = opponent.gem.ToString();

            IRequest request = game.answers.getLastRequest(player.id);
            if (request is InitReplaceRequest initReplace)
            {
                if (!InitReplaceDialog.isDisplaying)
                {
                    InitReplaceDialog.display();
                    InitReplaceDialog.InitReplaceCardList.clearItems();
                    InitReplaceDialog.InitReplaceCardList.updateItems(player.init, (i, c) => i.Card.card == c, (item, card) =>
                    {
                        item.Card.update(card, getSkin(card));
                        item.MarkImage.enabled = false;
                        item.asButton.onClick.RemoveAllListeners();
                        item.asButton.onClick.AddListener(() =>
                        {
                            item.MarkImage.enabled = !item.MarkImage.enabled;
                        });
                    });
                    InitReplaceDialog.InitReplaceCardList.sortItems((a, b) => player.init.indexOf(a.Card.card) - player.init.indexOf(b.Card.card));
                    InitReplaceDialog.ConfirmButton.interactable = true;
                    InitReplaceDialog.ConfirmButton.GetComponent<Image>().color = Color.white;
                    InitReplaceDialog.ConfirmButton.onClick.RemoveAllListeners();
                    InitReplaceDialog.ConfirmButton.onClick.AddListener(() =>
                    {
                        game.answers.answer(player.id, new InitReplaceResponse()
                        {
                            cardsId = InitReplaceDialog.InitReplaceCardList.Where(item => item.MarkImage.enabled).Select(item => item.Card.card.id).ToArray()
                        });
                        InitReplaceDialog.ConfirmButton.interactable = false;
                        InitReplaceDialog.ConfirmButton.GetComponent<Image>().color = Color.gray;
                    });
                }
            }
            else
            {
                InitReplaceDialog.hide();
            }

            if (_animationQueue.Count > 0)
            {
                for (int i = 0; i < _animationQueue.Count; i++)
                {
                    Animation anim = _animationQueue[i];
                    bool isBlocked = i != 0;//第一个永远不被阻挡
                    for (int j = 0; j < i; j++)
                    {
                        Animation prevAnim = _animationQueue[j];
                        if (prevAnim.blockAnim(anim))
                        {
                            isBlocked = true;
                            break;
                        }
                    }
                    if (isBlocked)
                        continue;
                    if (anim.update(this))
                    {
                        _animationQueue.RemoveAt(i);
                        i--;
                    }
                }
            }
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
            return parent.game.getManager<CardManager>().GetCardSkin(define.id);
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
        [SerializeField]
        UIObject[] _selectableTargets = null;
        public UIObject[] selectableTargets
        {
            get { return _selectableTargets; }
            set { _selectableTargets = value; }
        }
    }
}
