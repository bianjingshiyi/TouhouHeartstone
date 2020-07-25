using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TouhouHeartstone;
using BJSYGameCore;
using TouhouCardEngine.Interfaces;
using BJSYGameCore.UI;
using TouhouCardEngine;
using TouhouHeartstone.Builtin;
using Game;
namespace UI
{
    partial class Table
    {
        [Obsolete]
        public THHGame game
        {
            get { return ui.getManager<TableManager>().game; }
        }
        [Obsolete]
        public THHPlayer player
        {
            get { return ui.getManager<TableManager>().player; }
        }
        partial void onAwake()
        {
        }
        public void setGame(THHGame game, THHPlayer player)
        {
            InitReplaceDialog.hide();
            TurnTipImage.hide();
            SelfHandList.clearItems();
            SelfFieldList.clearItems();
            EnemyFieldList.clearItems();
            EnemyHandList.clearItems();
            AttackArrowImage.hide();
            Fatigue.hide();
        }
        [Obsolete]
        public bool canControl
        {
            get { return ui.getManager<TableManager>().canControl; }
            set { ui.getManager<TableManager>().canControl = value; }
        }
        [SerializeField]
        Projectile _defaultProjectile;
        protected void Update()
        {

            if (player == null)
                return;

            if (canControl)
            {
                TurnEndButton.interactable = true;
                TurnEndButton.GetComponent<Image>().color = Color.white;
            }
            else
            {
                TurnEndButton.interactable = false;
                TurnEndButton.GetComponent<Image>().color = Color.gray;
            }

            THHPlayer opponent = game.getOpponent(player);
            if (opponent == null)
                return;
        }
        public void clickNoWhere(PointerEventData pointer)
        {
            onClickNoWhere.invoke(this, pointer);
        }
        public ActionEvent<Table, PointerEventData> onClickNoWhere { get; } = new ActionEvent<Table, PointerEventData>();
        #region Skin
        public CardSkinData getSkin(TouhouCardEngine.Card card)
        {
            return getSkin(card.define);
        }
        public CardSkinData getSkin(CardDefine define)
        {
            return parent.game.getManager<CardManager>().getSkin(define.id);
        }
        #endregion
        #region Use
        [SerializeField]
        HandListItem _usingHand;
        public HandListItem usingHand
        {
            get { return _usingHand; }
            set { _usingHand = value; }
        }
        TouhouCardEngine.Card _usingCard;
        [SerializeField]
        int _usingPosition;
        [SerializeField]
        bool _isSelectingTarget = false;
        public bool isSelectingTarget
        {
            get { return _isSelectingTarget; }
            set { _isSelectingTarget = value; }
        }
        private void resetUse(bool resetItem, bool resetPlaceHolder)
        {
            if (usingHand != null && resetItem)
            {
                usingHand.Card.display();
                usingHand.Card.rectTransform.localScale = Vector3.one;
                usingHand.Card.rectTransform.localPosition = Vector2.zero;
            }
            if (resetPlaceHolder)
                hideServantPlaceHolder();
            isSelectingTarget = false;
            selectableTargets = null;
        }
        private void hideServantPlaceHolder()
        {
            addChild(ServantPlaceHolder.rectTransform);
            ServantPlaceHolder.Servant.hide();
            ServantPlaceHolder.hide();
        }
        #endregion
        public Servant getServant(TouhouCardEngine.Card card)
        {
            foreach (var item in SelfFieldList)
            {
                if (item.card == card)
                    return item;
            }
            foreach (var item in EnemyFieldList)
            {
                if (item.card == card)
                    return item;
            }
            return null;
        }
        public Master getMaster(TouhouCardEngine.Card card)
        {
            if (SelfMaster.card == card)
                return SelfMaster;
            else if (EnemyMaster.card == card)
                return EnemyMaster;
            else
                return null;
        }
        public UIObject getCharacter(TouhouCardEngine.Card card)
        {
            Master master = getMaster(card);
            if (master == null)
                return getServant(card);
            return master;
        }
        public UIObject[] getCharacters(TouhouCardEngine.Card[] targets)
        {
            return targets.Select(target =>
            {
                if (getMaster(target) is Master master)
                    return master as UIObject;
                else if (getServant(target) is Servant servant)
                    return servant as UIObject;
                throw new ActorNotFoundException(target);
            }).ToArray();
        }
        [SerializeField]
        UIObject[] _selectableTargets = null;
        public UIObject[] selectableTargets
        {
            get { return _selectableTargets; }
            set { _selectableTargets = value; }
        }
    }
}
