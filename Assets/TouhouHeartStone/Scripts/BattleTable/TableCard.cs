using System;
using TouhouHeartstone;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.BattleTable
{
    public class TableCard : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public void setVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
        public void setFaceup(bool faceup)
        {
            cardBack.SetActive(!faceup);
            cardFront.SetActive(faceup);
        }
        public void setType(string type)
        {
            servantCardFace.setVisible(type == DEFINE.SERVANT || type == DEFINE.MASTER);
            spellCardFace.setVisible(type == DEFINE.SPELL || type == DEFINE.SKILL);
            itemCardFace.setVisible(type == DEFINE.ITEM);
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