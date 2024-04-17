using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BJSYGameCore;
using Game.BattleTable.UI;
using NGettext.Plural.Ast;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouCardEngine.Shared;
using TouhouHeartstone;
using UI;
using UnityEngine;
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
            initDecksAndGraves();
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
        public bool isTurnTipImageActive()
        {
            return ui.TurnTipImage.gameObject.activeSelf;
        }
        public void showTipBox(string tip, GameObject source)
        {
            ui.TipBox.display(tip);
            _tipboxSource = source;
        }
        public void setTipboxPosition(Vector3 tablePos)
        {
            var worldPos = table.TableToWorldPosition(tablePos + _tipBoxOffset);
            ui.TipBox.setPosition(worldPos);
        }
        public void hideTipBoxFromObject(GameObject source)
        {
            if (_tipboxSource == source)
            {
                _tipboxSource = null;
                ui.TipBox.hide();
            }
        }
        public void showGameResult(bool result)
        {
            ui.GameResultDialog.ShowGameResult(result);
        }
        public void setOpponentWaiting(bool waiting)
        {
            ui.OpponentWaitingObj.SetActive(waiting);
        }
        public void setCanControl(bool value)
        {
            canControl = value;
        }
        public bool canSeeCard(ICardData cardData)
        {
            if (cardData == null)
                return false;
            return canSeeCard(cardData.id);
        }
        public bool canSeeCard(int cardId)
        {
            var cardDesc = getCardDescriptor(cardId);
            return canSeeCard(cardDesc);
        }
        public bool canSeeCard(CardDescriptor cardDesc)
        {
            if (cardDesc == null)
                return false;
            if (canSeeAllCards)
                return true;
            return cardDesc.isVisibleTo(player);
        }
        #region 坐标转换
        // 为了防止Table的Transform被随便旋转缩放后造成的坐标错乱问题，现分为世界坐标和桌面坐标两个坐标系。
        // 世界坐标即Unity场景中各种transform的position，单位为每100px为1。
        // 桌面坐标则是xz平面组成2D画面，y为深度的坐标系，单位为每1px为1。
        // 
        // 简单来说，TableManager的大多数方法使用的都是桌面坐标，因此需要将transform.position转换为桌面坐标才能使用。
        // transform.localPosition等局部变换则不需要动，因为这些不会受到影响。
        public Vector3 WorldToTablePosition(Vector3 worldPos) => table.WorldToTablePosition(worldPos);
        public Vector3 WorldToTableScale(Vector3 worldScale) => table.WorldToTableScale(worldScale);
        public float WorldToTableDistance(float worldDis) => table.WorldToTableDistance(worldDis);
        public Vector3 TableToWorldPosition(Vector3 tablePos) => table.TableToWorldPosition(tablePos);
        public Vector3 TableToWorldScale(Vector3 tableScale) => table.TableToWorldScale(tableScale);
        public float TableToWorldDistance(float tableDis) => table.TableToWorldDistance(tableDis);
        public Vector3 ScreenToWorldPosition(Vector3 screenPos) => table.Camera.ScreenToWorldPoint(screenPos);
        public Vector3 ScreenToTablePosition(Vector3 screenPos) => WorldToTablePosition(ScreenToWorldPosition(screenPos));
        #endregion

        #region 皮肤
        public IEnumerable<ICardPoolReference> getCardPoolSet()
        {
            return game.getOption().cardPools;
        }
        public ICardPoolReference getCardPoolReference(long cardPoolId)
        {
            var poolRefs = getCardPoolSet();
            return cardManager.getCardPoolReferenceInEnv(cardPoolId, poolRefs);
        }
        public CardSkinData getCardSkin(ICardData card)
        {
            return getCardSkin(card.define);
        }
        public CardSkinData getCardSkin(CardDescriptor desc)
        {
            return getCardSkin(desc.define);
        }
        public CardSkinData getCardSkin(CardDefine define)
        {
            if (define == null)
                return null;
            CardSkinData skin = getCardSkin(define.cardPoolId, define.id);
            if (skin != null)
                return skin;
            else
            {
                game.logger.logError($"没有找到{define}对应的皮肤");
                return new CardSkinData()
                {
                    id = define.id,
                    desc = $"{define}（这张卡随时可能爆炸！）",
                    imagePath = cardManager.getDefaultImagePath(),
                    name = define.GetType().Name
                };
            }
        }
        public CardSkinData getCardSkin(long cardPoolId, int id)
        {
            var poolRef = getCardPoolReference(cardPoolId);
            return cardManager.getCardSkin(poolRef, id);
        }
        public BuffSkin getBuffSkin(BuffDefine define)
        {
            if (define == null)
                return null;
            var poolRef = getCardPoolReference(define.cardPoolId);
            return cardManager.getBuffSkin(poolRef, define.id);
        }
        public BuffSkin getBuffSkin(DefineReference defineRef)
        {
            if (defineRef == null)
                return null;
            var poolRef = getCardPoolReference(defineRef.cardPoolId);
            return cardManager.getBuffSkin(poolRef, defineRef.defineId);
        }
        public Task<Sprite> loadCardImage(CardDescriptor desc)
        {
            return loadCardImage(desc.define);
        }
        public Task<Sprite> loadCardImage(CardDefine define)
        {
            var poolRef = getCardPoolReference(define.cardPoolId);
            return cardManager.loadCardImage(poolRef, define.id);
        }
        public Task<Sprite> loadBuffImage(BuffDefine define)
        {
            var poolRef = getCardPoolReference(define.cardPoolId);
            return cardManager.loadBuffImage(poolRef, define.id);
        }
        #endregion

        #region 衍生物&关键词
        public CardDefine[] getCardTokens(CardDefine card)
        {
            if (_cardTokenDict.TryGetValue(card, out var tokens))
            {
                return tokens;
            }
            tokens = cardManager.evaluateCardTokens(card, getCardPoolSet());
            _cardTokenDict.Add(card, tokens);
            return tokens;
        }
        public string[] getCardDefineReferedKeywords(CardDefine card)
        {
            if (_cardKeywordsDict.TryGetValue(card, out var keywords))
                return keywords;

            var skin = getCardSkin(card);
            if (skin == null)
                return null;
            keywords = cardManager.getKeywordsInDescription(skin.desc);
            _cardKeywordsDict.Add(card, keywords);
            return keywords;
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
            table.SelfField.onServantPointerUp += onServantPointerUpCallback;

            table.EnemyField.onServantClick += onServantClickCallback;
            table.EnemyField.onServantBeginDrag += onServantBeginDragCallback;
            table.EnemyField.onServantDrag += onServantDragCallback;
            table.EnemyField.onServantEndDrag += onServantEndDragCallback;
            table.EnemyField.onServantPointerEnter += onServantPointerEnterCallback;
            table.EnemyField.onServantPointerExit += onServantPointerExitCallback;
            table.EnemyField.onServantPointerUp += onServantPointerUpCallback;

            table.SelfMaster.onClick += onMasterClickCallback;
            table.SelfMaster.onBeginDrag += onMasterBeginDragCallback;
            table.SelfMaster.onDrag += onMasterDragCallback;
            table.SelfMaster.onEndDrag += onMasterEndDragCallback;
            table.SelfMaster.onPointerUp += onMasterPointerUpCallback;
            table.SelfMaster.HeadSlots.onItemPointerEnter += onHeadItemPointerEnterCallback;
            table.SelfMaster.HeadSlots.onItemPointerExit += onHeadItemPointerExitCallback;

            table.EnemyMaster.onClick += onMasterClickCallback;
            table.EnemyMaster.onBeginDrag += onMasterBeginDragCallback;
            table.EnemyMaster.onDrag += onMasterDragCallback;
            table.EnemyMaster.onEndDrag += onMasterEndDragCallback;
            table.EnemyMaster.onPointerUp += onMasterPointerUpCallback;
            table.EnemyMaster.HeadSlots.onItemPointerEnter += onHeadItemPointerEnterCallback;
            table.EnemyMaster.HeadSlots.onItemPointerExit += onHeadItemPointerExitCallback;


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
            table.EnemyDeck.onClick += onDeckClickCallback;
            table.EnemyDeck.onPointerEnter += onDeckPointerEnterCallback;
            table.EnemyDeck.onPointerExit += onDeckPointerExitCallback;

            table.SelfGrave.onClick += onGraveClickCallback;
            table.SelfGrave.onPointerEnter += onGravePointerEnterCallback;
            table.SelfGrave.onPointerExit += onGravePointerExitCallback;
            table.EnemyGrave.onClick += onGraveClickCallback;
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
            table.Discover.onItemPointerEnter += onCardChooseItemPointerEnterCallback;
            table.Discover.onItemPointerExit += onCardChooseItemPointerExitCallback;

            ui.InitReplaceDialog.onItemClicked += onMulliganItemClickCallback;
            ui.InitReplaceDialog.onAccept += onMulliganAcceptCallback;

            ui.PilePanel.onCloseButtonClicked += onClosePilePanelClickCallback;
            ui.PilePanel.onItemPointerEnter += onPilePanelItemPointerEnterCallback;
            ui.PilePanel.onItemPointerExit += onPilePanelItemPointerExitCallback;

            loadAnim(GetType().Assembly);
        }
        void Update()
        {
            table.update(Time.deltaTime);


            // 鼠标悬浮到仆从/道具/陷阱上查看
            if (_showCardTimer.isExpired())
            {
                // 显示卡面
                if (tryGetHoveringPosition(out var targetPosition))
                {
                    var hoveringCard = getHoveringCard();
                    if (hoveringCard != null)
                    {
                        var worldPosition = TableToWorldPosition(targetPosition);
                        showCardDisplayer(worldPosition, getCardDescriptor(hoveringCard));
                    }
                }
                _showCardTimer.reset();
            }
            if (_isDisplayingCard)
            {
                var hoveringCard = getHoveringCard();
                if (hoveringCard == null)
                {
                    hideCardDisplayer();
                }
            }

            // 查看仆从/道具/陷阱/手牌/检视选项/牌堆卡牌的衍生卡和关键词。
            if (!_isPopingUpCard)
            {
                if (_popupCardTimer.isExpired() && tryGetPopupHoveringPosition(out var popupPosition))
                {
                    _isPopingUpCard = true;
                    var tokenDefine = getPopupHoveringCardToken();
                    var keywords = getPopupHoveringCardKeywords();
                    bool shouldShow = tokenDefine != null || (keywords != null && keywords.Length > 0);
                    if (shouldShow)
                    {
                        showCardPopUp(popupPosition, tokenDefine, keywords);
                    }
                    if (_popupCardTimer.isStarted)
                        _popupCardTimer.reset();
                }
            }
            else
            {
                if (tryGetPopupHoveringPosition(out var popupPosition))
                {
                    setCardPopUpPosition(popupPosition);
                }
                else
                {
                    hideCardPopUp();
                }
            }

            // 错误提示
            if (_tipTimer.isExpired())
            {
                _tipTimer.reset();
                ui.ErrorTipText.gameObject.SetActive(false);
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
            Vector3 pointerWorldPosition = table.Camera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 pointerPosition = WorldToTablePosition(pointerWorldPosition);
            Vector3 tipboxPos = pointerPosition + _tipBoxOffset;
            ui.TipBox.setPosition(TableToWorldPosition(tipboxPos));
            if (!_tipboxSource)
            {
                ui.TipBox.hide();
            }

            if (isSelectingTarget())
            {
                var targetPosition = pointerPosition;
                targetPosition.y = 0;
                showArrowWhileUsing(targetPosition);
            }

            //随从牌使用预览
            var removeKeys = _servantPreviewAnimDict.Where(p => p.Value.isFinished()).Select(p => p.Key).ToArray();
            foreach (var key in removeKeys)
            {
                _servantPreviewAnimDict.Remove(key);
            }
            //游戏事件处理
            eventCache.poll();
            //动画
            queueAnims();
            updateAnim();
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
        }
        private void LateUpdate()
        {
            updateCardBuffEffects(Time.deltaTime);
        }
        #endregion
        #region 事件回调
        #region 换牌
        private void onMulliganItemClickCallback(int index)
        {
            if (!canControl)
                return;
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
            if (!canControl)
                return;
            player.cmdInitReplace(game, _mulliganedCards.ToArray());
            ui.InitReplaceDialog.setWaiting(true);
        }
        #endregion
        #region 选牌
        private void onCardChooseCallback(int index)
        {
            if (!canControl)
                return;
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
            _cardChooseMode = CardChooseMode.None;
        }
        private void onCardChooseItemPointerEnterCallback(int index)
        {
            startHoverDiscoverItem(table.Discover.getCard(index));
        }
        private void onCardChooseItemPointerExitCallback(int index)
        {
            stopHover();
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
            _viewingDeckName = null;
            _viewingDeckPlayer = null;
        }
        private void onPilePanelItemPointerEnterCallback(int index)
        {
            startHoverPilePanelItem(ui.PilePanel.getCard(index));
        }
        private void onPilePanelItemPointerExitCallback(int index)
        {
            stopHover();
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
            TableAnimation anim;
            switch (obj)
            {
                default:
                    anim = createRequestAnim(obj);
                    break;
            }
            if (anim != null)
                addAnim(anim);
            else if (!(obj is FreeActRequest))
                UberDebug.LogWarningChannel("UI", $"没有与{obj}相应的动画");
            _allControlDirty = true;
        }
        void onResponse(IResponse response)
        {
        }
        /// <summary>
        /// 事件前处理
        /// </summary>
        /// <param name="arg"></param>
        private void onEventBefore(IEventArg arg)
        {
            checkCardSkinEffectAnimations(game.currentFlow, arg as EventArg);
            TableAnimation anim = createEventAnim(arg);
            if (anim != null)
            {
                addAnim(anim);
                addToInsertedAnimsOfParent(arg, anim);
            }

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
            if (arg.define is UpdateDeathEventDefine)
            {
                // 死亡结算事件结束后，清理死亡仆从。
                var anim = new ClearDeadAnchorAnimation();
                addAnim(anim);
                addToInsertedAnimsOfParent(arg, anim);
            }
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
            table.SelfGem.setMaxGem(0);
            table.EnemyGem.setMaxGem(0);

            table.SelfItem.setExist(false);
            table.EnemyItem.setExist(false);
            _enemyItemDesc = null;
            _selfItemDesc = null;

            table.SelfField.clear();
            table.EnemyField.clear();

            table.SelfHandList.clear();
            table.EnemyHandList.clear();

            table.AttackArrowImage.setActive(false);
            table.Fatigue.setActive(false);
            table.Discover.setActive(false);

            // UI
            ui.PilePanel.setActive(false);
            _viewingDeckName = null;
            _viewingDeckPlayer = null;
            ui.CardDisplayer.setVisible(false);
            ui.ErrorTipText.gameObject.SetActive(false);
            ui.TipBox.hide();
            ui.GameResultDialog.hide();
            ui.CardPopUp.initTable(this);

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
            _cardTokenDict.Clear();
            clearDeadCardAnchors();
            clearCardBuffEffects();
            DestroyInvokeCard();
        }
        #endregion
        public TableUI ui => _ui;
        public BattleTable table => _table;
        public CardEngine game { get; private set; }
        public Player player { get; private set; }
        public CardManager cardManager => _main.card;
        public LangManager lang => _main.lang;
        public GameManager gameManager => _main.game;
        public EventManager eventManager => _main.eventManager;
        public AnimationManager animManager => _main.anims;
        public SEManager sounds => _main.seManager;
        public THHResourceManager resources => _main.getManager<THHResourceManager>();
        private ActionEventCache eventCache = new ActionEventCache();
        private List<Card> _mulliganedCards = new List<Card>();
        private Dictionary<CardDefine, CardDefine[]> _cardTokenDict = new Dictionary<CardDefine, CardDefine[]>();
        private Dictionary<CardDefine, string[]> _cardKeywordsDict = new Dictionary<CardDefine, string[]>();
        private MainManager _main;
        private GameObject _tipboxSource;
        /// <summary>
        /// 能否看到所有卡牌，用于回放或观战。
        /// </summary>
        public bool canSeeAllCards { get; set; }
        /// <summary>
        /// 能否控制，用于回放或观战。
        /// </summary>
        private bool canControl = true;
        [SerializeField]
        private TableUI _ui;
        [SerializeField]
        private BattleTable _table;


        [Header("Config")]
        [SerializeField]
        private float _skillDragThreshold = 75;
        public float skillDragThreshold => _skillDragThreshold;
        public AnimationCurve cardShowCurve => _cardShowCurve;
        public AnimationCurve toDeckCurveStart => _toDeckCurveStart;
        public AnimationCurve toDeckCurveEnd => _toDeckCurveEnd;
        [SerializeField]
        AnimationCurve _cardShowCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.2f, 1));
        [SerializeField]
        AnimationCurve _toDeckCurveStart = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.6f, 1));
        [SerializeField]
        AnimationCurve _toDeckCurveEnd = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.6f, 1));
    }
}
