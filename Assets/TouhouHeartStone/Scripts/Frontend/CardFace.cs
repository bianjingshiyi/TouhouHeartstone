using UnityEngine;
using UnityEngine.UI;

using System;

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

        [SerializeField]
        CardAnimationController aniController;

        [NonSerialized]
        int instanceID;

        [SerializeField]
        int typeID;

        /// <summary>
        /// 卡片对应的类型ID
        /// </summary>
        public int TypeID => typeID;

        /// <summary>
        /// 卡片的ID
        /// </summary>
        public int InstanceID => instanceID;

        /// <summary>
        /// 设置卡片的唯一指定id
        /// </summary>
        /// <remarks>
        /// 337845818
        /// </remarks>
        /// <param name="id"></param>
        public void SetInstanceID(int id)
        {
            instanceID = id;
        }

        /// <summary>
        /// 卡片动画控制器
        /// </summary>
        public CardAnimationController CardAniController => aniController;

        private void Awake()
        {
            aniController = aniController ?? GetComponent<CardAnimationController>();
        }

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
