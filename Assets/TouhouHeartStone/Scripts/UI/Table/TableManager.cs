using System;
using System.Linq;
using BJSYGameCore;
using BJSYGameCore.UI;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Card = TouhouCardEngine.Card;
using UICard = UI.Card;
using System.Threading.Tasks;

namespace Game
{
    public partial class TableManager : Manager
    {
        #region 公有方法
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

            // 敌方牌库。
            ui.EnemyDeck.onPointerEnter.set(onDeckPointerEnter);
            ui.EnemyDeck.onPointerExit.set(onDeckPointerExit);
            ui.EnemyDeck.onPointerMove.set(onDeckPointerMove);

            // 敌方墓地。
            ui.EnemyGraveDeck.onPointerEnter.set(onGravePointerEnter);
            ui.EnemyGraveDeck.onPointerExit.set(onGravePointerExit);
            ui.EnemyGraveDeck.onPointerMove.set(onDeckPointerMove);

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

        #region 皮肤
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
                    image = getManager<CardManager>().getDefaultSprite(),
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
                    image = getManager<CardManager>().getDefaultSprite(),
                    name = define.GetType().Name
                };
            }
        }
        #endregion
        #endregion 
        #region 私有方法
        #region 生命周期
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
                Card hoveringCard = null;
                if (_servant != null)
                    hoveringCard = getCard(_servant);
                else if (_hoveredItem != null)
                    hoveringCard = _hoveredItem == ui.SelfItem ? player.getItem() : game.getOpponent(player).getItem();

                if (hoveringCard != null)
                {
                    displayLargeCard(false, hoveringCard);
                }
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
        #endregion 生命周期
        #region 事件回调
        void onRequestHandler(IRequest request)
        {
            eventCache.cache(onRequest, request);
        }
        void onResponseHandler(IResponse response)
        {
            eventCache.cache(onResponse, response);
        }
        void onEventBeforeHandler(IEventArg arg)
        {
            eventCache.cache(onEventBefore, arg);
        }
        private void onEventAfterHandler(IEventArg arg)
        {
            eventCache.cache(onEventAfter, arg);
        }
        #endregion

        #region 事件处理

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
        /// <summary>
        /// 事件后处理
        /// </summary>
        /// <param name="arg"></param>
        private void onEventAfter(IEventArg arg)
        {

        }
        #endregion
        #endregion 私有方法

        #region 属性字段

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
        UICard _cardPrefab;
        public UICard cardPrefab => _cardPrefab;
        [SerializeField]
        Vector2 _tipBoxOffset = new Vector2(20, 20);
        [Header("State")]
        [SerializeField]
        Table _table;
        [SerializeField]
        bool _canControl = true;
        [SerializeField]
        int _usingPosition = -1;
        [SerializeField]
        bool _isSelectingTarget = false;
        public Table ui
        {
            get { return _table; }
            set { _table = value; }
        }
        public CardEngine game { get; private set; } = null;
        public Player player { get; private set; } = null;
        ActionEventCache eventCache { get; } = new ActionEventCache();

        LangManager lang => getManager<LangManager>();

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
        public int usingPosition
        {
            get { return _usingPosition; }
            set { _usingPosition = value; }
        }
        /// <summary>
        /// 是否进入目标选择模式
        /// </summary>
        public bool isSelectingTarget
        {
            get { return _isSelectingTarget; }
            set { _isSelectingTarget = value; }
        }
        public Card attackingCard { get; set; } = null;

        Card _selfItemCard;
        Card _enemyItemCard;
        #endregion
    }
}