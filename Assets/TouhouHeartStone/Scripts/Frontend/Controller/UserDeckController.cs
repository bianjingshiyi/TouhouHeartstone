using TouhouHeartstone.Frontend.View.Animation;
using TouhouHeartstone.Frontend.ViewModel;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TouhouHeartstone.Frontend.Model;

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

            for (int i = 0; i < cards.Length; i++)
            {
                var card = drawCard(cards[i]);
                card.PlayAnimation(this, new CardAnimationEventArgs()
                {
                    AnimationName = "InitDrawCard",
                    EventArgs = new CardPositionEventArgs(cards.Length, i)
                }, i == 0 ? a : null);
            }
        }

        /// <summary>
        /// 抽一张卡
        /// </summary>
        public void DrawCard(CardID cardID, GenericAction callback)
        {
            int originCount = handCards.Count;
            updateHandCardPos(originCount + 1);

            CardFaceViewModel card = drawCard(cardID);
            card.PlayAnimation(this, new CardAnimationEventArgs()
            {
                AnimationName = "DrawCard",
                EventArgs = new CardPositionEventArgs(1, 0) { GroupOffset = originCount }
            }, callback);
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

            int originCount = handCards.Count;
            updateHandCardPos(originCount + cards.Length);

            for (int i = 0; i < cards.Length; i++)
            {
                var card = drawCard(cards[i]);
                card.PlayAnimation(this, new CardAnimationEventArgs()
                {
                    AnimationName = "InitDrawCard",
                    EventArgs = new CardPositionEventArgs(cards.Length, i) { GroupOffset = originCount }
                }, i == 0 ? callback : null);
            }
        }

        private CardFaceViewModel drawCard(CardID cardID)
        {
            var card = Instantiate(cardfacePrefab, cardSpawnRoot);
            card.gameObject.SetActive(true);
            card.CardID = cardID.DefineID;
            card.RuntimeID = cardID.RuntimeID;

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
            // todo: call some function

            throwCard.gameObject.SetActive(false);

            GetComponentInParent<Model.DeckController>()?.InitReplace(SelfID, throwingCards.Select(c => c.RuntimeID).ToArray());
            throwingCards.Clear();
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
                var item = throwingCards[i];
                CardAnimationEventArgs arg = new CardAnimationEventArgs()
                {
                    AnimationName = "CardToCenter",
                    EventArgs = new CardPositionEventArgs() { GroupCount = throwingCards.Count, GroupID = i }
                };
                item.PlayAnimation(this, arg, null);
            }
            updateHandCardPos();
        }

        void updateHandCardPos(int count)
        {
            IGensoukyo.Utilities.DebugUtils.Log("重载卡片位置");
            for (int i = 0; i < handCards.Count; i++)
            {
                CardAnimationEventArgs arg = new CardAnimationEventArgs()
                {
                    AnimationName = "CardToHand",
                    EventArgs = new CardPositionEventArgs() { GroupCount = count, GroupID = i }
                };
                handCards[i].PlayAnimation(this, arg, null);
            }
        }

        void updateHandCardPos()
        {
            updateHandCardPos(handCards.Count);
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
    }
}
