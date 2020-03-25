using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;
using TouhouHeartstone;
using TouhouCardEngine.Interfaces;

namespace UI
{
    partial class Table
    {
        [SerializeField]
        CardSkinData[] _skins = new CardSkinData[0];
        public THHGame game { get; private set; } = null;
        public THHPlayer player { get; private set; } = null;
        Dictionary<int, CardSkinData> skinDic { get; } = new Dictionary<int, CardSkinData>();
        partial void onAwake()
        {
            skinDic.Clear();
            foreach (CardSkinData skin in _skins)
            {
                skinDic.Add(skin.id, skin);
            }

            InitReplaceDialog.hide();
        }
        public void setGame(THHGame game, THHPlayer player)
        {
            if (game != null)
            {
            }
            this.game = game;
            if (game != null)
            {
            }
            this.player = player;
        }
        protected void Update()
        {
            if (game == null)
                return;

            if (player == null)
                return;
            SelfMaster.update(player.master, getSkin(player.master));
            if (player.skill != null)
            {
                SelfSkill.update(player.skill, getSkin(player.skill));
                SelfSkill.display();
            }
            else
                SelfSkill.hide();
            SelfGem.Text.text = player.gem.ToString();
            SelfHandList.updateItems(player.hand, (item, card) => item.card == card, (item, card) =>
            {
                item.update(card, getSkin(card));
            });
            SelfHandList.sortItems((a, b) => player.hand.indexOf(a.card) - player.hand.indexOf(b.card));

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

            THHPlayer opponent = game.getOpponent(player);
            if (opponent == null)
                return;
            EnemyMaster.update(opponent.master, getSkin(opponent.master));
            if (opponent.skill != null)
            {
                EnemySkill.update(opponent.skill, getSkin(opponent.skill));
                EnemySkill.display();
            }
            else
                EnemySkill.hide();
            EnemyGem.Text.text = opponent.gem.ToString();
            EnemyHandList.updateItems(opponent.hand, (item, card) => item.card == card, (item, card) =>
            {
                item.update(card, null);
            });
        }
        CardSkinData getSkin(TouhouCardEngine.Card card)
        {
            return skinDic.ContainsKey(card.define.id) ? skinDic[card.define.id] : null;
        }
    }
}
