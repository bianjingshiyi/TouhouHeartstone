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

        /// <summary>
        /// 通用的抽卡
        /// </summary>
        public void DrawCard(GenericAction callback)
        {
            var card = Instantiate(cardfacePrefab, cardSpawnRoot);
            // card.gameObject.SetActive(false);

            card.PlayAnimation(this, new CardAnimationEventArgs()
            {
                AnimationName = "DrawCard",
                EventArgs = new CardPositionEventArgs(1, 0)
            }, callback);
        }

        #region test

        [SerializeField]
        CardImageResources images;

        [SerializeField]
        CardTextResources texts;

        void Start()
        {
            #region test_data
            cardfacePrefab.CardSpec.Cost = Random.Range(0, 10);
            cardfacePrefab.CardSpec.HP = Random.Range(0, 10);
            cardfacePrefab.CardSpec.Atk = Random.Range(0, 10);
            cardfacePrefab.ImageResource = images.Get("0", "zh-CN");
            cardfacePrefab.TextResource = texts.Get("0", "zh-CN");

            crystalBar.CrystalTotal = 5;
            crystalBar.CrystalHighlight = 2;
            crystalBar.CrystalUsed = 1;
            crystalBar.CrystalDisable = 1;

            characterInfo.Character = new Model.CharacterData() { Atk = 3, Defence = 4, HP = 9 };
            cardStackLibrary.CardCount = 30;
            cardStackGrave.CardCount = 2;
            #endregion

            DrawCard(null);
        }
        #endregion
    }

}
