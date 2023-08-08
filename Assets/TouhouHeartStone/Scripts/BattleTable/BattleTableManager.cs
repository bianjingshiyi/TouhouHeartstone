using System;
using System.Linq;
using BJSYGameCore;
using Boo.Lang;
using Game.BattleTable.UI;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using Card = TouhouCardEngine.Card;

namespace Game.BattleTable
{
    public partial class BattleTableManager : MonoBehaviour
    {
        /// <summary>
        /// 设置游戏对象和玩家并初始化UI
        /// </summary>
        /// <param name="game"></param>
        /// <param name="player"></param>
        public void setGame(MainManager main, CardEngine game, Player player)
        {
            _main = main;

            reset();

            initMaster(table.SelfMaster);
            initMaster(table.EnemyMaster);
            this.player = player;
            if (this.game != null)
            {
                this.game.triggers.onEventBefore -= onEventBeforeHandler;
                this.game.triggers.onEventAfter -= onEventAfterHandler;
                this.game.answers.onRequest -= onRequestHandler;
                this.game.answers.onResponse -= onResponseHandler;
            }
            this.game = game;
            if (game != null)
            {
                game.triggers.onEventBefore += onEventBeforeHandler;
                game.triggers.onEventAfter += onEventAfterHandler;
                game.answers.onRequest += onRequestHandler;
                game.answers.onResponse += onResponseHandler;

                setMaster(table.SelfMaster, newSnapshot(player.getMaster()));
                setSkill(table.SelfSkill, newSnapshot(player.getSkill()));
                var oppo = game.getOpponent(player);
                setMaster(table.EnemyMaster, newSnapshot(oppo.getMaster()));
                setSkill(table.EnemySkill, newSnapshot(oppo.getSkill()));
            }
        }
        public void showTurnTipImage(bool friendly)
        {
            var turnTipImage = ui.TurnTipImage;
            turnTipImage.setFriendly(friendly);
            turnTipImage.play();
        }
        public void hideTurnTipImage()
        {
            ui.TurnTipImage.setActive(false);
        }
        public void showGameResult(bool result)
        {
            ui.GameResultDialog.ShowGameResult(result);
        }
        public void setOpponentWaiting(bool waiting)
        {
            ui.OpponentWaitingObj.SetActive(waiting);
        }
        #region 皮肤
        public CardSkinData getSkin(ICardData card)
        {
            return getSkin(card.define);
        }
        public CardSkinData getSkin(CardDescriptor desc)
        {
            return getSkin(desc.define);
        }
        public CardSkinData getSkin(CardDefine define)
        {
            CardSkinData skin = cardManager.getCardSkin(define);
            if (skin != null)
                return skin;
            else
            {
                game.logger.logError("没有找到" + define + "对应的皮肤");
                return new CardSkinData()
                {
                    id = define.id,
                    desc = define.ToString() + "（这张卡随时可能爆炸！）",
                    image = cardManager.getDefaultSprite(),
                    name = define.GetType().Name
                };
            }
        }
        #endregion
        #region 私有方法
        #region 生命周期
        private void Awake()
        {
            table.onClickNoWhere += onClickNoWhere;

            table.TurnEndButton.onClick += onTurnEndClickCallback;
            table.TurnEndButton.onPointerEnter += onTurnEndPointerEnterCallback;
            table.TurnEndButton.onPointerExit += onTurnEndPointerExitCallback;

            table.SelfField.onServantClick += onServantClickCallback;
            table.SelfField.onServantBeginDrag += onServantBeginDragCallback;
            table.SelfField.onServantDrag += onServantDragCallback;
            table.SelfField.onServantEndDrag += onServantEndDragCallback;
            table.SelfField.onServantPointerEnter += onServantPointerEnterCallback;
            table.SelfField.onServantPointerExit += onServantPointerExitCallback;

            table.EnemyField.onServantClick += onServantClickCallback;
            table.EnemyField.onServantBeginDrag += onServantBeginDragCallback;
            table.EnemyField.onServantDrag += onServantDragCallback;
            table.EnemyField.onServantEndDrag += onServantEndDragCallback;
            table.EnemyField.onServantPointerEnter += onServantPointerEnterCallback;
            table.EnemyField.onServantPointerExit += onServantPointerExitCallback;

            table.SelfMaster.onClick += onMasterClickCallback;
            table.SelfMaster.onBeginDrag += onMasterBeginDragCallback;
            table.SelfMaster.onDrag += onMasterDragCallback;
            table.SelfMaster.onEndDrag += onMasterEndDragCallback;

            table.EnemyMaster.onClick += onMasterClickCallback;
            table.EnemyMaster.onBeginDrag += onMasterBeginDragCallback;
            table.EnemyMaster.onDrag += onMasterDragCallback;
            table.EnemyMaster.onEndDrag += onMasterEndDragCallback;


            table.SelfSkill.onClick += onSkillClickCallback;
            table.SelfSkill.onBeginDrag += onSkillBeginDragCallback;
            table.SelfSkill.onDrag += onSkillDragCallback;
            table.SelfSkill.onEndDrag += onSkillEndDragCallback;
            table.SelfSkill.onPointerEnter += onSkillPointerEnterCallback;
            table.SelfSkill.onPointerExit += onSkillPointerExitCallback;
            table.EnemySkill.onPointerEnter += onSkillPointerEnterCallback;
            table.EnemySkill.onPointerExit += onSkillPointerExitCallback;


            table.SelfItem.onPointerEnter += onItemPointerEnterCallback;
            table.SelfItem.onPointerExit += onItemPointerExitCallback;
            table.EnemyItem.onPointerEnter += onItemPointerEnterCallback;
            table.EnemyItem.onPointerExit += onItemPointerExitCallback;


            table.SelfDeck.onClick += onDeckClickCallback;
            table.SelfDeck.onPointerEnter += onDeckPointerEnterCallback;
            table.SelfDeck.onPointerExit += onDeckPointerExitCallback;
            table.EnemyDeck.onPointerEnter += onDeckPointerEnterCallback;
            table.EnemyDeck.onPointerExit += onDeckPointerExitCallback;

            table.SelfGrave.onClick += onGraveClickCallback;
            table.SelfGrave.onPointerEnter += onGravePointerEnterCallback;
            table.SelfGrave.onPointerExit += onGravePointerExitCallback;
            table.EnemyGrave.onPointerEnter += onGravePointerEnterCallback;
            table.EnemyGrave.onPointerExit += onGravePointerExitCallback;

            table.SelfHandList.onBlockerClick += onHandListBlockerClickCallback;
            table.SelfHandList.onHandItemClick += onHandItemClickCallback;
            table.SelfHandList.onHandItemBeginDrag += onHandItemBeginDragCallback;
            table.SelfHandList.onHandItemDrag += onHandItemDragCallback;
            table.SelfHandList.onHandItemEndDrag += onHandItemEndDragCallback;
            table.SelfHandList.onHandItemPointerEnter += onHandItemPointerEnterCallback;
            table.SelfHandList.onHandItemPointerExit += onHandItemPointerExitCallback;
            table.EnemyHandList.onHandItemPointerEnter += onHandItemPointerEnterCallback;
            table.EnemyHandList.onHandItemPointerExit += onHandItemPointerExitCallback;

            table.Discover.onHideButtonClicked += onDiscoverHideButtonClick;
            table.Discover.onItemClicked += onCardChooseCallback;

            ui.InitReplaceDialog.onItemClicked += onMulliganItemClickCallback;
            ui.InitReplaceDialog.onAccept += onMulliganAcceptCallback;

            ui.PilePanel.onCloseButtonClicked += onClosePilePanelClickCallback;

            loadAnim(GetType().Assembly);
        }
        void Update()
        {
            table.update(Time.deltaTime);


            if (_showCardTimer.isExpired())
            {
                // 显示卡面
                Card hoveringCard = null;
                if (_servant != null)
                    hoveringCard = getCard(_servant);
                else if (_hoveredItem != null)
                    hoveringCard = _hoveredItem == table.SelfItem ? player.getItem() : game.getOpponent(player).getItem();

                if (hoveringCard != null)
                {
                    displayLargeCard(false, getCardDescriptor(hoveringCard));
                }
                _showCardTimer.reset();
            }

            if (_tipTimer.isExpired())
            {
                _tipTimer.reset();
                ui.TipText.gameObject.SetActive(false);
            }
            else if (_tipTimer.isStarted)
            {
                ui.TipText.color = new Color(1, 1, 1, 1/*_tipTimer.getRemainedTime() / _tipTimer.duration*/);
            }



            if (game == null)
                return;

            var clock = table.TurnEndButton;

            float longAngle = 0;
            float turn = game.getTurnCount();
            var timer = game.getTurnTimer();
            if (timer != null)
            {
                longAngle = timer.remainedTime / timer.time * 360;
            }
            clock?.setLongHandAngle(longAngle);
            clock?.setShortHandAngle(-turn * 3.6f);

            // 更新提示框位置
            Vector2 pointerPosition = EventSystem.current.currentInputModule.input.mousePosition;
            ui.TipBox.updatePos((Vector3)pointerPosition + _tipBoxOffset);

            //随从牌使用预览
            if (_servantPreviewAnim != null)
            {
                if (_servantPreviewAnim.update(this, Time.time))
                {
                    _servantPreviewAnim = null;
                    var hand = getHand(usingCard.id);
                    hand.card.setVisible(false);
                    //显示占位随从
                    try
                    {
                        if (tryGetServant(usingCard.id, out var servant))
                        {
                            setServant(servant, usingCard.define);
                        }
                    }
                    catch (NullReferenceException e)
                    {
                        Debug.LogError(e);
                    }
                }
            }
            //游戏事件处理
            eventCache.poll();
            if (_allControlDirty)
            {
                updateAllControlable();
                _allControlDirty = false;

                _controlDirtyCards.Clear();
            }
            else
            {
                foreach (var cardId in _controlDirtyCards)
                {
                    updateCardControlable(cardId);
                }
                _controlDirtyCards.Clear();
            }
            //动画
            queueAnims();
            updateAnim();
        }
        #endregion
        #region 事件回调
        #region 换牌
        private void onMulliganItemClickCallback(int index)
        {
            var initPile = player.getInit();
            var card = initPile[index];
            bool mulliganed = _mulliganedCards.Contains(card);
            if (mulliganed)
            {
                _mulliganedCards.Remove(card);
            }
            else
            {
                _mulliganedCards.Add(card);
            }
            ui.InitReplaceDialog.getItem(index).setMarkVisible(_mulliganedCards.Contains(card));
        }
        private void onMulliganAcceptCallback()
        {
            player.cmdInitReplace(game, _mulliganedCards.ToArray());
            ui.InitReplaceDialog.setWaiting(true);
        }
        #endregion
        #region 选牌
        private void onCardChooseCallback(int index)
        {
            if (_cardChooseMode == CardChooseMode.Card)
            {
                var cardId = _choosingCards[index];
                game.answers.answer(player.id, new DiscoverResponse()
                {
                    cardId = cardId
                });
                _choosingCards = null;
            }
            else if (_cardChooseMode == CardChooseMode.CardDefine)
            {
                var cardRef = _choosingCardDefineRefs[index];
                game.answers.answer(player.id, new DiscoverResponse()
                {
                    cardDefRef = cardRef
                });
                _choosingCardDefineRefs = null;
            }
            closeDiscoverDialog();
        }
        private void onDiscoverHideButtonClick()
        {
            table.Discover.isShowing = !table.Discover.isShowing;
            table.Discover.setHideButtonText(table.Discover.isShowing ? lang._(TAB_SHOW) : lang._(TAB_HIDE));
        }
        #endregion
        #region 牌堆查看
        private void onClosePilePanelClickCallback()
        {
            ui.PilePanel.setActive(false);
        }
        #endregion
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
            onEventBefore(arg);
        }
        private void onEventAfterHandler(IEventArg arg)
        {
            onEventAfter(arg);
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
            UIAnimation anim = getEventAnim(arg);
            if (anim != null)
                addAnim(anim);
            _allControlDirty = true;
        }
        /// <summary>
        /// 事件后处理
        /// </summary>
        /// <param name="arg"></param>
        private void onEventAfter(IEventArg arg)
        {
            foreach (var anim in _animationCache.OfType<EventAnimation>().Where(a => a.eventArg == arg))
            {
                anim.postEvent(this);
            }
            foreach (var anim in _animationQueue.OfType<EventAnimation>().Where(a => a.eventArg == arg))
            {
                anim.postEvent(this);
            }
            table.TurnEndButton.setInteractable(game.getCurrentPlayer() == player);
        }
        #endregion
        private void reset()
        {
            ui.InitReplaceDialog.setActive(false);
            ui.InitReplaceDialog.clearChoices();
            ui.TurnTipImage.setActive(false);

            table.SelfMaster.setArmor(0);
            table.EnemyMaster.setArmor(0);

            table.SelfGem.setGem(0);
            table.EnemyGem.setGem(0);

            table.SelfItem.setDisplay(false);
            table.EnemyItem.setDisplay(false);
            _enemyItemDesc = null;
            _selfItemDesc = null;

            table.SelfField.clear();
            table.EnemyField.clear();

            table.SelfHandList.clear();
            table.EnemyHandList.clear();

            table.CardDisplayer.setVisible(false);
            table.AttackArrowImage.setActive(false);
            table.Fatigue.setActive(false);
            table.Discover.setActive(false);

            // UI
            ui.TipText.gameObject.SetActive(false);
            ui.PilePanel.setActive(false);
            ui.TipBox.hide();
            ui.GameResultDialog.hide();

            eventCache.clear();
            _mulliganedCards.Clear();
            _animationCache.Clear();
            _animationQueue.Clear();
            _descriptors.Clear();
            foreach (var pair in cardServantDic)
            {
                pair.Value.destroy();
            }
            cardServantDic.Clear();
            cardSecretDic.Clear();
            foreach (var pair in cardHandDic)
            {
                pair.Value.destroy();
            }
            cardHandDic.Clear();
            DestroyInvokeCard();
        }
        #endregion
        public TableUI ui => _ui;
        public BattleTable table => _table;
        public CardEngine game { get; private set; }
        public Player player { get; private set; }
        public CardManager cardManager => _main.card;
        public LangManager lang => _main.lang;
        public SEManager sounds => _main.getManager<SEManager>();
        public THHResourceManager resources => _main.getManager<THHResourceManager>();
        private ActionEventCache eventCache = new ActionEventCache();
        private List<Card> _mulliganedCards = new List<Card>();
        private MainManager _main;
        [SerializeField]
        private TableUI _ui;
        [SerializeField]
        private BattleTable _table;


        [Header("Config")]
        [SerializeField]
        private float _skillDragThreshold = 75;
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
    }
}
