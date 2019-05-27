using TouhouHeartstone.Frontend.View.Animation;
using TouhouHeartstone.Frontend.ViewModel;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TouhouHeartstone.Frontend.Model;
using System;
using IGensoukyo.Utilities;

namespace TouhouHeartstone.Frontend.Controller
{
    /// <summary>
    /// 用户桌面
    /// </summary>
    public class BoardController : MonoBehaviour
    {
        [SerializeField]
        CrystalBarViewModel crystalBar;

        [SerializeField]
        CharacterInfoViewModel characterInfo;

        [SerializeField]
        CardViewModel cardfacePrefab;

        [SerializeField]
        CardStackViewModel cardStackLibrary;

        [SerializeField]
        CardStackViewModel cardStackGrave;

        [SerializeField]
        Transform cardSpawnRoot;

        [SerializeField]
        ThrowCardViewModel throwCard;

        List<CardViewModel> handCards = new List<CardViewModel>();

        [SerializeField]
        private bool _IsSelf;

        [SerializeField]
        CardViewModel _selectingCard = null;

        public CardViewModel selectingCard
        {
            get { return _selectingCard; }
            set { _selectingCard = value; }
        }

        public int HandCardCount => handCards.Count;

        public DeckController Deck => GetComponentInParent<DeckController>();
        /// <summary>
        /// 是否为本体
        /// </summary>
        public bool IsSelf
        {
            get { return _IsSelf; }
        }

        #region draw
        /// <summary>
        /// 初始抽卡
        /// </summary>
        /// <param name="cards"></param>
        public void InitDraw(CardID[] cards)
        {
            GenericAction a = (evt, arg) =>
            {
                UberDebug.LogDebugChannel("Frontend", "准备进入抽卡选择模式");
                enterThrowingMode(0, 0);
            };
            DrawCard(cards, a);
        }

        GenericAction savedCallback;
        int lastDrawCardRID = -1;

        /// <summary>
        /// 抽一张卡
        /// </summary>
        void DrawCard(CardID cardID, GenericAction callback)
        {
            setCallback(cardID, callback);
            drawCardInternal(cardID);

            reArrangeHandCards();
        }

        private void setCallback(CardID cardID, GenericAction callback)
        {
            if (savedCallback != null)
                savedCallback?.Invoke(this, null);
            lastDrawCardRID = cardID.CardRID;
            savedCallback = callback;
        }

        /// <summary>
        /// 抽多张卡
        /// </summary>
        /// <param name="cards"></param>
        /// <param name="callback"></param>
        void DrawCard(CardID[] cards, GenericAction callback)
        {
            if (cards.Length == 0)
            {
                callback?.Invoke(this, null);
                return;
            }
            for (int i = 0; i < cards.Length; i++)
            {
                var item = cards[i];
                if (i == 0)
                {
                    setCallback(item, callback);
                }
                drawCardInternal(cards[i]);
            }
            reArrangeHandCards();
        }

        void onCardDestroy(CardViewModel card)
        {
            if (handCards.Contains(card))
                handCards.Remove(card);

            reArrangeHandCards();
        }

        private CardViewModel drawCardInternal(CardID cardID)
        {
            var card = Instantiate(cardfacePrefab, cardSpawnRoot);
            card.gameObject.SetActive(true);
            card.CardID = cardID.CardDID;
            card.RuntimeID = cardID.CardRID;
            card.Index = handCards.Count;

            card.OnDestroyEvent += onCardDestroy;
            card.OnActionEvent += DoAction;

            handCards.Add(card);
            return card;
        }
        #endregion

        void Start()
        {
            #region test_data
            characterInfo.Character = new Model.CharacterData() { Atk = 3, Defence = 4, HP = 9 };
            cardStackGrave.CardCount = 2;
            #endregion

            cardfacePrefab.gameObject.SetActive(false);
            throwCard.gameObject.SetActive(false);

            throwCard.OnThrow += onThrow;
        }

        private int _SelfID;
        public int SelfID => _SelfID;

        /// <summary>
        /// 设置自己的ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="character"></param>
        public void Init(int id, CardID character, bool isSelf)
        {
            _SelfID = id;
            _IsSelf = isSelf;

            if (!isSelf)
            {
                transform.localRotation = Quaternion.Euler(0, 0, 180);
            }

            // todo: 设置角色图像
        }

