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
namespace UI
{
    partial class Table
    {
        [SerializeField]
        CardSkinData[] _skins = new CardSkinData[0];
        public THHGame game { get; private set; } = null;
        public THHPlayer player { get; private set; } = null;
        Dictionary<int, CardSkinData> skinDic { get; } = new Dictionary<int, CardSkinData>();
        [SerializeField]
        string _defaultImagePath;
        [SerializeField]
        Sprite _defaultImage;
        partial void onAwake()
        {
            if (File.Exists(Application.streamingAssetsPath + "/" + _defaultImagePath))
            {
                using (FileStream stream = new FileStream(Application.streamingAssetsPath + "/" + _defaultImagePath, FileMode.Open))
                {
                    Texture2D texture = new Texture2D(512, 512);
                    byte[] bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, (int)stream.Length);
                    texture.LoadImage(bytes);
                    _defaultImage = Sprite.Create(texture, new Rect(0, 0, 512, 512), new Vector2(.5f, .5f), 100);
                }
            }
            skinDic.Clear();
            foreach (CardSkinData skin in _skins)
            {
                if (skin.image == null)
                    skin.image = _defaultImage;
                skinDic.Add(skin.id, skin);
            }

            InitReplaceDialog.hide();
            SelfHandList.asButton.onClick.AddListener(() =>
            {
                if (SelfHandList.isExpanded)
                    SelfHandList.shrink();
                else
                    SelfHandList.expand();
            });
            AttackArrowImage.gameObject.SetActive(false);
        }
        public void setGame(THHGame game, THHPlayer player)
        {
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
                    player.cmdUse(game, SelfSkill.card, 0);
                });
                TurnEndButton.onClick.AddListener(() =>
                {
                    player.cmdTurnEnd(game);
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
                default:
                    //game.logger?.log("UI", "被忽略的事件结束：" + obj);
                    break;
            }
        }
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

            if (player == null)
                return;
            SelfMaster.update(player.master, getSkin(player.master));
            if (player.skill != null)
            {
                SelfSkill.update(game, player, player, player.skill, getSkin(player.skill));
                SelfSkill.display();
            }
            else
                SelfSkill.hide();
            SelfGem.Text.text = player.gem.ToString();
            SelfHandList.updateItems(player.hand.ToArray(), (item, card) => item.Card.card == card, (item, card) =>
            {
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
            EnemyMaster.update(opponent.master, getSkin(opponent.master));
            if (opponent.skill != null)
            {
                EnemySkill.update(game, player, opponent, opponent.skill, getSkin(opponent.skill));
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
            return skinDic.ContainsKey(card.define.id) ? skinDic[card.define.id] : null;
        }
        public CardSkinData getSkin(CardDefine define)
        {
            if (skinDic.Count != _skins.Length)
            {
                skinDic.Clear();
                foreach (CardSkinData skin in _skins)
                {
                    skin.image = _defaultImage;
                    skinDic.Add(skin.id, skin);
                }
            }
            return skinDic.ContainsKey(define.id) ? skinDic[define.id] : null;
        }
        public void addSkins(CardSkinData[] skins)
        {
            foreach (var skin in _skins)
            {
                if (!skinDic.ContainsKey(skin.id))
                    skinDic.Add(skin.id, skin);
            }
            foreach (var skin in skins)
            {
                if (!skinDic.ContainsKey(skin.id))
                    skinDic.Add(skin.id, skin);
            }
            _skins = skinDic.Values.ToArray();
        }
        #endregion
        [SerializeField]
        Timer _tipTimer = new Timer();
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
    }
}
