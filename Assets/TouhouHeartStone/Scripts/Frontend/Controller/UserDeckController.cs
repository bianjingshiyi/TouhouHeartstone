using TouhouHeartstone.Frontend.View.Animation;
using TouhouHeartstone.Frontend.ViewModel;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TouhouHeartstone.Frontend.Model;
using System;

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
            GenericAction a = (evt, arg) => { EnterThrowingMode(0, 0); };
            DrawCard(cards, a);
        }

        /// <summary>
        /// 抽一张卡
        /// </summary>
        public void DrawCard(CardID cardID, GenericAction callback)
        {
            CardFaceViewModel card = drawCard(cardID);
            card.DrawCallback += callback;

            HandCardChangeEvent?.Invoke();
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
                var card = drawCard(cards[i]);
                if (i == 0)
                {
                    card.DrawCallback += callback;
                }
            }
            HandCardChangeEvent?.Invoke();
        }

        event Action HandCardChangeEvent;

        void onCardDestroy(CardFaceViewModel card)
        {
            HandCardChangeEvent -= card.OnIndexChange;
            if (handCards.Contains(card))
                handCards.Remove(card);
        }

        private CardFaceViewModel drawCard(CardID cardID)
        {
            var card = Instantiate(cardfacePrefab, cardSpawnRoot);
            card.gameObject.SetActive(true);
            card.CardID = cardID.DefineID;
            card.RuntimeID = cardID.RuntimeID;
            card.Index = handCards.Count;

            HandCardChangeEvent += card.OnIndexChange;
            card.OnDestroyEvent += onCardDestroy;

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
            for (int i = 0; i < throwingCards.Count; i++)
            {
                throwingCards[i].PlayAnimation(this, new CardAnimationEventArgs()
                {
                    AnimationName = "CardToStack",
                    EventArgs = new CardPositionEventArgs() { GroupCount = throwingCards.Count, GroupID = i }
                }, (s, a) => {
                    Destroy(s as GameObject);
                });
            }

            throwCard.gameObject.SetActive(false);

            GetComponentInParent<Model.DeckController>()?.InitReplace(SelfID, throwingCards.Select(c => c.RuntimeID).ToArray());
            throwingCards.Clear();
        }

        public void ThrowCards(CardID[] cards, GenericAction callback)
        {
            List<CardFaceViewModel> throwList = new List<CardFaceViewModel>();
            foreach (var card in cards)
            {
                var f = handCards.Where(v => v.RuntimeID == card.RuntimeID)?.First();
                if (f != null)
                {
                    throwList.Add(f);
                    handCards.Remove(f);
                }
            }

            for (int i = 0; i < throwList.Count; i++)
            {
                throwList[i].PlayAnimation(this, new CardAnimationEventArgs()
                {
                    AnimationName = "CardToStack",
                    EventArgs = new CardPositionEventArgs() { GroupCount = throwList.Count, GroupID = i }
                }, (s, a) => {
                    Destroy(s as GameObject);
                });
            }

            if (throwList.Count > 0)
            {
                for (int i = 0; i < handCards.Count; i++)
                {
                    handCards[i].Index = i;
                }
                HandCardChangeEvent?.Invoke();
            }
            callback?.Invoke(this, null);
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
            for (int i = 0; i < handCards.Count; i++)
            {
                handCards[i].Index = i;
            }
            for (int i = 0; i < throwingCards.Count; i++)
            {
                throwingCards[i].Index = i;
            }
            HandCardChangeEvent?.Invoke();
        }
        #endregion 

        /// <summary>
        /// 设置水晶数量
        /// </summary>
        /// <param name="maxGem"></param>
        /// <param name="currentGem"></param>
        public void SetGem(int maxGem, int currentGem)
        {
            crystalBar.CrystalTotal = maxGem;
            crystalBar.CrystalUsed = maxGem - currentGem;
        }

        /// <summary>
        /// 设置最大水晶数量
        /// </summary>
        /// <param name="maxGem"></param>
        public void SetMaxGem(int maxGem)
        {
            crystalBar.CrystalTotal = maxGem;
        }

        public void SetCurrentGem(int currentGem)
        {
            crystalBar.CrystalUsed = crystalBar.CrystalTotal - currentGem;
        }
    }
}