        #region throw
        /// <summary>
        /// 点击了丢卡按钮
        /// </summary>
        private void onThrow()
        {
            throwCardsInternal(throwingCards.ToArray());
            throwCard.gameObject.SetActive(false);

            DoAction(this, new ThrowCardEventArgs(SelfID, throwingCards.Select(c => c.RuntimeID).ToArray()));
            throwingCards.Clear();
        }

        private void throwCards(CardID[] cards, GenericAction callback)
        {
            List<CardViewModel> throwList = new List<CardViewModel>();
            foreach (var card in cards)
            {
                var f = handCards.Where(v => v.RuntimeID == card.CardRID);
                if (f.Count() > 0)
                {
                    var c = f.First();
                    throwList.Add(c);
                    handCards.Remove(c);
                }
            }

            throwCardsInternal(throwList.ToArray(), callback);
            if (throwList.Count > 0)
            {
                reArrangeHandCards();
            }
        }

        /// <summary>
        /// 内部丢卡
        /// </summary>
        /// <param name="throwCards"></param>
        /// <param name="callback"></param>
        private void throwCardsInternal(CardViewModel[] throwCards, GenericAction callback = null)
        {
            if (throwCards.Length == 0)
            {
                callback?.Invoke(this, null);
                return;
            }

            GenericAction throwCallback = (s, a) =>
            {
                UberDebug.LogDebugChannel("Frontend", $"准备销毁{s}");
                Destroy((s as MonoBehaviour).gameObject);
            };

            GenericAction realCallback = throwCallback + callback;

            for (int i = 0; i < throwCards.Length; i++)
            {
                throwCards[i].RecvAction(new CardToStackEventArgs() { Count = throwCards.Length, Index = i }, i == 0 ? realCallback : throwCallback);
            }
        }

        /// <summary>
        /// 进入丢卡模式
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        void enterThrowingMode(int min, int max)
        {
            // 仅本玩家显示丢卡
            if (IsSelf)
            {
                throwCard.gameObject.SetActive(true);
                throwCard.Max = max;
                throwCard.Min = min;
            }
        }

        /// <summary>
        /// 当前是否正在丢卡
        /// </summary>
        public bool ThrowingCard => throwCard.gameObject.activeSelf;

        /// <summary>
        /// 等待被丢的卡
        /// </summary>
        List<CardViewModel> throwingCards = new List<CardViewModel>();

        public int ThrowingCardCount => throwingCards.Count;

        void PrepareThrowCard(int cardRID, bool moveIn)
        {
            CardViewModel card;
            if (moveIn)
                card = handCards.Where(e => e.RuntimeID == cardRID).FirstOrDefault();
            else
                card = throwingCards.Where(e => e.RuntimeID == cardRID).FirstOrDefault();

            PrepareThrowCard(card, moveIn);
        }

        /// <summary>
        /// 准备丢弃卡牌
        /// </summary>
        /// <param name="card"></param>
        /// <param name="moveIn"></param>
        void PrepareThrowCard(CardViewModel card, bool moveIn)
        {
            if (moveIn)
            {
                throwingCards.Add(card);
                handCards.Remove(card);
            }
            else
            {
                throwingCards.Remove(card);
                handCards.Add(card);
            }

            for (int i = 0; i < throwingCards.Count; i++)
            {
                throwingCards[i].Index = i;
                throwingCards[i].RecvAction(new IndexChangeEventArgs(i));
            }
            reArrangeHandCards();
        }

        private void reArrangeHandCards()
        {
            for (int i = 0; i < handCards.Count; i++)
            {
                handCards[i].Index = i;
                if (handCards[i] != null)
                    handCards[i].RecvAction(new IndexChangeEventArgs(i));
            }
        }
        #endregion

        #region event_handler

        public event GenericAction OnDeckAction;

        /// <summary>
        /// 下面传上来的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void DoAction(object sender, EventArgs args)
        {
            // 处理抽卡完成的事件
            if (args is CardDrewEventArgs)
            {
                var arg = args as CardDrewEventArgs;
                UberDebug.LogDebugChannel("Frontend", $"卡{arg}抽出完毕");
                if (arg.CardRID == lastDrawCardRID)
                {
                    savedCallback?.Invoke(this, null);
                    lastDrawCardRID = -1;
                    savedCallback = null;
                }
            }

            // 准备丢卡
            if (args is PrepareThrowEventArgs)
            {
                var arg = args as PrepareThrowEventArgs;
                PrepareThrowCard(arg.CardRID, arg.State);
            }

            // 预览随从
            if (args is RetinuePreview)
            {
                retinuePreview((args as RetinuePreview).Position);
            }

            if (args is IPlayerEventArgs)
            {
                (args as IPlayerEventArgs).PlayerID = SelfID;
                OnDeckAction?.Invoke(sender, args);
            }
        }

