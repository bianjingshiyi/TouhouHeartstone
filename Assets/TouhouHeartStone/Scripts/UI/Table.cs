using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;
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
        public THHGame game { get; private set; } = null;
        public THHPlayer player { get; private set; } = null;
        partial void onAwake()
        {
            SelfHandList.asButton.onClick.AddListener(() =>
            {
                if (SelfHandList.isExpanded)
                    SelfHandList.shrink();
                else
                    SelfHandList.expand();
            });
        }
        public void setGame(THHGame game, THHPlayer player)
        {
            InitReplaceDialog.hide();
            AttackArrowImage.gameObject.SetActive(false);
            SelfHandList.clearItems();
            SelfFieldList.clearItems();
            EnemyFieldList.clearItems();
            EnemyHandList.clearItems();

            if (game != null)
            {
                game.triggers.onEventBefore -= onEventBefore;
                game.triggers.onEventAfter -= onEventAfter;
            }
            this.game = game;
            if (game != null)
            {
                game.triggers.onEventBefore += onEventBefore;
                game.triggers.onEventAfter += onEventAfter;
            }
            if (player != null)
            {
                SelfSkill.asButton.onClick.RemoveAllListeners();
                TurnEndButton.onClick.RemoveAllListeners();
            }
            this.player = player;
            if (player != null)
            {
                SelfSkill.asButton.onClick.AddListener(() =>
                {
                    if (selectableTargets != null)
                        return;
                    player.cmdUse(game, SelfSkill.card, 0);
                });
                TurnEndButton.onClick.AddListener(() =>
                {
                    player.cmdTurnEnd(game);

                    SelfHandList.stopPlacing();
                    selectableTargets = null;
                });
            }
        }
        [SerializeField]
        List<Animation> _animationQueue = new List<Animation>();
        private void onEventBefore(IEventArg arg)
        {
            switch (arg)
            {
                case THHPlayer.MoveEventArg move:
                    _animationQueue.Add(new MoveServantAnimation(move));
                    break;
                //case THHPlayer.CreateTokenEventArg createToken:
                //    _animationQueue.Add(new CreateTokenAnimation(createToken));
                //    break;
                case THHCard.AttackEventArg attack:
                    _animationQueue.Add(new AttackServantAnimation(attack));
                    break;
                case THHCard.DeathEventArg death:
                    _animationQueue.Add(new DeathAnimation(death));
                    break;
                case THHPlayer.ActiveEventArg active:
                    foreach (var target in active.targets)
                    {
                        if (target is TouhouCardEngine.Card card)
                        {
                            _animationQueue.Add(new SelectTargetAnimation(active));
                        }
                    }
                    break;
                default:
                    //game.logger?.log("UI", "被忽略的事件结束：" + obj);
                    break;
            }
        }
        [SerializeField]
        Projectile _defaultProjectile;
        private void onEventAfter(IEventArg arg)
        {

        }
        protected void Update()
        {
            if (_tipTimer.isExpired())
            {
                _tipTimer.reset();
                TipText.gameObject.SetActive(false);
            }
            else if (_tipTimer.isStarted)
            {
                TipText.color = new Color(TipText.color.r, TipText.color.g, TipText.color.b, 1/*_tipTimer.getRemainedTime() / _tipTimer.duration*/);
            }

            if (game == null)
                return;
            if (game.turnTimer != null && game.turnTimer.remainedTime <= 15)
            {
                TimeoutSlider.display();
                TimeoutSlider.value = game.turnTimer.remainedTime / 15;
            }
            else
                TimeoutSlider.hide();

            if (player == null)
                return;
            SelfMaster.update(this, player, player.master, getSkin(player.master));
            if (player.skill != null)
            {
                SelfSkill.update(this, player, player, player.skill, getSkin(player.skill));
                SelfSkill.display();
            }
            else
                SelfSkill.hide();
            SelfGem.Text.text = player.gem.ToString();
            SelfHandList.updateItems(player.hand.ToArray(), (item, card) => item.Card.card == card, (item, card) =>
            {
                if (item.Card.isDisplaying)
                    item.Card.update(card, getSkin(card));
            });
            SelfHandList.sortItems((a, b) => player.hand.indexOf(a.Card.card) - player.hand.indexOf(b.Card.card));
            foreach (var servant in SelfFieldList)
            {
                servant.update(player, servant.card, getSkin(servant.card));
            }
            if (game.currentPlayer == player)
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
            EnemyMaster.update(this, opponent, opponent.master, getSkin(opponent.master));
            if (opponent.skill != null)
            {
                EnemySkill.update(this, player, opponent, opponent.skill, getSkin(opponent.skill));
                EnemySkill.display();
            }
            else
                EnemySkill.hide();
            EnemyGem.Text.text = opponent.gem.ToString();
            EnemyHandList.updateItems(opponent.hand.ToArray(), (item, card) => item.Card.card == card, (item, card) =>
            {
                item.Card.update(card, null);
            });
            EnemyHandList.sortItems((a, b) => opponent.hand.indexOf(a.Card.card) - opponent.hand.indexOf(b.Card.card));
            foreach (var servant in EnemyFieldList)
            {
                servant.update(opponent, servant.card, getSkin(servant.card));
            }

            IRequest request = game.answers.getLastRequest(player.id);
            if (request is InitReplaceRequest initReplace)
            {
                if (!InitReplaceDialog.isDisplaying)
                {
                    InitReplaceDialog.display();
                    InitReplaceDialog.InitReplaceCardList.clearItems();
                    InitReplaceDialog.InitReplaceCardList.updateItems(player.init, (i, c) => i.Card.card == c, (item, card) =>
                    {
                        item.Card.update(card, getSkin(card));
                        item.MarkImage.enabled = false;
                        item.asButton.onClick.RemoveAllListeners();
                        item.asButton.onClick.AddListener(() =>
                        {
                            item.MarkImage.enabled = !item.MarkImage.enabled;
                        });
                    });
                    InitReplaceDialog.InitReplaceCardList.sortItems((a, b) => player.init.indexOf(a.Card.card) - player.init.indexOf(b.Card.card));
                    InitReplaceDialog.ConfirmButton.interactable = true;
                    InitReplaceDialog.ConfirmButton.GetComponent<Image>().color = Color.white;
                    InitReplaceDialog.ConfirmButton.onClick.RemoveAllListeners();
                    InitReplaceDialog.ConfirmButton.onClick.AddListener(() =>
                    {
                        game.answers.answer(player.id, new InitReplaceResponse()
                        {
                            cardsId = InitReplaceDialog.InitReplaceCardList.Where(item => item.MarkImage.enabled).Select(item => item.Card.card.id).ToArray()
                        });
                        InitReplaceDialog.ConfirmButton.interactable = false;
                        InitReplaceDialog.ConfirmButton.GetComponent<Image>().color = Color.gray;
                    });
                }
            }
            else
            {
                InitReplaceDialog.hide();
            }

            if (_animationQueue.Count > 0)
            {
                Animation animation = _animationQueue[0];
                if (animation.update(this))
                {
                    _animationQueue.RemoveAt(0);
                }
            }
        }
        #region Skin
        public CardSkinData getSkin(TouhouCardEngine.Card card)
        {
            return getSkin(card.define);
        }
        public CardSkinData getSkin(CardDefine define)
        {
            return parent.game.getManager<CardManager>().GetCardSkin(define.id);
        }
        #endregion
        [SerializeField]
        BJSYGameCore.Timer _tipTimer = new BJSYGameCore.Timer();
        public void showTip(string tip)
        {
            TipText.gameObject.SetActive(true);
            TipText.text = tip;
            _tipTimer.start();
        }
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
