using UnityEngine;
using UnityEngine.UI;

namespace TouhouHeartstone.Frontend
{
    public class CardFace : MonoBehaviour
    {
        [SerializeField]
        TextMeshExtend nameLabel;

        [SerializeField]
        TextMeshExtend descLabel;

        [SerializeField]
        TextMeshExtend costLabel;

        [SerializeField]
        TextMeshExtend lifeLabel;

        [SerializeField]
        TextMeshExtend attackLabel;

        [SerializeField]
        Image headImage;

        private void Start()
        {
            nameLabel.text = "女仆妖精";
            descLabel.text = "嘲讽";

            costLabel.text = "1";
            lifeLabel.text = "1";
            attackLabel.text = "1";
        }
    }
}