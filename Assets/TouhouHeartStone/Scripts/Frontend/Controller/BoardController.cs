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
        CrystalBarViewModel crystalBar = null;

        [SerializeField]
        CharacterInfoViewModel characterInfo = null;

        [SerializeField]
        CardViewModel cardfacePrefab = null;

        [SerializeField]
        CardStackViewModel cardStackLibrary = null;

        [SerializeField]
        CardStackViewModel cardStackGrave = null;

        [SerializeField]
        Transform cardSpawnRoot = null;

        [SerializeField]
        Transform throwRoot = null;

        [SerializeField]
        Transform servantRoot = null;

        /// <summary>
        /// 场上的所有卡（包括手上、等待丢弃和随从卡）
        /// </summary>
        List<CardViewModel> cards = new List<CardViewModel>();

        /// <summary>
        /// 手上的卡
        /// </summary>
        List<CardViewModel> handCards = new List<CardViewModel>();

        /// <summary>
        /// 手牌的数目
        /// </summary>
        public int HandCardCount => handCards.Count;

        /// <summary>
        /// 关联的DeckController
        /// </summary>
        public DeckController Deck => GetComponentInParent<DeckController>();

        /// <summary>
        /// 是否为本体
        /// </summary>
        public bool IsSelf { get; private set; }

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

        /// <summary>
        /// 卡片销毁时做的工作
        /// </summary>
        /// <param name="card"></param>
        void onCardDestroy(CardViewModel card)
        {
            if (cards.Contains(card))
            {
                if (handCards.Contains(card))
                    handCards.Remove(card);
                if (servants.Contains(card))
                    servants.Remove(card);
                if (preparingThrowCards.Contains(card))
                    preparingThrowCards.Remove(card);
            }
            reArrangeHandCards();
        }

        /// <summary>
        /// 内部 生成一张卡
        /// </summary>
        /// <param name="cardID"></param>
        /// <returns></returns>
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
            cards.Add(card);
            return card;
        }
        #endregion

        void Start()
        {
            #region test_data
            characterInfo.Character = new Model.CharacterData() { Atk = 3, Defence = 4, HP = 9 };
            cardStackGrave.CardCount = 2;
            #endregion
        }

        /// <summary>
        /// 自己的ID
        /// </summary>
        public int SelfID { get; private set; }

        /// <summary>
        /// 设置自己的ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="character"></param>
        public void Init(int id, CardID character, bool isSelf)
        {
            SelfID = id;
            IsSelf = isSelf;

            // todo: 设置角色图像
        }

        #region throw
        /// <summary>
        /// 点击了丢卡按钮
        /// </summary>
        private void onThrow()
        {
            throwCardsInternal(preparingThrowCards.ToArray());

            DoAction(this, new ThrowCardEventArgs(SelfID, preparingThrowCards.Select(c => c.RuntimeID).ToArray()));
            preparingThrowCards.Clear();
        }

        /// <summary>
        /// 丢卡
        /// </summary>
        /// <param name="cards"></param>
        /// <param name="callback"></param>
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
                    this.cards.Remove(c);
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
                Deck.CommonDeck.ShowThrowCardDialog(min, max, onThrow);
            }
        }

        /// <summary>
        /// 当前是否正在丢卡
        /// </summary>
        public bool ThrowingCard => Deck.CommonDeck.Throwing;

        /// <summary>
        /// 等待被丢的卡
        /// </summary>
        List<CardViewModel> preparingThrowCards = new List<CardViewModel>();

        /// <summary>
        /// 当前丢卡的数目
        /// </summary>
        public int ThrowingCardCount => preparingThrowCards.Count;

        /// <summary>
        /// 将卡加入准备丢弃队列中
        /// </summary>
        /// <param name="cardRID"></param>
        /// <param name="moveIn"></param>
        void PrepareThrowCard(int cardRID, bool moveIn)
        {
            CardViewModel card;
            if (moveIn)
                card = handCards.Where(e => e.RuntimeID == cardRID).FirstOrDefault();
            else
                card = preparingThrowCards.Where(e => e.RuntimeID == cardRID).FirstOrDefault();

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
                preparingThrowCards.Add(card);
                handCards.Remove(card);
                moveCard(card, CardPos.Throw);
            }
            else
            {
                preparingThrowCards.Remove(card);
                handCards.Add(card);
                moveCard(card, CardPos.Hand);
            }

            for (int i = 0; i < preparingThrowCards.Count; i++)
            {
                preparingThrowCards[i].Index = i;
                preparingThrowCards[i].RecvAction(new IndexChangeEventArgs(i));
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
                servantPreviewInsert((args as RetinuePreview).Position);
            }

            if (args is IPlayerEventArgs)
            {
                (args as IPlayerEventArgs).PlayerID = SelfID;
                OnDeckAction?.Invoke(sender, args);
            }
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
                servantSummon(args as RetinueSummonEventArgs);
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
                    UberDebug.LogWarningChannel("Frontend", $"没用找到对应RID为{rid}的卡片。当前卡片列表：{getCardsString()}");
                    callback?.Invoke(this, null);
                }
            }
        }
        #endregion

        #region Servant

        List<CardViewModel> servants = new List<CardViewModel>();

        /// <summary>
        /// 随从数量
        /// </summary>
        public int ServantCount => servants.Count;

        /// <summary>
        /// 将一个随从放到场上
        /// </summary>
        /// <param name="arg"></param>
        void servantSummon(RetinueSummonEventArgs arg)
        {
            var cards = handCards.Where(c => c.RuntimeID == arg.CardRID);
            if (cards.Count() == 1)
            {
                var card = cards.First();
                handCards.Remove(card);
                servants.Insert(arg.Position, card);
                moveCard(card, CardPos.Servant);

                reArrangeHandCards();
                reArrangeServants();
            }
            else
            {
                UberDebug.LogWarningChannel("Frontend", "没找到对应的卡片");
            }
        }

        /// <summary>
        /// 重排列随从
        /// </summary>
        void reArrangeServants()
        {
            for (int i = 0; i < servants.Count; i++)
            {
                servants[i].Index = i;
                if (servants[i] != null)
                    servants[i].RecvAction(new IndexChangeEventArgs(i));
            }
        }

        /// <summary>
        /// 预览随从位置
        /// </summary>
        /// <param name="index"></param>
        void servantPreviewInsert(int index)
        {
            if (index == -1)
            {
                reArrangeServants();
            }
            else
            {
                for (int i = 0; i < servants.Count; i++)
                {
                    int previewIndex = i >= index ? i + 1 : i;
                    if (servants[i] != null)
                        servants[i].RecvAction(new IndexChangeEventArgs(previewIndex) { Count = servants.Count + 1 });
                }
            }
        }
        #endregion

        /// <summary>
        /// 根据ID获取卡
        /// </summary>
        /// <param name="rid"></param>
        /// <returns></returns>
        public CardViewModel GetCardByRID(int rid)
        {
            var card = cards.Where(e => e.RuntimeID == rid);
            if (card.Count() > 0)
                return card.First();
            return null;
        }

        void moveCard(CardViewModel card, CardPos pos)
        {
            switch(pos)
            {
                case CardPos.Hand:
                    card.transform.SetParent(cardSpawnRoot);
                    break;
                case CardPos.Servant:
                    card.transform.SetParent(servantRoot);
                    card.transform.localPosition = Vector3.zero;
                    break;
                case CardPos.Throw:
                    card.transform.SetParent(throwRoot);
                    break;
                default:
                    return;
            }
        }

        enum CardPos
        {
            Hand,
            Throw,
            Servant
        }

        string getCardsString()
        {
            string str = "手牌叠：{";
            for (int i = 0; i < handCards.Count; i++)
            {
                if (i > 0) str += ", ";
                str += handCards[i].ToString();
            }

            str += "}, 随从叠：{";
            for (int i = 0; i < servants.Count; i++)
            {
                if (i > 0) str += ", ";
                str += servants[i].ToString();
            }

            str += "}, 丢弃叠：{";
            for (int i = 0; i < preparingThrowCards.Count; i++)
            {
                if (i > 0) str += ", ";
                str += preparingThrowCards[i].ToString();
            }
            str += "}";

            return str;
        }
    }
}
