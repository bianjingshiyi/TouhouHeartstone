using TouhouHeartstone.Frontend.View.Animation;
using TouhouHeartstone.Frontend.ViewModel;
using UnityEngine;

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

        /// <summary>
        /// 初始抽卡
        /// </summary>
        /// <param name="cards"></param>
        public void InitDraw(int[] cards)
        {
            GenericAction a = (evt, arg) => { ThrowCard(0, 0); };

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
        public void DrawCard(int cardID, GenericAction callback)
        {
            CardFaceViewModel card = drawCard(cardID);
            card.PlayAnimation(this, new CardAnimationEventArgs()
            {
                AnimationName = "DrawCard",
                EventArgs = new CardPositionEventArgs(1, 0)
            }, callback);
        }

        /// <summary>
        /// 抽多张卡
        /// </summary>
        /// <param name="cards"></param>
        /// <param name="callback"></param>
        public void DrawCard(int[] cards, GenericAction callback)
        {
            for (int i = 0; i < cards.Length; i++)
            {
                var card = drawCard(cards[i]);
                card.PlayAnimation(this, new CardAnimationEventArgs()
                {
                    AnimationName = "InitDrawCard",
                    EventArgs = new CardPositionEventArgs(cards.Length, i)
                }, i == 0 ? callback : null);
            }
        }

        private CardFaceViewModel drawCard(int cardID)
        {
            var card = Instantiate(cardfacePrefab, cardSpawnRoot);
            card.gameObject.SetActive(true);
            card.CardID = cardID;
            return card;
        }

        #region test
        void Start()
        {
            #region test_data

            crystalBar.CrystalTotal = 5;
            crystalBar.CrystalHighlight = 2;
            crystalBar.CrystalUsed = 1;
            crystalBar.CrystalDisable = 1;

            characterInfo.Character = new Model.CharacterData() { Atk = 3, Defence = 4, HP = 9 };
            cardStackLibrary.CardCount = 30;
            cardStackGrave.CardCount = 2;
            #endregion

            cardfacePrefab.gameObject.SetActive(false);
            throwCard.gameObject.SetActive(false);

            throwCard.OnThrow += ThrowCard_OnThrow;
        }

        private void ThrowCard_OnThrow()
        {
            throw new System.NotImplementedException();
        }
        #endregion

        private int _SelfID;
        public int SelfID => _SelfID;

        /// <summary>
        /// 设置自己的ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="character"></param>
        public void Init(int id, int character, bool isSelf)
        {
            if (!isSelf)
                transform.localRotation = Quaternion.Euler(0, 0, 180);
            _SelfID = id;

            // todo: 设置角色图像
        }

        public void ThrowCard(int min, int max)
        {
            throwCard.gameObject.SetActive(true);
            throwCard.Max = max;
            throwCard.Min = min;
        }

        public bool ThrowingCard => throwCard.gameObject.activeSelf;
    }
}