        CardViewModel GetCardByRID(int rid)
        {
            var card = handCards.Where(e => e.RuntimeID == rid);
            if (card.Count() > 0)
                return card.First();
            card = retinues.Where(e => e.RuntimeID == rid);
            if (card.Count() > 0)
                return card.First();
            card = throwingCards.Where(e => e.RuntimeID == rid);
            if (card.Count() > 0)
                return card.First();

            return null;
        }

        /// <summary>
        /// 上面传下去的事件
        /// </summary>
        /// <param name="args"></param>
        /// <param name="callback"></param>
        public void RecvAction(EventArgs args, GenericAction callback = null)
        {
            // 设置水晶的事件
            if (args is SetGemEventArgs)
            {
                var gemArgs = args as SetGemEventArgs;
                if (gemArgs.MaxGem >= 0)
                    crystalBar.CrystalTotal = gemArgs.MaxGem;
                if (gemArgs.CurrentGem >= 0)
                    crystalBar.CrystalUsed = crystalBar.CrystalTotal - gemArgs.CurrentGem;

                callback?.Invoke(this, null);
                return;
            }

            // 丢卡事件
            if (args is ThrowCardEventArgs)
            {
                var arg = args as ThrowCardEventArgs;

                if (arg.NewCards.Length == 0)
                {
                    throwCards(arg.Cards, callback);
                }
                else
                {
                    throwCards(arg.Cards, (a, b) =>
                    {
                        DrawCard(arg.NewCards, callback);
                    });
                }
            }

            // 抽卡事件
            if (args is DrawCardEventArgs)
            {
                DrawCard((args as DrawCardEventArgs).Card, callback);
            }

            // 设置牌库
            if (args is SetUserDeckEventArgs)
            {
                var arg = args as SetUserDeckEventArgs;
                // todo: 设置牌库
                cardStackLibrary.CardCount = arg.CardsDID.Length;
            }

            // 设置这个什么玩意来着……没错是随从
            if (args is RetinueSummonEventArgs)
            {
                retinueSummon(args as RetinueSummonEventArgs);
            }

            // 若传入事件是卡相关事件，则交予卡处理
            if (args is ICardEventArgs)
            {
                var rid = (args as ICardEventArgs).CardRID;
                var card = GetCardByRID(rid);
                if (card != null)
                {
                    card.RecvAction(args, callback);
                }
                else
                {
                    UberDebug.LogWarningChannel("Frontend", $"没用找到对应RID为{rid}的卡片。");
                }
            }
        }
        #endregion

        #region Retinue

        List<CardViewModel> retinues = new List<CardViewModel>();

        /// <summary>
        /// 随从数量
        /// </summary>
        public int RetinueCount => retinues.Count;

        /// <summary>
        /// 将一个随从放到场上
        /// </summary>
        /// <param name="arg"></param>
        void retinueSummon(RetinueSummonEventArgs arg)
        {
            var cards = handCards.Where(c => c.RuntimeID == arg.CardRID);
            if (cards.Count() == 1)
            {
                var card = cards.First();
                handCards.Remove(card);
                retinues.Insert(arg.Position, card);

                reArrangeHandCards();
                reArrangeRetinues();
            }
            else
            {
                UberDebug.LogWarningChannel("Frontend", "没找到对应的卡片");
            }
        }

        /// <summary>
        /// 重排列随从
        /// </summary>
        void reArrangeRetinues()
        {
            for (int i = 0; i < retinues.Count; i++)
            {
                retinues[i].Index = i;
                if (retinues[i] != null)
                    retinues[i].RecvAction(new IndexChangeEventArgs(i));
            }
        }

        /// <summary>
        /// 预览随从位置
        /// </summary>
        /// <param name="index"></param>
        void retinuePreview(int index)
        {
            if (index == -1)
            {
                reArrangeRetinues();
            }
            else
            {
                for (int i = 0; i < retinues.Count; i++)
                {
                    int previewIndex = i >= index ? i + 1 : i;
                    if (retinues[i] != null)
                        retinues[i].RecvAction(new IndexChangeEventArgs(previewIndex) { Count = retinues.Count + 1 });
                }
            }
        }

        #endregion

    }
}
