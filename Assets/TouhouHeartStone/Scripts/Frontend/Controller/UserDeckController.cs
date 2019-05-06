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
    public class UserDeckController : MonoBehaviour
    {
        [SerializeField]
        CrystalBarViewModel crystalBar;

        [SerializeField]
        CharacterInfoViewModel characterInfo;

        [SerializeField]
        CardFaceViewModel cardfacePrefab;

        [SerializeField]
        CardStackViewModel cardStackLibrary;

        [SerializeField]
        CardStackViewModel cardStackGrave;

        [SerializeField]
        Transform cardSpawnRoot;

        [SerializeField]
        ThrowCardViewModel throwCard;

        List<CardFaceViewModel> handCards = new List<CardFaceViewModel>();

        public int HandCardCount => handCards.Count;

        public DeckController Deck => GetComponentInParent<DeckController>();


        private bool _IsSelf;
        /// <summary>
        /// 是否为本体
        /// </summary>
        public bool IsSelf => _IsSelf;

        /// <summary>
        /// 初始抽卡
        /// </summary>
        /// <param name="cards"></param>
        public void InitDraw(CardID[] cards)
        {
            GenericAction a = (evt, arg) =>
            {
                UberDebug.LogDebugChannel("Frontend", "准备进入抽卡选择模式");
                EnterThrowingMode(0, 0);
            };
            DrawCard(cards, a);
        }

        GenericAction savedCallback;
        int lastDrawCardRID = -1;

        /// <summary>
        /// 抽一张卡
        /// </summary>
        public void DrawCard(CardID cardID, GenericAction callback)
        {
            setCallback(cardID, callback);
            drawCard(cardID);

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
        public void DrawCard(CardID[] cards, GenericAction callback)
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
                drawCard(cards[i]);
            }
            reArrangeHandCards();
        }

        void onCardDestroy(CardFaceViewModel card)
        {
            if (handCards.Contains(card))
                handCards.Remove(card);

            reArrangeHandCards();
        }

        private CardFaceViewModel drawCard(CardID cardID)
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

        /// <summary>
        /// 设置默认卡牌堆
        /// </summary>
        /// <param name="cards"></param>
        public void SetDeck(int[] cards)
        {
            // todo: 设置卡牌堆
            cardStackLibrary.CardCount = cards.Length;
        }

        #region test
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
        #endregion

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
            // todo: 这个也移动到CardView里面去可好？
            throwCardsInternal(throwingCards);
            throwCard.gameObject.SetActive(false);

            DoAction(this, new ThrowCardEventArgs(SelfID, throwingCards.Select(c => c.RuntimeID).ToArray()));
            throwingCards.Clear();
        }

        public void ThrowCards(CardID[] cards, GenericAction callback)
        {
            List<CardFaceViewModel> throwList = new List<CardFaceViewModel>();
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

            throwCardsInternal(throwList);
            if (throwList.Count > 0)
            {
                reArrangeHandCards();
            }
            callback?.Invoke(this, null);
        }

        private void throwCardsInternal(List<CardFaceViewModel> throwList)
        {
            for (int i = 0; i < throwList.Count; i++)
            {
                throwList[i].PlayAnimation(this, new CardAnimationEventArgs()
                {
                    AnimationName = "CardToStack",
                    EventArgs = new CardPositionEventArgs() { GroupCount = throwList.Count, GroupID = i }
                }, (s, a) =>
                {
                    Destroy((s as MonoBehaviour).gameObject);
                    UberDebug.LogDebugChannel("Frontend", $"准备销毁{s}");
                });
                UberDebug.LogDebugChannel("Frontend", $"丢弃卡{throwList[i]}");
            }
        }

        /// <summary>
        /// 进入丢卡模式
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void EnterThrowingMode(int min, int max)
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
        List<CardFaceViewModel> throwingCards = new List<CardFaceViewModel>();

        public int ThrowingCardCount => throwingCards.Count;

        /// <summary>
        /// 准备丢弃卡牌
        /// </summary>
        /// <param name="card"></param>
        /// <param name="moveIn"></param>
        public void PrepareThrowCard(CardFaceViewModel card, bool moveIn)
        {
            if (moveIn)
            {
                throwingCards.Add(card);
                handCards.Remove(card);
            }
            else
            {
                if (throwingCards.Contains(card))
                {
                    throwingCards.Remove(card);
                    handCards.Add(card);
                }
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

        public event GenericAction OnDeckAction;
        public void DoAction(object sender, EventArgs args)
        {
            if (args is IPlayer)
            {
                (args as IPlayer).PlayerID = SelfID;
            }

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

            OnDeckAction?.Invoke(sender, args);
        }

        public void RecvAction(EventArgs args, GenericAction callback = null)
        {
            // 若传入事件是卡相关事件，则交予卡处理
            if (args is ICardID)
            {
                var card = handCards.Where(e => e.RuntimeID == (args as ICardID).CardRID);
                if (card.Count() > 0)
                {
                    card.First().RecvAction(args, callback);
                }
            }

            // 设置水晶的事件
            if (args is SetGemEventArgs)
            {
                var gemArgs = args as SetGemEventArgs;
                if (gemArgs.MaxGem>=0)
                    crystalBar.CrystalTotal = gemArgs.MaxGem;
                if (gemArgs.CurrentGem >= 0)
                    crystalBar.CrystalUsed = crystalBar.CrystalTotal - gemArgs.CurrentGem;

                callback?.Invoke(this, null);
                return;
            }
        }
    }
}
