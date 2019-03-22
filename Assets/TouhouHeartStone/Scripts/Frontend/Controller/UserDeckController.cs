using UnityEngine;

using TouhouHeartstone.Frontend.ViewModel;

namespace TouhouHeartstone.Frontend.Controller
{
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


        #region test

        [SerializeField]
        CardImageResources images;

        [SerializeField]
        CardTextResources texts;

        void Start()
        {
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
        }
        #endregion
    }
}
