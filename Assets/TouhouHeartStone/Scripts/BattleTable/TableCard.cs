using System;
using Newtonsoft.Json.Linq;
using TouhouHeartstone;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;

namespace Game.BattleTable
{
    public class TableCard : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public void setVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
        public void animateFaceup(bool faceup)
        {
            cardBack.SetActive(!faceup);
            cardFront.SetActive(faceup);
        }
        public void setFaceup(bool faceup)
        {
            animator.SetBool("isFaceUp", faceup);
        }
        public void setType(string type)
        {
            servantCardFace.setVisible(type == DEFINE.SERVANT || type == DEFINE.MASTER);
            spellCardFace.setVisible(type == DEFINE.SPELL || type == DEFINE.SKILL);
            itemCardFace.setVisible(type == DEFINE.ITEM);
        }
        public PropertyNumber getCostPropNum(string cardType)
        {
            switch (cardType)
            {
                case DEFINE.SERVANT:
                case DEFINE.MASTER:
                    return servantCardFace.costPropNumber;
                case DEFINE.SPELL:
                case DEFINE.SKILL:
                    return spellCardFace.costPropNumber;
                case DEFINE.ITEM:
                    return itemCardFace.costPropNumber;
            }
            return null;
        }
        public void setPreheat(bool preheat)
        {
            _preheatEffectFaceObj.SetActive(preheat);
            _preheatEffectBackObj.SetActive(preheat);
        }
        public void setTag(string name)
        {
            servantCardFace.setTag(name);
        }
        public void setName(string name)
        {
            servantCardFace.nameText.text = name;
            spellCardFace.nameText.text = name;
            itemCardFace.nameText.text = name;
        }
        public void setSubname(string name)
        {
            servantCardFace.subnameText.text = name;
        }
        public void setDesc(string desc)
        {
            servantCardFace.descText.text = desc;
            spellCardFace.descText.text = desc;
            itemCardFace.descText.text = desc;
        }
        public void setCost(int value)
        {
            servantCardFace.costPropNumber.text = value.ToString();
            spellCardFace.costPropNumber.text = value.ToString();
            itemCardFace.costPropNumber.text = value.ToString();
        }
        public void setCostColor(PropertyNumberColor color)
        {
            servantCardFace.costPropNumber.setTextColor(color);
            spellCardFace.costPropNumber.setTextColor(color);
            itemCardFace.costPropNumber.setTextColor(color);
        }
        public void setAttack(int value)
        {
            servantCardFace.attackPropNumber.text = value.ToString();
            itemCardFace.attackPropNumber.text = value.ToString();
        }
        public void setAttackColor(PropertyNumberColor color)
        {
            servantCardFace.attackPropNumber.setTextColor(color);
            itemCardFace.attackPropNumber.setTextColor(color);
        }
        public void setLife(int value)
        {
            servantCardFace.lifePropNumber.text = value.ToString();
            itemCardFace.lifePropNumber.text = value.ToString();
        }
        public void setLifeColor(PropertyNumberColor color)
        {
            servantCardFace.lifePropNumber.setTextColor(color);
            itemCardFace.lifePropNumber.setTextColor(color);
        }
        public void setImageSprite(Sprite image)
        {
            // TODO:_imageRenderer.SetTexture(image);
        }
        public void destroy()
        {
            Destroy(gameObject);
        }

        #region 事件回调
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke(this, eventData);
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            onBeginDrag?.Invoke(this, eventData);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            onDrag?.Invoke(this, eventData);
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            onEndDrag?.Invoke(this, eventData);
        }
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            onPointerEnter?.Invoke(this, eventData);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            onPointerExit?.Invoke(this, eventData);
        }
        #endregion

        #region 事件
        public event Action<TableCard, PointerEventData> onClick;
        public event Action<TableCard, PointerEventData> onBeginDrag;
        public event Action<TableCard, PointerEventData> onDrag;
        public event Action<TableCard, PointerEventData> onEndDrag;
        public event Action<TableCard, PointerEventData> onPointerEnter;
        public event Action<TableCard, PointerEventData> onPointerExit;
        #endregion

        #region 属性字段
        public SimpleAnim onShowAnim;
        public Animator animator => animator;
        public float width => _width;
        public float height => _height;
        public string flipToFaceAnimName;
        public string notifyAnimName;
        public string flipToBackAnimName;

        [SerializeField]
        private float _width;
        [SerializeField]
        private float _height;
        [SerializeField]
        private GameObject _preheatEffectFaceObj;
        [SerializeField]
        private GameObject _preheatEffectBackObj;
        [SerializeField]
        private Animator _animator;
        [SerializeField]
        private GameObject cardBack;
        [SerializeField]
        private GameObject cardFront;
        [SerializeField]
        private ServantCardFace servantCardFace;
        [SerializeField]
        private SpellCardFace spellCardFace;
        [SerializeField]
        private ItemCardFace itemCardFace;
        #endregion
    }
}