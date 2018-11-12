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

        [SerializeField]
        CardType type;

        /// <summary>
        /// 卡片对应的类型ID
        /// </summary>
        public int TypeID => typeID;

        /// <summary>
        /// 卡片的ID
        /// </summary>
        public int InstanceID => instanceID;

        /// <summary>
        /// 卡片当前的状态
        /// </summary>
        public CardState State { get; set; }

        /// <summary>
        /// 卡片的种类
        /// </summary>
        public CardType Type => type;

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
        #region MouseEvent
        public event Action<CardFace> OnClick;
        public event Action<CardFace> OnMouseIn;
        public event Action<CardFace> OnMouseOut;
        public event Action<CardFace> OnDrag;
        public event Action<CardFace> OnRelease;

        private void OnMouseUpAsButton()
        {
            OnClick?.Invoke(this);
        }

        bool isDraging = false;
        bool checking = false;
        Vector2 lastMousePos;

        private void OnMouseDrag()
        {
            if (!isDraging)
            {
                if (checking)
                {
                    if (Vector2.Distance(lastMousePos, Input.mousePosition) > 4)
                    {
                        isDraging = true;
                        Debug.Log("Drag");
                        OnDrag?.Invoke(this);
                    }
                }
                else
                {
                    lastMousePos = Input.mousePosition;
                    checking = true;
                }
            }
        }

        private void OnMouseUp()
        {
            if (isDraging)
            {
                isDraging = false;
                checking = false;
                Debug.Log("Release");
                OnRelease?.Invoke(this);
            }
        }

        private void OnMouseEnter()
        {
            OnMouseIn?.Invoke(this);
        }

        private void OnMouseExit()
        {
            OnMouseOut?.Invoke(this);
        }
        #endregion

        public override string ToString()
        {
            return $"Cardface: {instanceID}";
        }

        /// <summary>
        /// 使用这张卡
        /// </summary>
        /// <param name="target"></param>
        public void Use(object target = null)
        {
            Debug.Log($"Use card {instanceID}, target: {target}");
            gameObject.SetActive(false);
        }
    }

    public enum CardState
    {
        /// <summary>
        /// 在牌堆中
        /// </summary>
        Stack,
        /// <summary>
        /// 正在抽卡
        /// </summary>
        Drawing,
        /// <summary>
        /// 手牌上
        /// </summary>
        Hand,
        /// <summary>
        /// 指上去的详细信息
        /// </summary>
        Active,
        /// <summary>
        /// 拿起来
        /// </summary>
        Pickup,
        /// <summary>
        /// 销毁
        /// </summary>
        Destory
    }

    public enum CardType
    {
        /// <summary>
        /// 无目标的法术卡
        /// </summary>
        DriftlessSpell,
        /// <summary>
        /// 有目标的法术卡
        /// </summary>
        DirectedSpell,
        /// <summary>
        /// 无目标的实体卡
        /// </summary>
        Entity,
        /// <summary>
        /// 有目标的实体卡
        /// </summary>
        DirectedEntity,
    }
}
