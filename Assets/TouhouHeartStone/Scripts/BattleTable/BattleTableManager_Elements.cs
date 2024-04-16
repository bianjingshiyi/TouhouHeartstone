using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BJSYGameCore;
using Game.Animations;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using Card = TouhouCardEngine.Card;
using UICard = Game.BattleTable.TableCard;
using Timer = BJSYGameCore.Timer;

namespace Game.BattleTable
{
    public partial class BattleTableManager
    {
        #region 公有方法

        /// <summary>
        /// 设置所有卡牌的控制绿光需要更新。
        /// </summary>
        /// <param name="card">卡牌。</param>
        public void setAllCardControlDirty() => _allControlDirty = true;
        /// <summary>
        /// 设置某张卡牌的控制绿光需要更新。
        /// </summary>
        /// <param name="card">卡牌。</param>
        public void setCardControlDirty(ICardData card)
        {
            if (card == null)
                return;
            setCardControlDirty(card.id);
        }
        /// <summary>
        /// 设置某张卡牌的控制绿光需要更新。
        /// </summary>
        /// <param name="cardId">卡牌ID。</param>
        public void setCardControlDirty(int cardId)
        {
            _controlDirtyCards.Add(cardId);
        }
        /// <summary>
        /// 根据卡牌快照和指定的事件记录，更新某张卡牌。
        /// </summary>
        /// <param name="card">卡牌快照。</param>
        /// <param name="record">事件记录。用于更新道具的同时更新英雄。</param>
        public void updateCardBySnapshot(CardSnapshot card)
        {
            if (tryGetServant(card, out var servant))
            {
                setServant(servant, card);
            }
            else if (tryGetMaster(card, out var master))
            {
                setMaster(master, card);
            }
            else if (tryGetSecret(card, out var secret))
            {
                setSecret(secret, card);
            }
            else if (tryGetItem(card, out var item))
            {
                setItem(item, card);
            }
            else if (tryGetSkill(card, out var skill))
            {
                setSkill(skill, card);
            }
            else if (tryGetHand(card, out var hand))
            {
                setCard(hand.card, card);
            }
            else if (tryGetMovingCard(card, out var movingCard))
            {
                setCard(movingCard, card);
            }
            else if (tryGetDeckOfCard(card, out Deck deck))
            {
                updateCardDescriptor(card);
                updateDeckPanel(deck);
            }
            else if (tryGetGraveOfCard(card, out Deck grave))
            {
                updateCardDescriptor(card);
                updateDeckPanel(grave);
            }
        }
        /// <summary>
        /// 将使用卡牌状态重置到什么也没做的状态。
        /// </summary>
        /// <param name="resetHandItem">是否重置手牌</param>
        /// <param name="resetServantPreview">是否重置随从占位</param>
        /// <param name="finishSelectTarget">是否要完成目标选择而非取消目标选择</param>
        public void resetUse(bool resetHandItem = true, bool resetServantPreview = true, bool finishSelectTarget = false)
        {
            if (resetHandItem)
            {
                resetHand(usingCard);
            }
            if (resetServantPreview)
            {
                table.SelfField.setPreviewPosition(-1);
                _servantPreviewAnimDict.Clear();
            }
            usingCard = null;
            if (finishSelectTarget)
                finishTargetSelect();
            else
                cancelTargetSelect();
        }

        public void startLocalTurn()
        {
            isLocalTurn = true;
            setAllCardControlDirty();
        }
        public void endLocalTurn()
        {
            isLocalTurn = false;
            cancelTargetSelect();
        }
        #region 英雄
        /// <summary>
        /// 初始化Master的事件
        /// </summary>
        /// <param name="master"></param>

        public void initMaster(Master master)
        {
            master.setLifeColor(PropertyNumberColor.White);
            master.setShackle(false);
            master.setStealth(false);
            master.setHighlight(false);
            master.setAttackDisplay(false);
            master.setAttack(0);
            master.setArmorDisplay(false);
            master.reset();
        }
        /// <summary>
        /// 尝试获取一张卡对应的Master
        /// </summary>
        /// <param name="card"></param>
        /// <param name="master"></param>
        /// <returns></returns>
        public bool tryGetMaster(ICardData card, out Master master) => tryGetMaster(card?.id ?? 0, out master);
        /// <summary>
        /// 尝试获取一张卡对应的Master
        /// </summary>
        /// <param name="card"></param>
        /// <param name="master"></param>
        /// <returns></returns>
        public bool tryGetMaster(int cardId, out Master master)
        {
            if (player.getMaster().id == cardId)
            {
                master = table.SelfMaster;
                return true;
            }
            else if (game.getOpponent(player).getMaster().id == cardId)
            {
                master = table.EnemyMaster;
                return true;
            }
            else
            {
                master = null;
                return false;
            }
        }
        public int getMasterItemAttack(ICardData card) => getMasterItemAttack(card.id);
        public int getMasterItemAttack(int cardId) => getMasterItemAttack(getCardDescriptor(cardId));
        public int getMasterItemAttack(CardDescriptor masterDesc)
        {
            var masterOwner = masterDesc.getOwner();
            if (currentAnimPlayer == masterOwner)
            {
                var itemDesc = masterOwner == player ? _selfItemDesc : _enemyItemDesc;
                return itemDesc?.getAttack() ?? 0;
            }
            return 0;
        }
        /// <summary>
        /// 设置Master的UI
        /// </summary>
        /// <param name="master"></param>
        /// <param name="card"></param>
        /// <param name="isSelectable"></param>
        public void setMaster(Master master, CardSnapshot card) => setMaster(master, updateCardDescriptor(card));
        /// <summary>
        /// 设置Master的UI
        /// </summary>
        /// <param name="master"></param>
        /// <param name="desc"></param>
        /// <param name="isSelectable"></param>
        public async void setMaster(Master master, CardDescriptor desc)
        {
            var curLife = desc.getCurrentLife();
            var life = desc.getLife();
            master.setLife(curLife);
            var lifeColor = curLife < life ? PropertyNumberColor.Red : life > desc.define.getLife() ? PropertyNumberColor.Green : PropertyNumberColor.White;
            master.setLifeColor(lifeColor);

            var attack = desc.getAttack();
            master.setAttackDisplay(attack > 0);
            master.setAttack(attack);
            var atkColor = attack > desc.define.getAttack() + getMasterItemAttack(desc) ? PropertyNumberColor.Green : PropertyNumberColor.White;
            master.setAttackColor(atkColor);

            var armor = desc.getArmor();
            master.setArmorDisplay(armor > 0);
            master.setArmor(armor);

            master.setShield(desc.hasShield);
            master.setStealth(desc.isStealth);
            master.setShackle(desc.isFrozen);
            master.setImageSprite(await loadCardImage(desc));
            setCardControlDirty(desc.id);
        }
        /// <summary>
        /// 从Master获取对应的牌
        /// </summary>
        /// <param name="master"></param>
        /// <returns></returns>
        public Card getCard(Master master)
        {
            if (master == table.SelfMaster)
                return player.getMaster();
            else
                return game.getOpponent(player).getMaster();
        }
        #endregion 英雄

        #region 技能
        public bool tryGetSkill(ICardData card, out Skill skill) => tryGetSkill(card?.id ?? 0, out skill);
        public bool tryGetSkill(int cardId, out Skill skill)
        {
            if (cardId == player.getSkill()?.id)
            {
                skill = table.SelfSkill;
                return true;
            }
            else if (cardId == game.getOpponent(player).getSkill()?.id)
            {
                skill = table.EnemySkill;
                return true;
            }
            else
            {
                skill = null;
                return false;
            }
        }
        public void setSkill(Skill skill, CardSnapshot card) => setSkill(skill, updateCardDescriptor(card));
        /// <summary>
        /// 设置Skill的UI
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="desc"></param>
        public async void setSkill(Skill skill, CardDescriptor desc)
        {
            skill.setOpen(!desc.isUsed);
            var cost = desc.getCost();
            var defineCost = desc.define.getCost();
            var skillColor = cost > defineCost ? PropertyNumberColor.Red : cost < defineCost ? PropertyNumberColor.Green : PropertyNumberColor.White;
            skill.setCost(cost);
            skill.setCostVisible(!desc.define.hasKeyword(KEYWORD.UNPLAYABLE));
            skill.setUsable(!desc.isModifiedUnplayable);
            skill.costNumber.setTextColor(skillColor);

            skill.setImageSprite(await loadCardImage(desc));

            setCardControlDirty(desc.id);
        }
        public Card getSkillCard(Skill skill)
        {
            if (skill == table.SelfSkill)
            {
                return player.getSkill();
            }
            else if (skill == table.EnemySkill)
            {
                return game.getOpponent(player).getSkill();
            }
            return null;
        }
        #endregion

        #region 道具
        public bool tryGetItem(ICardData card, out Item item) => tryGetItem(card?.id ?? 0, out item);
        public bool tryGetItem(int cardId, out Item item)
        {
            if (_selfItemDesc?.id == cardId)
            {
                item = table.SelfItem;
                return true;
            }
            else if (_enemyItemDesc?.id == cardId)
            {
                item = table.EnemyItem;
                return true;
            }
            else
            {
                item = null;
                return false;
            }
        }
        public void setItem(Item item, CardSnapshot card) => setItem(item, updateCardDescriptor(card));
        public async void setItem(Item item, CardDescriptor desc)
        {
            if (desc == null)
            {
                removeItem(desc.id);
            }
            item.setExist(true);
            if (item == table.SelfItem)
                _selfItemDesc = desc;
            else if (item == table.EnemyItem)
                _enemyItemDesc = desc;

            item.setOpen(desc.getOwner() == currentAnimPlayer);
            item.setLethal(desc.isLethal);
            item.setLifeDrain(desc.isDrain);
            item.setLastword(desc.hasDeathRattle);
            int attack = desc.getAttack();
            item.setAttack(attack);
            item.setAttackColor(attack > desc.define.getAttack() ? PropertyNumberColor.Green : PropertyNumberColor.White);

            int life = desc.getCurrentLife();
            item.setLife(life);
            item.setLifeColor(life == desc.getLife() ? (life > desc.define.getLife() ? PropertyNumberColor.Green : PropertyNumberColor.White) : PropertyNumberColor.Red);

            item.setImageSprite(await loadCardImage(desc));
        }
        public bool removeItem(ICardData card) => removeItem(card.id);
        public bool removeItem(int id)
        {
            if (tryGetItem(id, out Item item))
            {
                item.setExist(false);
                if (item == table.SelfItem)
                    _selfItemDesc = null;
                else if (item == table.EnemyItem)
                    _enemyItemDesc = null;
                return true;
            }
            return false;
        }
        public Item getItem(Pile pile)
        {
            return pile?.owner == player ? table.SelfItem : table.EnemyItem;
        }
        public Item[] getAllItems()
        {
            return new Item[]
            {
                table.SelfItem,
                table.EnemyItem,
            };
        }
        public Card getCardByItem(Item item)
        {
            if (item == table.SelfItem)
            {
                if (_selfItemDesc != null)
                    return game.getCard(_selfItemDesc.id);
            }
            else if (item == table.EnemyItem)
            {
                if (_enemyItemDesc != null)
                    return game.getCard(_enemyItemDesc.id);
            }
            return null;
        }
        #endregion 道具

        #region 手牌
        /// <summary>
        /// 创建一张手牌
        /// </summary>
        /// <param name="desc"></param>
        /// <param name="position">默认为-1也就是最右手</param>
        /// <returns></returns>
        public HandItem createHand(ICardData card, int position = -1) => createHand(updateCardDescriptor(card), position);
        /// <summary>
        /// 创建一张手牌
        /// </summary>
        /// <param name="desc"></param>
        /// <param name="position">默认为-1也就是最右手</param>
        /// <returns></returns>
        public HandItem createHand(CardDescriptor desc, int position = -1)
        {
            return createHand(desc, desc.getOwner(), position);
        }
        /// <summary>
        /// 创建一张手牌
        /// </summary>
        /// <param name="desc"></param>
        /// <param name="position">默认为-1也就是最右手</param>
        /// <returns></returns>
        public HandItem createHand(CardDescriptor desc, Player owner, int position = -1)
        {
            if (tryGetHand(desc.id, out var hand))
            {
                UberDebug.LogErrorChannel("UI", $"手牌中已经存在{desc}对应UI{hand}");
                return hand;
            }
            UberDebug.LogTraceChannel("UI", $"创建手牌UI：{desc}");
            HandItem item;
            bool selfHand = owner == player;
            HandList handList = selfHand ? table.SelfHandList : table.EnemyHandList;
            if (position < 0)
            {
                item = handList.addHand();
            }
            else
            {
                item = handList.insertHand(position);
            }
            item.isDraggable = selfHand;
            setCardDesc(item.card, desc);
            item.card.setFaceup(canSeeCard(desc));

            var skin = getCardSkin(desc.define);
            item.gameObject.name = $"{skin?.name ?? "Hand"}<{desc.id}>";
            cardHandDic.Add(desc.id, item);
            return item;
        }

        /// <summary>
        /// 获取UI手牌引用
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public HandItem getHand(ICardData card) => getHand(card.id);
        /// <summary>
        /// 尝试获取UI手牌引用
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public bool tryGetHand(ICardData card, out HandItem hand) => tryGetHand(card?.id ?? 0, out hand);

        /// <summary>
        /// 移除一个UI手牌。
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public bool removeHand(ICardData card) => removeHand(card.id);
        /// <summary>
        /// 获取UI手牌引用
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        public HandItem getHand(int cardId)
        {
            if (cardHandDic.ContainsKey(cardId))
                return cardHandDic[cardId];
            throw new ActorNotFoundException(game.getCard(cardId));
        }
        /// <summary>
        /// 尝试获取UI手牌引用
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public bool tryGetHand(int id, out HandItem hand)
        {
            if (cardHandDic.ContainsKey(id))
            {
                if (cardHandDic[id] != null)
                {
                    hand = cardHandDic[id];
                    return true;
                }
                else
                    cardHandDic.Remove(id);
            }
            hand = null;
            return false;
        }

        /// <summary>
        /// 移除一个UI手牌。
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public bool removeHand(int id)
        {
            if (cardHandDic.ContainsKey(id))
            {
                var item = cardHandDic[id];
                cardHandDic.Remove(id);

                bool selfRemoved = table.SelfHandList.removeHand(item);
                bool enemyRemoved = table.EnemyHandList.removeHand(item);
                return selfRemoved || enemyRemoved;
            }
            return false;
        }
        /// <summary>
        /// 重置手牌的各种显示属性，如拖拽等。
        /// </summary>
        /// <param name="handCard"></param>
        public void resetHand(ICardData handCard)
        {
            if (tryGetHand(handCard, out var usingHand))
            {
                usingHand.card.setVisible(true);
                usingHand.setSpellReady(false);
                var desc = updateCardDescriptor(handCard);
                setCardDesc(usingHand.card, desc);
                usingHand.card.setFaceup(true);
                usingHand.card.transform.localScale = Vector3.one;
                usingHand.card.transform.localPosition = Vector3.zero;
                usingHand.ignoreAlign = false;
            }
        }
        /// <summary>
        /// 从UI手牌引用获取对应的牌
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Card getCard(HandItem item)
        {
            foreach (var pair in cardHandDic)
            {
                if (pair.Value == item)
                    return game.getCard(pair.Key);
            }
            return null;
        }
        public HandList getHandList(Pile pile)
        {
            return pile?.owner == player ? table.SelfHandList : table.EnemyHandList;
        }
        #endregion 手牌

        #region 仆从
        /// <summary>
        /// 创建一个随从
        /// </summary>
        /// <param name="desc"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public Servant createServant(ICardData card, int position) => createServant(updateCardDescriptor(card), position);
        /// <summary>
        /// 创建一个随从
        /// </summary>
        /// <param name="desc"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public Servant createServant(CardDescriptor desc, int position) => createServant(desc, desc.getOwner(), position);
        /// <summary>
        /// 创建一个随从
        /// </summary>
        /// <param name="desc"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public Servant createServant(CardDescriptor desc, Player owner, int position)
        {
            bool isSelf = owner == player;
            var field = isSelf ? table.SelfField : table.EnemyField;
            Servant servant;
            if (position < 0)
            {
                servant = field.addServant();
            }
            else
            {
                servant = field.insertServant(position);
            }
            var skin = getCardSkin(desc.define);
            servant.gameObject.name = $"{skin?.name ?? "Card"}<{desc.id}>";
            UberDebug.LogChannel(servant, "UI", $"为{desc.id}创建{servant}");
            setServant(servant, desc);
            cardServantDic.Add(desc.id, servant);
            setCardControlDirty(desc.id);
            return servant;
        }
        /// <summary>
        /// 设置随从
        /// </summary>
        /// <param name="servant"></param>
        /// <param name="desc"></param>
        /// <param name="isSelectable"></param>
        public void setServant(Servant servant, CardSnapshot card) => setServant(servant, updateCardDescriptor(card));
        /// <summary>
        /// 设置随从
        /// </summary>
        /// <param name="servant"></param>
        /// <param name="desc"></param>
        /// <param name="isSelectable"></param>
        public async void setServant(Servant servant, CardDescriptor desc)
        {
            servant.setAttack(desc.getAttack());
            servant.setLife(desc.getCurrentLife());
            var atkColor = desc.getAttack() > desc.define.getAttack() ? PropertyNumberColor.Green : PropertyNumberColor.White;
            var lifeColor = desc.getCurrentLife() < desc.getLife() ? PropertyNumberColor.Red : desc.getLife() > desc.define.getLife() ? PropertyNumberColor.Green : PropertyNumberColor.White;
            servant.attackNumber.setTextColor(atkColor);
            servant.lifeNumber.setTextColor(lifeColor);

            servant.setTaunt(desc.isTaunt);
            servant.setShield(desc.hasShield);
            servant.setStealth(desc.isStealth);
            servant.setLastword(desc.hasDeathRattle);
            servant.setLethal(desc.isLethal);
            servant.setLifeDrain(desc.isDrain);
            servant.setShackle(desc.isFrozen);
            servant.setSealed(desc.isSilenced);

            servant.setImageSprite(await loadCardImage(desc));
            setCardControlDirty(desc.id);
        }
        /// <summary>
        /// 设置随从定义
        /// </summary>
        /// <param name="servant"></param>
        /// <param name="cardDefine"></param>
        public async void setServant(Servant servant, CardDefine cardDefine)
        {
            servant.setAttack(cardDefine.getAttack());
            servant.setLife(cardDefine.getLife());
            servant.setTaunt(cardDefine.hasKeyword(KEYWORD.TAUNT));
            servant.setShield(cardDefine.hasKeyword(KEYWORD.SHIELD));
            servant.setStealth(cardDefine.hasKeyword(KEYWORD.STEALTH) || cardDefine.hasKeyword(KEYWORD.PERMANENTSTEALTH));
            servant.setLastword(cardDefine.getEffects().hasDeathRattles());
            servant.setLethal(cardDefine.hasKeyword(KEYWORD.POISONOUS));
            servant.setLifeDrain(cardDefine.hasKeyword(KEYWORD.DRAIN));
            servant.setShackle(cardDefine.hasKeyword(KEYWORD.FROZEN) || cardDefine.hasKeyword(KEYWORD.PERMANENT_FROZEN));
            servant.setSealed(false);

            servant.setImageSprite(await loadCardImage(cardDefine));
        }
        /// <summary>
        /// 从卡获取对应的随从UI
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public Servant getServant(ICardData card) => getServant(card.id);
        /// <summary>
        /// 从卡获取对应的随从UI
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Servant getServant(int id)
        {
            if (cardServantDic.ContainsKey(id))
                return cardServantDic[id];
            throw new ActorNotFoundException(game.getCard(id));
        }
        /// <summary>
        /// 尝试从卡获取对应的随从UI
        /// </summary>
        /// <param name="card"></param>
        /// <param name="servant"></param>
        /// <returns></returns>
        public bool tryGetServant(ICardData card, out Servant servant) => tryGetServant(card?.id ?? 0, out servant);
        /// <summary>
        /// 尝试从卡获取对应的随从UI
        /// </summary>
        /// <param name="card"></param>
        /// <param name="servant"></param>
        /// <returns></returns>
        public bool tryGetServant(int id, out Servant servant)
        {
            if (cardServantDic.ContainsKey(id))
            {
                if (cardServantDic[id] != null)
                {
                    servant = cardServantDic[id];
                    return true;
                }
                cardServantDic.Remove(id);
                servant = null;
                return false;
            }
            else
            {
                servant = null;
                return false;
            }
        }

        /// <summary>
        /// 从随从获取对应的卡
        /// </summary>
        /// <param name="servant"></param>
        /// <returns></returns>
        public Card getCard(Servant servant)
        {
            foreach (var pair in cardServantDic)
            {
                if (pair.Value == servant)
                    return game.getCard(pair.Key);
            }
            return null;
        }



        /// <summary>
        /// 移除一个仆从UI。
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public bool removeServant(ICardData card) => removeServant(card.id);
        /// <summary>
        /// 移除一个仆从UI。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool removeServant(int id)
        {
            if (cardServantDic.ContainsKey(id))
            {
                var item = cardServantDic[id];
                UberDebug.LogChannel(item, "UI", $"移除ID为{id}的仆从{item}");
                cardServantDic.Remove(id);
                table.SelfField.removeServant(item);
                table.EnemyField.removeServant(item);
                item.destroy();
                return true;
            }
            return false;
        }
        public Field getField(Pile pile)
        {
            if (pile == player.getField())
                return table.SelfField;
            else if (pile == game.getOpponent(player).getField())
                return table.EnemyField;
            return null;
        }
        public Field getFieldOfServant(Servant servant)
        {
            if (table.SelfField.Contains(servant))
                return table.SelfField;
            else if (table.EnemyField.Contains(servant))
                return table.EnemyField;
            return null;
        }
        public Pile getFieldPile(Field field)
        {
            if (field == table.SelfField)
                return player.getField();
            else if (field == table.EnemyField)
                return game.getOpponent(player).getField();
            return null;
        }
        #endregion

        #region 牌库
        public void addCardToDeckOrGrave(Pile pile, int cardId)
        {
            if (pile?.name == PILE.GRAVE)
            {
                addCardToGrave(pile?.owner, cardId);
            }
            else
            {
                addCardToDeck(pile?.owner, cardId);
            }
        }
        public void addCardToDeck(ICardData card) => addCardToDeck(card.owner, card.id);
        public void addCardToDeck(Player owner, int cardId)
        {
            var deckList = owner == player ? _selfDeckList : _enemyDeckList;
            deckList.Add(cardId);
            updateDeckPanel(owner, PILE.DECK);
            updateDeckTipbox();
        }
        public bool removeCardFromDeck(ICardData card) => removeCardFromDeck(card.id);
        public bool removeCardFromDeck(int cardId)
        {
            if (_selfDeckList.Remove(cardId))
            {
                updateDeckPanel(player, PILE.DECK);
                updateDeckTipbox();
                return true;
            }
            if (_enemyDeckList.Remove(cardId))
            {
                updateDeckPanel(game.getOpponent(player), PILE.DECK);
                updateDeckTipbox();
                return true;
            }
            return false;
        }
        /// <summary>
        /// 获取牌库引用
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public Deck getDeckOfCard(ICardData card) => getDeckOfCard(card.id);
        /// <summary>
        /// 获取牌库引用
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        public Deck getDeckOfCard(int cardId)
        {
            if (_selfDeckList.Contains(cardId))
                return table.SelfDeck;
            else if (_enemyDeckList.Contains(cardId))
                return table.EnemyDeck;
            throw new ActorNotFoundException(game.getCard(cardId));
        }
        /// <summary>
        /// 尝试获取牌库引用
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public bool tryGetDeckOfCard(ICardData card, out Deck deck) => tryGetDeckOfCard(card?.id ?? 0, out deck);

        /// <summary>
        /// 尝试获取牌库引用
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public bool tryGetDeckOfCard(int id, out Deck deck)
        {
            if (_selfDeckList.Contains(id))
            {
                deck = table.SelfDeck;
                return true;
            }
            else if (_enemyDeckList.Contains(id))
            {
                deck = table.EnemyDeck;
                return true;
            }
            deck = null;
            return false;
        }
        public Deck getDeckUI(Pile pile)
        {
            if (pile?.name == PILE.GRAVE)
            {
                return pile.owner == player ? table.SelfGrave : table.EnemyGrave;
            }
            return pile.owner == player ? table.SelfDeck : table.EnemyDeck;
        }
        #endregion

        #region 墓地
        public void addCardToGrave(ICardData card) => addCardToGrave(card.owner, card.id);
        public void addCardToGrave(Player owner, int cardId)
        {
            var deckList = owner == player ? _selfGraveList : _enemyGraveList;
            deckList.Add(cardId);
            updateDeckPanel(owner, PILE.GRAVE);
            updateDeckTipbox();
        }
        public bool removeCardFromGrave(ICardData card) => removeCardFromGrave(card.id);
        public bool removeCardFromGrave(int cardId)
        {
            if (_selfGraveList.Remove(cardId))
            {
                updateDeckPanel(player, PILE.GRAVE);
                updateDeckTipbox();
                return true;
            }
            if (_enemyGraveList.Remove(cardId))
            {
                updateDeckPanel(game.getOpponent(player), PILE.GRAVE);
                updateDeckTipbox();
                return true;
            }
            return false;
        }
        /// <summary>
        /// 获取墓地引用
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public Deck getGraveOfCard(ICardData card) => getGraveOfCard(card.id);
        /// <summary>
        /// 获取墓地引用
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        public Deck getGraveOfCard(int cardId)
        {
            if (_selfGraveList.Contains(cardId))
                return table.SelfGrave;
            else if (_enemyGraveList.Contains(cardId))
                return table.EnemyGrave;
            throw new ActorNotFoundException(game.getCard(cardId));
        }
        /// <summary>
        /// 尝试获取墓地引用
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public bool tryGetGraveOfCard(ICardData card, out Deck deck) => tryGetGraveOfCard(card?.id ?? 0, out deck);

        /// <summary>
        /// 尝试获取墓地引用
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public bool tryGetGraveOfCard(int id, out Deck deck)
        {
            if (_selfGraveList.Contains(id))
            {
                deck = table.SelfGrave;
                return true;
            }
            else if (_enemyGraveList.Contains(id))
            {
                deck = table.EnemyGrave;
                return true;
            }
            deck = null;
            return false;
        }
        #endregion

        #region 揭示
        public UICard createInvokeCard(ICardData card, Vector3 position, float height = -1) => createInvokeCard(updateCardDescriptor(card), position, height);
        public UICard createInvokeCard(CardDescriptor desc, Vector3 position, float height = -1)
        {
            var worldPos = TableToWorldPosition(position);
            var card = Instantiate(table.CardTemplate, worldPos, Quaternion.identity, table.InvokeDisplayParent).GetComponent<UICard>();
            card.transform.localRotation = Quaternion.identity;
            setCardDesc(card, desc);
            if (height < 0)
                return card;
            card.transform.localScale = Vector3.one * height / _cardHeight;
            return card;
        }
        public UICard createInvokeCard(CardDefine define, Vector3 position, float height = -1)
        {
            var worldPos = TableToWorldPosition(position);
            var card = Instantiate(table.CardTemplate, worldPos, Quaternion.identity, table.InvokeDisplayParent).GetComponent<UICard>();
            card.transform.localRotation = Quaternion.identity;
            setCardDefine(card, define);
            if (height < 0)
                return card;
            card.transform.localScale = Vector3.one * height / _cardHeight;
            return card;
        }
        /// <summary>
        /// 删除强制退出过程之中产生的揭示动画
        /// </summary>
        /// <param name="master"></param>
        public void DestroyInvokeCard()
        {
            Transform[] Cards = table.InvokeDisplayParent.GetComponentsInChildren<Transform>();
            for (int i = 1; i < Cards.Length; i++)
            {
                Destroy(Cards[i].gameObject);
            }
        }
        #endregion

        #region 移动中的卡牌
        public UICard createMovingCard(ICardData card, Vector3 position, float height = -1) => createMovingCard(updateCardDescriptor(card), position, height);
        public UICard createMovingCard(CardDescriptor desc, Vector3 position, float height = -1)
        {
            var card = createCard(desc, position, height);
            _movingCardDic.Add(desc.id, card);
            return card;
        }
        public bool removeMovingCard(ICardData card)
        {
            return removeMovingCard(card.id);
        }
        public bool removeMovingCard(CardDescriptor desc)
        {
            return removeMovingCard(desc.id);
        }
        public bool removeMovingCard(int cardId)
        {
            if (_movingCardDic.TryGetValue(cardId, out var uiCard))
            {
                uiCard.destroy();
                return _movingCardDic.Remove(cardId);
            }
            return false;
        }
        public bool tryGetMovingCard(ICardData card, out UICard uiCard)
        {
            return tryGetMovingCard(card.id, out uiCard);
        }
        public bool tryGetMovingCard(int cardId, out UICard card)
        {
            if (_movingCardDic.TryGetValue(cardId, out card))
            {
                if (!card)
                {
                    _movingCardDic.Remove(cardId);
                    return false;
                }
                return true;
            }
            return false;
        }
        #endregion

        #region 发现
        public void displayDiscoverDialog(DefineReference[] cardRefs, string title)
        {
            table.Discover.setActive(true);
            table.Discover.setTitleText(string.IsNullOrEmpty(title) ? lang._(StringTable.HINT_SELECT_A_CARD) : title);
            table.Discover.clearChoices();
            _cardChooseMode = CardChooseMode.CardDefine;
            _choosingCardDefineRefs = cardRefs;
            foreach (var cardRef in cardRefs)
            {
                if (cardRef == null)
                    continue;
                var cardDefine = game.getDefine(cardRef.cardPoolId, cardRef.defineId);
                var item = table.Discover.addChoice();
                setCardDefine(item.card, cardDefine);
            }
        }
        public void displayDiscoverDialog(int[] cardIds, string title)
        {
            table.Discover.setActive(true);
            table.Discover.setTitleText(string.IsNullOrEmpty(title) ? lang._(StringTable.HINT_SELECT_A_CARD) : title);
            table.Discover.clearChoices();
            _cardChooseMode = CardChooseMode.Card;
            _choosingCards = cardIds;
            foreach (var cardId in cardIds)
            {
                var card = game.getCard(cardId);
                if (card == null)
                    continue;
                var item = table.Discover.addChoice();
                setCard(item.card, newSnapshot(card));
            }
        }
        public void closeDiscoverDialog()
        {
            table.Discover.setActive(false);
        }
        #endregion

        #region 陷阱
        public Secret createSecret(ICardData card) => createSecret(updateCardDescriptor(card));
        public Secret createSecret(CardDescriptor desc) => createSecret(desc, desc.getOwner());
        public Secret createSecret(CardDescriptor desc, Player owner)
        {
            var master = owner == player ? table.SelfMaster : table.EnemyMaster;
            var headSlots = master.HeadSlots;
            Secret secret = headSlots.createItem();
            headSlots.addItem(secret);
            cardSecretDic.Add(desc.id, secret);
            setSecret(secret, desc);
            return secret;
        }
        public bool tryGetSecret(ICardData card, out Secret secret) => tryGetSecret(card?.id ?? 0, out secret);
        public bool tryGetSecret(int id, out Secret secret)
        {
            if (cardSecretDic.TryGetValue(id, out secret))
            {
                if (!secret)
                {
                    secret = null;
                    return false;
                }
                return true;
            }
            return false;
        }
        public void setSecret(Secret secret, ICardData card) => setSecret(secret, updateCardDescriptor(card));
        public void setSecret(Secret secret, CardDescriptor desc)
        {
            var owner = desc.getOwner();
            secret.setSecretOpen(currentAnimPlayer != owner);
            secret.setMission(desc.define.hasKeyword(KEYWORD.MISSION));
        }
        public bool removeSecret(ICardData card) => removeSecret(card.id);
        public bool removeSecret(CardDescriptor desc) => removeSecret(desc.id);
        public bool removeSecret(int cardId)
        {
            if (tryGetSecret(cardId, out var secret))
            {
                cardSecretDic.Remove(cardId);
                table.SelfMaster.HeadSlots.removeItem(secret);
                table.EnemyMaster.HeadSlots.removeItem(secret);
                secret.destroy();
                return true;
            }
            return false;
        }
        /// <summary>
        /// 从Secret获取对应的牌
        /// </summary>
        /// <param name="secret"></param>
        /// <returns></returns>
        public Card getCardBySecret(Secret secret)
        {
            foreach (var pair in cardSecretDic)
            {
                if (pair.Value == secret)
                    return game.getCard(pair.Key);
            }
            return null;
        }
        public Secret[] getAllSecrets()
        {
            return cardSecretDic.Values.ToArray();
        }
        public HeadSlots getHeadSlots(Pile pile)
        {
            return pile?.owner == player ? table.SelfMaster.HeadSlots : table.EnemyMaster.HeadSlots;
        }
        #endregion

        #region 特效
        public TableEffect createEffect(DefineReference defRef, VisualEffectInfo info)
        {
            var define = getAnimationDefine(defRef);
            return createEffect(define, info);
        }
        public TableEffect createEffect(AnimationDefine animDef, VisualEffectInfo info)
        {
            if (animDef == null)
                return null;
            var effect = animDef.createEffect(this, table.EffectsParent);
            if (effect)
                effect.init(this, animDef, info);
            return effect;
        }
        public bool removeEffect(TableEffect effect)
        {
            if (effect && effect.gameObject)
            {
                Destroy(effect.gameObject);
                return true;
            }
            return false;
        }
        #endregion

        #region 初始换牌
        public Card getInitCard(MulliganCardItem init)
        {
            foreach (var pair in initDic)
            {
                if (pair.Value == init)
                    return game.getCard(pair.Key);
            }
            return null;
        }
        public void setInitCard(MulliganCardItem init, int cardID)
        {
            if (initDic.ContainsKey(cardID))
            {
                initDic[cardID] = init;
            }
            else
            {
                initDic.Add(cardID, init);
            }
        }
        public void showInitReplaceDialog(InitReplaceRequest request)
        {
            var dialog = ui.InitReplaceDialog;
            if (dialog.isShowing)
                return;

            dialog.setActive(true);

            var initPile = player.getInit();
            dialog.clearChoices();
            foreach (var card in initPile)
            {
                var snapshot = newSnapshot(card);
                var item = dialog.addChoice();
                item.setMarkVisible(false);
                setCard(item.card, snapshot);
                item.card.setFaceup(true);
                setInitCard(item, card.id);
            }
            dialog.setWaiting(false);
        }
        #endregion

        #region 音效
        public bool cardHasSound(CardDefine define, string soundName)
        {
            if (define == null)
                return false;
            var skin = getCardSkin(define);
            if (skin == null)
                return false;
            var soundRef = skin.getSoundRef(soundName);
            if (soundRef == null || string.IsNullOrEmpty(soundRef.path))
                return false;
            return true;
        }
        public async Task<bool> playSound(CardDefine define, string soundName)
        {
            if (define == null)
                return false;
            var skin = getCardSkin(define);
            if (skin == null)
                return false;
            var soundRef = skin.getSoundRef(soundName);
            if (soundRef == null || string.IsNullOrEmpty(soundRef.path))
                return false;
            var audio = await loadSound(define.cardPoolId, soundRef.path);
            if (!audio)
                return false;
            _main.seManager.playSE(audio);
            return true;
        }
        #endregion
        public Transform getCardTransform(Card card)
        {
            return getCardTransform(card.id);
        }
        public Transform getCardTransform(int cardId, bool ignoreDeck = false)
        {
            if (tryGetMaster(cardId, out Master master))
            {
                return master.positionLerp.transform;
            }
            else if (tryGetItem(cardId, out Item item))
            {
                return item.positionLerp.transform;
            }
            else if (tryGetHand(cardId, out HandItem hand))
            {
                return hand.card.positionLerp.transform;
            }
            else if (tryGetSecret(cardId, out Secret secret))
            {
                return secret.positionLerp.transform;
            }
            else if (tryGetSkill(cardId, out Skill skill))
            {
                return skill.positionLerp.transform;
            }
            else if (tryGetMovingCard(cardId, out var movingCard))
            {
                return movingCard.positionLerp.transform;
            }
            else if (tryGetServant(cardId, out Servant servant))
            {
                return servant.positionLerp.transform;
            }
            else if (tryGetDeadCardAnchor(cardId, out Transform trans))
            {
                return trans;
            }

            if (!ignoreDeck)
            {
                if (tryGetDeckOfCard(cardId, out Deck deck))
                {
                    return deck.transform;
                }
                else if (tryGetGraveOfCard(cardId, out Deck grave))
                {
                    return grave.transform;
                }
            }
            return null;
        }
        public Transform getAreaTransform(TableArea area)
        {
            switch (area)
            {
                case TableArea.AllField:
                    return table.AllFieldAreaTransform;
                case TableArea.SelfField:
                    return table.SelfFieldAreaTransform;
                case TableArea.EnemyField:
                    return table.EnemyFieldAreaTransform;
            }
            return null;
        }
        public UICard createCard(ICardData card, Vector3 position, float height = -1) => createCard(updateCardDescriptor(card), position, height);
        public UICard createCard(CardDescriptor desc, Vector3 position, float height = -1)
        {
            var worldPos = TableToWorldPosition(position);
            var card = Instantiate(table.CardTemplate, worldPos, Quaternion.identity, table.DisplayerCardsParent).GetComponent<UICard>();
            card.transform.localEulerAngles = Vector3.zero;
            setCardDesc(card, desc);
            if (height >= 0)
                card.transform.localScale = Vector3.one * height / _cardHeight;
            return card;
        }
        public UICard createCard(CardDefine define, Vector3 position, float height = -1)
        {
            var worldPos = TableToWorldPosition(position);
            var card = Instantiate(table.CardTemplate, worldPos, Quaternion.identity, table.DisplayerCardsParent).GetComponent<UICard>();
            card.transform.localEulerAngles = Vector3.zero;
            setCardDefine(card, define);
            if (height >= 0)
                card.transform.localScale = Vector3.one * height / _cardHeight;
            return card;
        }
        public void setCard(UICard table, CardSnapshot card) => setCardDesc(table, updateCardDescriptor(card));
        /// <summary>
        /// 设置卡片UI
        /// </summary>
        /// <param name="card"></param>
        /// <param name="desc"></param>
        /// <param name="isFaceup"></param>
        public async void setCardDesc(UICard card, CardDescriptor desc)
        {
            await card.setCard(desc, this);
            setCardDescriptor(desc);
        }
        public async void setCardDefine(UICard card, CardDefine define)
        {
            await card.setCardDefine(define, this);
        }
        /// <summary>
        /// 显示文本提示
        /// </summary>
        /// <param name="tip">翻译的<b>源文本</b></param>
        public void showErrorTip(string tip)
        {
            ui.ErrorTipText.gameObject.SetActive(true);
            ui.ErrorTipText.text = lang._(tip);
            ui.ErrorTipText.color = Color.white;
            _tipTimer.start();
        }

        /// <summary>
        /// 高亮所有指定的目标（标记为可选择）
        /// </summary>
        /// <param name="targets"></param>
        private void highlightTargets(Card[] targets)
        {
            foreach (var target in targets)
            {
                if (tryGetMaster(target, out var master))
                {
                    master.setHighlight(true, true);
                }
                else if (tryGetServant(target, out var servant))
                {
                    servant.setHighlight(true, true);
                }
            }
        }

        /// <summary>
        /// 判断鼠标是否在某张卡上，并返回卡
        /// </summary>
        /// <param name="pointer"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        private bool isOnTarget(PointerEventData pointer, out Card card)
        {
            if (pointer.pointerCurrentRaycast.gameObject != null)
            {
                if (pointer.pointerCurrentRaycast.gameObject.GetComponentInParent<Servant>() is Servant targetServant)
                {
                    card = getCard(targetServant);
                    return card != null;
                }
                else if (pointer.pointerCurrentRaycast.gameObject.GetComponentInParent<Master>() is Master targetMaster)
                {
                    card = getCard(targetMaster);
                    return card != null;
                }
            }
            card = null;
            return false;
        }

        public void cancelTargetSelect()
        {
            // TODO: 目前只能取消宣言，因为目前只有宣言的操作是需要点两下，攻击需要拖动。
            if (isSelectingTargetForBattlecry())
            {
                cancelBattlecry(targetSelectingCard);
            }
            finishTargetSelect();
        }
        public void finishTargetSelect()
        {
            targetSelectingCard = null;
            _servantUsingPosition = 0;
            targetSelectMode = TargetSelectMode.None;
            updateAllControlable();
            hideArrow();
        }

        public Vector3 getCardMoveMiddlePosition(Vector3 startTablePos, Vector3 targetTablePos)
        {
            var centerPos = WorldToTablePosition(table.CenterDisplayTransform.position);
            var x = (startTablePos.x + targetTablePos.x) / 2;
            var y = centerPos.y;
            var middleZ = (startTablePos.z + targetTablePos.z) / 2;
            var z = middleZ - (middleZ - centerPos.z) * 0.2f;
            return new Vector3(x, y, z);
        }
        public Vector3 getCardMoveMiddleWorldPosition(Vector3 startWorldPosition, Vector3 targetWorldPosition)
        {
            var startTablePos = WorldToTablePosition(startWorldPosition);
            var targetTablePos = WorldToTablePosition(targetWorldPosition);
            var middlePosition = getCardMoveMiddlePosition(startTablePos, targetTablePos);
            return TableToWorldPosition(middlePosition);
        }

        #endregion
        #region 私有方法

        #region 事件回调
        private void onTurnEndClickCallback(TurnEndButton turnEndButton)
        {
            if (!canControl)
                return;
            if (isSelectingTarget())
                return;
            sounds.PlaySE("Button");
            resetUse();
            player.cmdTurnEnd(game);
        }
        private void onTurnEndPointerEnterCallback(TurnEndButton turnEndButton)
        {
            if (!canControl)
                return;
            turnEndButton.setHighlighted(true);
        }
        private void onTurnEndPointerExitCallback(TurnEndButton turnEndButton)
        {
            if (!canControl)
                return;
            turnEndButton.setHighlighted(false);
        }
        void onClickNoWhere(BattleTable table, PointerEventData pointer)
        {
            if (!canControl)
                return;
            cancelTargetSelect();
        }
        #region 道具
        private void onItemPointerEnterCallback(Item item, PointerEventData pointer)
        {
            startHover(item);
        }

        private void onItemPointerExitCallback(Item item, PointerEventData pointer)
        {
            stopHover();
        }
        #endregion 道具

        #region 手牌
        private void onHandListBlockerClickCallback(HandList list)
        {
            if (list != table.SelfHandList) return;

            if (list.isZoomIn)
            {
                zoomHand(false);
            }
        }
        private void onHandItemClickCallback(HandItem hand, HandList list, PointerEventData pointer)
        {
            if (list != table.SelfHandList)
                return;

            var card = getCard(hand);
            if (usingCard == card) // 点击正在拖动的卡牌时不会缩进
                return;

            if (list.isZoomIn)
            {
                if (_hoveredHand == hand)
                {
                    stopHover();
                }
                zoomHand(false);
            }
            else
            {
                startHover(hand);
                zoomHand(true);
            }
        }
        private void onHandItemBeginDragCallback(HandItem hand, HandList list, PointerEventData pointer)
        {
            if (!canControl)
                return;
            if (!hand.isDraggable)
                return;
            Card card = getCard(hand);
            if (isSelectingTargetForOtherCards(card))//已经在用别的牌了
                return;
            usingCard = card;
            hand.ignoreAlign = true;
        }
        private void onHandItemDragCallback(HandItem hand, HandList list, PointerEventData pointer)
        {
            if (!canControl)
                return;
            if (!hand.isDraggable)
                return;
            Card card = getCard(hand);
            if (isSelectingTargetForOtherCards(card)) //已经在用别的牌了
                return;
            if (usingCard != card)
            {
                resetUse();
                return;
            }
            bool setPosition = true;
            // 拖回手牌
            var screenPos = pointer.position;
            var worldPos = ScreenToWorldPosition(screenPos);
            var pos = WorldToTablePosition(worldPos);
            var inverseTransPosition = table.SelfHandList.transform.InverseTransformPoint(worldPos);
            if (table.SelfHandList.containsPosition(inverseTransPosition) || screenPos.y < 0)
            {
                //移除随从占位
                if (card.isServant())
                    table.SelfField.setPreviewPosition(-1);
            }
            // 卡牌不可用
            else
            {
                // 停止衍生物和关键词显示。
                stopHover();
                if (!canCardBeControlled(card, out string info))
                {
                    //停止拖拽并提示
                    showErrorTip(info);
                    resetUse();
                    return;
                }
                else
                {
                    //移动到手牌区以外的地方视作打算使用
                    // 仆从牌。
                    if (card.isServant())
                    {
                        //如果手牌是随从，那么在场上的时候会有一个占位符，预览这个随从放下去的位置。
                        table.SelfField.setPreviewPosition(calcIndexInField(pos.x));
                    }
                    else if (card.isSpell())
                    {
                        if (card.isNeedTarget(game))
                        {
                            startSelectingTargetForSpell(card);
                            setPosition = false;
                        }
                    }
                }
            }
            //拖拽卡片
            if (setPosition)
            {
                var handPos = WorldToTablePosition(hand.transform.position);
                handPos.x = pos.x;
                handPos.z = pos.z;
                hand.transform.position = TableToWorldPosition(handPos);
            }
        }
        /// <summary>
        /// 手牌拖拽结束
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="pointer"></param>
        private void onHandItemEndDragCallback(HandItem hand, HandList list, PointerEventData pointer)
        {
            if (!canControl)
                return;
            if (list != table.SelfHandList)//你不能用别人的卡
                return;
            Card card = getCard(hand);
            if (isSelectingTargetForOtherCards(card)) // 已经在用别的牌了
                return;
            if (usingCard != card || card.pile != player.getHand())
            {
                resetUse();
                return;
            }
            // 拖回手牌
            var screenPos = pointer.position;
            var worldPos = ScreenToWorldPosition(screenPos);
            var pos = WorldToTablePosition(worldPos);
            var inverseTransPosition = table.SelfHandList.transform.InverseTransformPoint(worldPos);
            if (table.SelfHandList.containsPosition(inverseTransPosition) || screenPos.y < 0)
            {
                //如果松开，取消使用
                resetUse();
                return;
            }
            //卡牌不可用
            if (!canCardBeControlled(card, out string info))
            {
                showErrorTip(info);
                resetUse();
                return;
            }

            // 随从牌。
            if (card.isServant())
            {
                //松开鼠标，确认使用随从牌
                int index = calcIndexInField(pos.x);
                table.SelfField.setPreviewPosition(-1);
                if (card.getAvaliableTargets(game) is Card[] targets && targets.Length > 0)
                {
                    //进入选择目标状态，固定手牌到占位上，高亮可以选择的目标
                    enterTargetSelectMode(TargetSelectMode.Use, card, card.getAvaliableTargets(game));
                    addServantPreviewAnim(card, index, pos);
                    zoomHand(false);//手牌缩小
                }
                else
                {
                    //使用无目标随从牌
                    player.cmdUse(game, card, index);
                    resetUse(resetHandItem: false);
                }
            }
            // 法术牌。
            else if (card.isSpell())
            {
                playSpellCard(card, pointer);
            }
            // 道具牌。
            else if (card.isItem())
            {
                if (card.isNeedTarget(game))
                {
                    //进入选择目标状态，固定手牌到占位上，高亮可以选择的目标
                    enterTargetSelectMode(TargetSelectMode.Use, card, card.getAvaliableTargets(game));
                    zoomHand(false);//手牌缩小
                }
                else
                {
                    //使用无目标道具牌
                    player.cmdUse(game, card, 0);
                    resetUse(resetHandItem: false);
                }
            }
        }

        /// <summary>
        /// 手牌悬停事件
        /// </summary>
        /// <param name="handList"></param>
        /// <param name="pointer"></param>
        private void onHandItemPointerEnterCallback(HandItem hand, HandList list, PointerEventData pointer)
        {
            if (list.count <= 0)
                return;
            if (list == table.SelfHandList)
                hand.setHovered(true);
            if (!table.SelfHandList.isZoomIn)
            {
                int count = list.count;
                showTipBox(lang._(TAB_CARD_NUMBER, count), hand.gameObject);
                var tablePos = ScreenToTablePosition(pointer.position);
                tablePos.y = 0;
                setTipboxPosition(tablePos);
            }
            else
            {
                startHover(hand);
            }
        }
        private void onHandItemPointerExitCallback(HandItem hand, HandList list, PointerEventData pointer)
        {
            hand.setHovered(false);
            hideTipBoxFromObject(hand.gameObject);
            stopHover();
        }
        #endregion

        #region 英雄
        /// <summary>
        /// 点击Master卡处理
        /// </summary>
        /// <param name="master"></param>
        /// <param name="pointer"></param>
        private void onMasterClickCallback(Master master, PointerEventData pointer)
        {
        }
        private void onMasterPointerUpCallback(Master master, PointerEventData pointer)
        {
            if (!canControl)
                return;
            // （目标选择模式）
            trySelectTargetForBattlecry(getCard(master));
        }


        private void onMasterBeginDragCallback(Master master, PointerEventData pointer)
        {

        }
        /// <summary>
        /// 英雄拖拽处理
        /// </summary>
        /// <param name="master"></param>
        /// <param name="pointer"></param>
        private void onMasterDragCallback(Master master, PointerEventData pointer)
        {
            if (!canControl)
                return;
            Card card = getCard(master);
            if (isSelectingTargetForOtherCards(card))//当前正在操作的卡牌不是这个英雄
                return;
            if (!canCardBeControlled(card))
                return;
            //拉动距离也应该有一个阈值
            var pos = ScreenToTablePosition(pointer.position);
            var masterPos = WorldToTablePosition(master.transform.position);
            if (Vector3.Distance(masterPos, pos) > masterAttackThreshold)
            {
                var targets = game.findAllCardsInField(c => card.isAttackable(game, c, out _));
                if (targetSelectMode != TargetSelectMode.Attack)
                {
                    _ = playSound(getCardDescriptor(card).define, CardSkinData.SOUND_NAME_ON_ATTACK);
                    enterTargetSelectMode(TargetSelectMode.Attack, card, targets);
                }
            }
            else
            {
                cancelTargetSelect();
            }
        }
        /// <summary>
        /// 英雄拖拽结束处理
        /// </summary>
        /// <param name="master"></param>
        /// <param name="pointer"></param>
        private void onMasterEndDragCallback(Master master, PointerEventData pointer)
        {
            if (!canControl)
                return;
            Card card = getCard(master);
            if (!isSelectingTargetForCard(card))//当前正在操作的卡牌不是这个英雄，或者没有正在操作的卡牌
                return;
            if (!canCardBeControlled(card))
                return;
            //如果在随从上面
            if (isOnTarget(pointer, out var target))
            {
                if (card.isAttackable(game, target, out var tip))
                {
                    player.cmdAttack(game, card, target);
                    resetUse(finishSelectTarget: true);
                    return;
                }
                else
                    showErrorTip(tip);
            }
            //取消选中和攻击
            cancelTargetSelect();
        }
        #endregion

        #region 技能
        /// <summary>
        /// 技能点击处理
        /// </summary>
        private void onSkillClickCallback(Skill skill, PointerEventData pointer)
        {
            if (!canControl)
                return;
            if (!isLocalTurn)
                return;
            var skillCard = player.getSkill();
            if (isSelectingTargetForOtherCards(skillCard))//已经在用别的牌了，不能点技能
                return;
            if (!canCardBeControlled(skillCard, out string info))
            {
                showErrorTip(info);
                return;
            }

            if (!skillCard.isNeedTarget(game))
                player.cmdUse(game, skillCard);
        }
        private void onSkillBeginDragCallback(Skill skill, PointerEventData pointer)
        {

        }
        /// <summary>
        /// 技能拖拽
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="pointer"></param>
        private void onSkillDragCallback(Skill skill, PointerEventData pointer)
        {
            if (!canControl)
                return;
            if (!isLocalTurn)
                return;
            var skillCard = player.getSkill();
            if (isSelectingTargetForOtherCards(skillCard)) //正在操作的卡牌不是当前技能
                return;
            // 不可用。
            if (!canCardBeControlled(skillCard, out var info))
            {
                showErrorTip(info);
                return;
            }
            var pos = ScreenToTablePosition(pointer.position);
            var skillPos = WorldToTablePosition(skill.transform.position);
            if (Vector3.Distance(skillPos, pos) > skillDragThreshold)
            {
                if (skillCard.isNeedTarget(game))
                {
                    enterTargetSelectMode(TargetSelectMode.Use, skillCard, skillCard.getAvaliableTargets(game));
                }
            }
            else
                cancelTargetSelect();
        }
        /// <summary>
        /// 技能结束拖拽
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="pointer"></param>
        private void onSkillEndDragCallback(Skill skill, PointerEventData pointer)
        {
            if (!canControl)
                return;
            if (!isLocalTurn)
            {
                cancelTargetSelect();
                return;
            }
            var skillCard = player.getSkill();
            if (!isSelectingTargetForCard(skillCard))
                return;//在用其他卡片
            if (!canCardBeControlled(skillCard, out string info))
            {
                showErrorTip(info);
            }
            else if (isOnTarget(pointer, out var target))
            {
                if (skillCard.isValidTarget(game, target, out string invalidMsg))
                {
                    player.cmdUse(game, player.getSkill(), 0, target);
                    resetUse(resetHandItem: false, finishSelectTarget: true);
                    return;
                }
                else
                    showErrorTip(invalidMsg);
            }
            cancelTargetSelect();
        }
        private void onSkillPointerExitCallback(Skill skill, PointerEventData pointer)
        {
            hideTipBoxFromObject(skill.gameObject);
        }

        private void onSkillPointerEnterCallback(Skill skill, PointerEventData pointer)
        {
            string desc = getCardSkin(skill == table.SelfSkill ? player.getSkill() : game.getOpponent(player).getSkill()).desc;
            showTipBox(desc, skill.gameObject);
            var tablePos = ScreenToTablePosition(pointer.position);
            tablePos.y = 0;
            setTipboxPosition(tablePos);
        }
        #endregion

        #region 仆从
        private void onServantBeginDragCallback(Servant servant, Field field, PointerEventData pointer)
        {

        }
        /// <summary>
        /// 点击随从卡
        /// </summary>
        /// <param name="servant"></param>
        /// <param name="pointer"></param>
        private void onServantClickCallback(Servant servant, Field field, PointerEventData pointer)
        {
        }
        private void onServantPointerUpCallback(Servant master, Field field, PointerEventData pointer)
        {
            if (!canControl)
                return;
            // （目标选择模式）
            trySelectTargetForBattlecry(getCard(master));
        }

        private void onServantPointerEnterCallback(Servant servant, Field field, PointerEventData pointer)
        {
            startHover(servant);
        }
        private void onServantPointerExitCallback(Servant servant, Field field, PointerEventData pointer)
        {
            stopHover();
        }
        /// <summary>
        /// 随从拖拽处理
        /// </summary>
        /// <param name="servant"></param>
        /// <param name="pointer"></param>
        private void onServantDragCallback(Servant servant, Field field, PointerEventData pointer)
        {
            if (!canControl)
                return;
            Card card = getCard(servant);
            if (!canCardBeControlled(card))
                return;
            if (isSelectingTargetForOtherCards(card))//正在操作的卡牌并非该仆从。
                return;
            //拉动距离也应该有一个阈值
            var pos = ScreenToTablePosition(pointer.position);
            var servantPos = WorldToTablePosition(servant.transform.position);
            if (Vector3.Distance(servantPos, pos) > servantAttackThreshold)
            {
                var targets = game.findAllCardsInField(c => card.isAttackable(game, c, out _));
                if (targetSelectMode != TargetSelectMode.Attack)
                {
                    _ = playSound(getCardDescriptor(card).define, CardSkinData.SOUND_NAME_ON_ATTACK);
                    enterTargetSelectMode(TargetSelectMode.Attack, card, targets);
                }
            }
            else
            {
                cancelTargetSelect();
            }
        }
        /// <summary>
        /// 随从拖拽结束处理
        /// </summary>
        /// <param name="servant"></param>
        /// <param name="pointer"></param>
        private void onServantEndDragCallback(Servant servant, Field field, PointerEventData pointer)
        {
            if (!canControl)
                return;
            Card card = getCard(servant);
            if (!isSelectingTargetForCard(card))//正在操作的卡牌并非该仆从，或者没有正在操作的卡牌。
                return;
            if (!canCardBeControlled(card))
                return;
            //如果在随从或英雄上面
            if (isOnTarget(pointer, out var target))
            {
                if (card.isAttackable(game, target, out var tip))
                {
                    player.cmdAttack(game, card, target);
                    resetUse(finishSelectTarget: true);
                    return;
                }
                else
                    showErrorTip(tip);
            }
            //取消选中和攻击
            cancelTargetSelect();
        }
        #endregion

        #region 牌库

        private void onDeckClickCallback(Deck deck, PointerEventData pointer)
        {
            if (isSelectingTarget())
                return;
            var owner = deck == table.SelfDeck ? player : game.getOpponent(player);
            if (owner != player && !canSeeAllCards)
                return;
            var isSelf = owner == player;
            var title = lang._(isSelf ? TAB_YOUR_BOX : TAB_OPPO_DECK);
            viewDeckPanel(title, owner, PILE.DECK);
        }
        private void onDeckPointerEnterCallback(Deck deck, PointerEventData pointer)
        {
            showDeckTipbox(deck, pointer);
        }
        private void onDeckPointerExitCallback(Deck deck, PointerEventData pointer)
        {
            hideTipBoxFromObject(deck.gameObject);
        }
        #endregion

        #region 墓地


        private void onGraveClickCallback(Deck grave, PointerEventData pointer)
        {
            if (isSelectingTarget())
                return;
            var owner = grave == table.SelfGrave ? player : game.getOpponent(player);
            if (owner != player && !canSeeAllCards)
                return;
            var isSelf = owner == player;
            var title = lang._(isSelf ? TAB_YOUR_GRAVEYARD : TAB_OPPO_GRAVEYARD);
            viewDeckPanel(title, owner, PILE.GRAVE);
        }
        private void onGravePointerEnterCallback(Deck deck, PointerEventData pointer)
        {
            showDeckTipbox(deck, pointer);
        }
        private void onGravePointerExitCallback(Deck deck, PointerEventData pointer)
        {
            hideTipBoxFromObject(deck.gameObject);
        }
        #endregion

        #region 陷阱
        private void onHeadItemPointerEnterCallback(Secret headItem, HeadSlots headSlots)
        {
            startHover(headItem);
        }

        private void onHeadItemPointerExitCallback(Secret headItem, HeadSlots headSlots)
        {
            stopHover();
        }
        #endregion 道具

        #endregion

        #region 指针悬浮
        private void startHoverTimer()
        {
            _showCardTimer.start();
            _popupCardTimer.start();
        }
        private void startHover(Secret headItem)
        {
            startHoverTimer();
            _hoveredHeadItem = headItem;
        }
        private void startHover(Servant servant)
        {
            startHoverTimer();
            _hoveredServant = servant;
        }
        private void startHover(Item item)
        {
            startHoverTimer();
            _hoveredItem = item;
        }
        private void startHover(HandItem hand)
        {
            startHoverTimer();
            _hoveredHand = hand;
        }
        private void startHoverPilePanelItem(UICard card)
        {
            startHoverTimer();
            _hoveredPileCard = card;
        }
        private void startHoverDiscoverItem(UICard card)
        {
            startHoverTimer();
            _hoveredDiscoverCard = card;
        }
        private void startHoverMulliganItem(MulliganCardItem card)
        {
            startHoverTimer();
            _hoveredMulliganItem = card;
        }
        private void stopHover()
        {
            _hoveredServant = null;
            _hoveredItem = null;
            _hoveredHeadItem = null;
            _hoveredHand = null;
            _hoveredPileCard = null;
            _hoveredDiscoverCard = null;
            _hoveredMulliganItem = null;
            clearCardDisplayerData();
            clearPopUpData();
        }
        private Vector3 getTransformHoveringPosition(Transform transform, bool autoOffset = true)
        {
            var targetPosition = WorldToTablePosition(transform.position);
            if (autoOffset)
            {
                Vector3 centerPosition = WorldToTablePosition(table.CenterDisplayTransform.position);
                if (Mathf.Abs(targetPosition.z - centerPosition.z) > centerPosition.z - WorldToTableDistance(_verticalExtents)) // 位于屏幕上下边缘
                {
                    targetPosition.z += targetPosition.z < centerPosition.z ? 250 : -250;
                }
                else // 位于屏幕中间部分
                {
                    targetPosition.x += targetPosition.x < centerPosition.x ? 250 : -250;
                }
            }
            return targetPosition;
        }
        #endregion

        #region 卡牌显示器
        private async void showCardDisplayer(Vector3 targetPosition, CardDescriptor desc)
        {
            var inversePos = ui.CardDisplayerObj.transform.parent.InverseTransformPoint(targetPosition);
            var pos = ui.CardDisplayerObj.transform.localPosition;
            pos.x = inversePos.x;
            pos.y = inversePos.y;
            ui.CardDisplayerObj.transform.localPosition = pos;

            ui.CardDisplayer.setVisible(true);
            setCardDesc(ui.CardDisplayer.card, desc);
            ui.CardDisplayer.card.setFaceup(true);

            var buffs = desc.getBuffs().Where(b => b.define is not IInnateBuffDefine);
            var buffViewDatas = await getCardBuffViewDatas(buffs);
            ui.CardDisplayer.setBuffs(buffViewDatas);

            ui.CardDisplayer.limitCanvasHeight();

            _isDisplayingCard = true;
        }
        private void hideCardDisplayer()
        {
            ui.CardDisplayer.setVisible(false);
            _isDisplayingCard = false;
        }
        private void clearCardDisplayerData()
        {
            _showCardTimer.reset();
            hideCardDisplayer();
        }
        private bool tryGetHoveringPosition(out Vector3 position)
        {
            Transform transform = null;
            if (_hoveredServant != null)
                transform = _hoveredServant.transform;
            else if (_hoveredItem != null)
                transform = _hoveredItem.transform;
            else if (_hoveredHeadItem != null)
            {
                var headCard = getCardBySecret(_hoveredHeadItem);
                if (!headCard.isTrap(game) || canSeeCard(headCard))
                {
                    transform = _hoveredHeadItem.transform;
                }
            }

            if (transform && transform.gameObject.activeInHierarchy)
            {
                position = getTransformHoveringPosition(transform);
                return true;
            }
            position = Vector3.zero;
            return false;
        }
        private Card getHoveringCard()
        {
            if (_hoveredServant != null)
                return getCard(_hoveredServant);
            else if (_hoveredItem != null)
                return getCardByItem(_hoveredItem);
            else if (_hoveredHeadItem != null)
            {
                var headCard = getCardBySecret(_hoveredHeadItem);
                if (!headCard.isTrap(game) || canSeeCard(headCard))
                {
                    return headCard;
                }
            }
            return null;
        }
        #endregion

        #region 涌现式衍生物&关键词提示
        private void setCardPopUpPosition(Vector3 targetPosition)
        {
            var worldPosition = TableToWorldPosition(targetPosition);
            ui.CardPopUp.setPosition(worldPosition);
        }
        private async void showCardPopUp(Vector3 targetPosition, CardDefine tokenDefine, string[] keywords)
        {
            var worldPosition = TableToWorldPosition(targetPosition);
            var skin = getCardSkin(tokenDefine);
            await ui.CardPopUp.show(worldPosition, tokenDefine, skin, cardManager, table.Camera, keywords);
        }
        private void hideCardPopUp()
        {
            ui.CardPopUp.hide();
            _isPopingUpCard = false;
        }
        private void clearPopUpData()
        {
            _popupCardTimer.reset();
            hideCardPopUp();
        }
        private bool tryGetPopupHoveringPosition(out Vector3 position)
        {
            if (tryGetHoveringPosition(out position))
                return true;

            Transform transform = null;
            if (_hoveredHand != null)
                transform = _hoveredHand.card.positionLerp.transform;
            else if (_hoveredPileCard != null)
                transform = _hoveredPileCard.transform;
            else if (_hoveredDiscoverCard != null)
                transform = _hoveredDiscoverCard.transform;
            else if (_hoveredMulliganItem != null)
                transform = _hoveredMulliganItem.transform;

            if (transform && transform.gameObject.activeInHierarchy)
            {
                position = getTransformHoveringPosition(transform, false);
                return true;
            }
            position = Vector3.zero;
            return false;
        }
        private Card getPopupHoveringCard()
        {
            var cardDisplayerCard = getHoveringCard();
            if (cardDisplayerCard != null)
                return cardDisplayerCard;

            if (_hoveredHand != null)
                return getCard(_hoveredHand);
            else if (_hoveredPileCard != null)
            {
                var index = ui.PilePanel.indexOf(_hoveredPileCard);
                var cardIds = getDeckCardIds(_viewingDeckPlayer, _viewingDeckName);
                var cardId = cardIds.ElementAt(index);
                return game.getCard(cardId);
            }
            else if (_hoveredDiscoverCard != null && _cardChooseMode == CardChooseMode.Card)
            {
                var index = table.Discover.indexOf(_hoveredDiscoverCard);
                var cardId = _choosingCards.ElementAt(index);
                return game.getCard(cardId);
            }
            else if (_hoveredMulliganItem != null)
            {
                var index = ui.InitReplaceDialog.indexOf(_hoveredMulliganItem);
                return player.getInit()[index];
            }
            return null;
        }
        private CardDefine getPopupHoveringCardToken()
        {
            Card card = getPopupHoveringCard();
            CardDefine cardDefine = null;
            if (card != null)
                cardDefine = card.define;
            else if (_hoveredDiscoverCard != null && _cardChooseMode == CardChooseMode.CardDefine)
            {
                var index = table.Discover.indexOf(_hoveredDiscoverCard);
                var defRef = _choosingCardDefineRefs.ElementAt(index);
                cardDefine = game.getDefine(defRef);
            }
            if (cardDefine == null)
                return null;
            return getCardTokens(cardDefine)?.FirstOrDefault();
        }
        private string[] getPopupHoveringCardKeywords()
        {
            Card card = getPopupHoveringCard();
            string[] keywords = null;
            if (card != null)
                keywords = getCardDefineReferedKeywords(card.define);
            else if (_hoveredDiscoverCard != null && _cardChooseMode == CardChooseMode.CardDefine)
            {
                var index = table.Discover.indexOf(_hoveredDiscoverCard);
                var defRef = _choosingCardDefineRefs.ElementAt(index);
                var cardDefine = game.getDefine(defRef);
                keywords = getCardDefineReferedKeywords(cardDefine);
            }
            return keywords;
        }
        #endregion

        /// <summary>
        /// 显示箭头
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        void displayArrow(Transform source, Vector3 from, Vector3 to)
        {
            table.AttackArrowImage.setActive(true);
            table.AttackArrowImage.connect(this, from, to);
        }
        /// <summary>
        /// 隐藏箭头
        /// </summary>
        void hideArrow()
        {
            table.AttackArrowImage.setActive(false);
        }
        /// <summary>
        /// 计算随从空位位置
        /// </summary>
        /// <param name="worldPos"></param>
        /// <returns></returns>
        private int calcIndexInField(float x)
        {
            int index = 0;
            var servants = table.SelfField;
            if (servants.Count > 0)
            {
                //需要选择空位，计算空位
                for (int i = 0; i < servants.Count; i++)
                {
                    var servantPos = WorldToTablePosition(servants[i].transform.position);
                    if (servantPos.x < x)
                        index = i + 1;
                }
            }
            return index;
        }
        #region 目标选择


        #region 法术
        private void startSelectingTargetForSpell(Card card)
        {
            if (isSelectingTarget())
                return;
            if (tryGetHand(card, out var hand))
            {
                hand.setSpellReady(true);
            }
            enterTargetSelectMode(TargetSelectMode.Use, card, card.getAvaliableTargets(game));
            zoomHand(false);//手牌缩小
        }
        private void playSpellCard(Card card, PointerEventData pointer)
        {
            if (card.isNeedTarget(game))
            {
                //获取鼠标停留的那张卡，就是目标
                if (isOnTarget(pointer, out var target))
                {
                    playSpellToTarget(card, target);
                }
                else
                {
                    resetUse();
                }
            }
            else
            {
                //使用无目标法术牌
                player.cmdUse(game, card, 0);
                resetUse(resetHandItem: false);
            }
        }
        private void playSpellToTarget(Card card, Card target)
        {
            if (card.isValidTarget(game, target, out string invalidMsg))
            {
                //合法目标，生效
                player.cmdUse(game, card, 0, target);
                resetUse(resetHandItem: false, finishSelectTarget: true);
            }
            else
            {
                //非法目标，跳出非法信息
                showErrorTip(invalidMsg);
                resetUse();
            }
        }
        #endregion
        /// <summary>
        /// 在目标选择期间，更新箭头。
        /// </summary>
        /// <param name="position"></param>
        private void showArrowWhileUsing(Vector3 position)
        {
            var sourceTransform = table.SelfMaster.transform;
            var source = targetSelectingCard;
            if (source != null)
            {
                if (source.isSpell())
                {
                    if (tryGetMaster(source, out Master master))
                    {
                        sourceTransform = master.transform;
                    }
                }
                else if (tryGetMovingCard(source, out var movingCard))
                {
                    sourceTransform = movingCard.transform;
                }
                else
                {
                    var trans = getCardTransform(source);
                    if (trans != null)
                    {
                        sourceTransform = trans;
                    }
                }
            }
            //拖出箭头
            var sourcePos = WorldToTablePosition(sourceTransform.position + Vector3.up * 10);
            displayArrow(sourceTransform, sourcePos, position);
        }
        /// <summary>
        /// 取消宣言目标选择。
        /// </summary>
        /// <param name="card">触发宣言的卡牌。</param>
        /// <returns>是否成功取消。</returns>
        private bool cancelBattlecry(Card card)
        {
            if (card == null)
                return false;
            // 取消随从的入场预览动画。
            return cancelServantPreviewAnim(card);
        }
        /// <summary>
        /// 尝试确定选择目标。
        /// </summary>
        /// <param name="target">目标卡牌。</param>
        /// <returns>是否成功。</returns>
        private bool trySelectTargetForBattlecry(Card target)
        {
            if (!isSelectingTargetForBattlecry())
                return false;
            //获取鼠标停留的那张卡，就是目标
            if (target != null)
            {
                var source = targetSelectingCard;
                if (source.isValidTarget(game, target, out string invalidMsg))
                {
                    //合法目标，生效
                    player.cmdUse(game, source, _servantUsingPosition, target);
                    resetUse(resetHandItem: false, finishSelectTarget: true);
                    return true;
                }
                else
                {
                    //非法目标，跳出非法信息
                    showErrorTip(invalidMsg);
                }
            }
            return false;
        }

        private void enterTargetSelectMode(TargetSelectMode mode, Card card, Card[] targets)
        {
            targetSelectingCard = card;
            targetSelectMode = mode;
            //高亮标记所有目标
            updateAllControlable();
            highlightTargets(targets);
        }
        private bool isSelectingTarget()
        {
            return targetSelectMode != TargetSelectMode.None;
        }
        private bool isSelectingTargetForOtherCards(Card card)
        {
            return targetSelectingCard != null && !isSelectingTargetForCard(card);
        }
        private bool isSelectingTargetForCard(Card card)
        {
            return targetSelectingCard == card;
        }
        private bool isSelectingTargetForBattlecry()
        {
            if (targetSelectMode != TargetSelectMode.Use)
                return false;
            return targetSelectingCard != null && (targetSelectingCard.isServant() || targetSelectingCard.isItem());
        }
        private HandToFieldAnim addServantPreviewAnim(Card card, int index, Vector3 pos)
        {
            cancelServantPreviewAnim(card);
            _servantUsingPosition = index;
            var fieldAnim = new HandToFieldAnim(getCardDescriptor(card), pos, player.getHand(), player.getField(), index, canSeeCard(card));
            addAnim(fieldAnim);
            _servantPreviewAnimDict.Add(card.id, fieldAnim);
            return fieldAnim;
        }
        private bool cancelServantPreviewAnim(Card card)
        {
            if (card.pile == player.getHand())
            {
                finishServantPreviewAnim(card.id);
                var handPosition = card.pile.indexOf(card);
                // 如果没有手牌。
                if (!tryGetHand(card, out HandItem hand))
                {
                    // 如果有这个随从，播放回手动画。
                    if (tryGetServant(card, out _))
                    {
                        updateCardDescriptor(card);
                        addAnim(new FieldToHandAnim(card.id, player.getField(), player.getHand(), handPosition));
                    }
                    else //没有这个随从，直接创建手牌。
                    {
                        createHand(getCardDescriptor(card), handPosition);
                    }
                }
                else // 有这张手牌。
                {
                    hand.ignoreAlign = false;
                }
                return true;
            }
            return false;
        }
        private bool finishServantPreviewAnim(int cardId)
        {
            if (_servantPreviewAnimDict.TryGetValue(cardId, out var anim))
            {
                if (anim.isStarted() && !anim.isFinished())
                {
                    anim.forceFinish(this);
                }
                removeAnim(anim);
                _servantPreviewAnimDict.Remove(cardId);
                return true;
            }
            return false;
        }
        #endregion
        private async Task<CardBuffListItemViewData[]> getCardBuffViewDatas(IEnumerable<Buff> buffs)
        {
            // 增益列表。
            // 合并同样的增益。
            var buffCountList = new List<(DefineReference defRef, int count)>();
            DefineReference curBuffRef = null;
            foreach (var buff in buffs)
            {
                var defineRef = buff.define.getDefineRef();
                if (curBuffRef == defineRef && buffCountList.Count > 0)
                {
                    var lastTuple = buffCountList[buffCountList.Count - 1];
                    lastTuple.count++;
                    buffCountList[buffCountList.Count - 1] = lastTuple;
                    continue;
                }
                curBuffRef = defineRef;
                buffCountList.Add((defineRef, 1));
            }

            // 将增益转化为显示数据。
            int maxCount = Mathf.Min(buffCountList.Count, _maxBuffDisplayCount);
            var viewDatas = new CardBuffListItemViewData[maxCount];
            for (int i = 0; i < viewDatas.Length; i++)
            {
                var (defineRef, count) = buffCountList[i];
                var buffDefine = game.getBuffDefine(defineRef);

                Sprite sprite;
                string name;
                string desc;
                if (buffDefine == null)
                {
                    sprite = cardManager.getDefaultSprite();
                    name = lang._(StringTable.BUFF_NAME_UNKNOWN);
                    desc = lang._(StringTable.BUFF_DESC_UNKNOWN);
                }
                else
                {
                    var skin = getBuffSkin(buffDefine);
                    sprite = await loadBuffImage(buffDefine);
                    name = skin?.name ?? string.Empty;
                    desc = skin?.desc ?? string.Empty;
                }
                if (name.Length > _maxBuffNameLength)
                    name = lang._(StringTable.TRIMMED_COTENTS, name.Substring(0, _maxBuffNameLength));
                if (desc.Length > _maxBuffDescLength)
                    desc = lang._(StringTable.TRIMMED_COTENTS, desc.Substring(0, _maxBuffDescLength));
                if (count > 1)
                    name = lang._(CARD_BUFF_LIST_MULTIPLE_TEMPLATE, name, count);

                CardBuffListItemViewData viewData = new CardBuffListItemViewData
                {
                    sprite = sprite,
                    name = name,
                    desc = desc
                };
                viewDatas[i] = viewData;
            }
            return viewDatas;
        }

        private void initDecksAndGraves()
        {
            _selfDeckList.Clear();
            _enemyDeckList.Clear();
            _selfGraveList.Clear();
            _enemyGraveList.Clear();
            foreach (var player in game.players)
            {
                var deck = player.getDeck();
                foreach (var card in deck)
                {
                    addCardToDeck(player, card.id);
                }
                var grave = player.getGrave();
                foreach (var card in grave)
                {
                    addCardToGrave(player, card.id);
                }
            }
        }
        private void showDeckTipbox(Deck deck, PointerEventData pointer)
        {
            showDeckTipbox(deck);
            var tablePos = ScreenToTablePosition(pointer.position);
            tablePos.y = 0;
            setTipboxPosition(tablePos);
        }
        private void showDeckTipbox(Deck deck)
        {
            bool self = deck == table.SelfDeck || deck == table.SelfGrave;
            bool isGrave = deck == table.SelfGrave || deck == table.EnemyGrave;
            string tip;
            if (isGrave)
            {
                var curDeck = self ? _selfGraveList : _enemyGraveList;
                tip = curDeck.Count > 0
                ? lang._(TAB_GRAVEYARD_HAS, curDeck.Count)
                : lang._(TAB_GRAVEYARD_NULL);
            }
            else
            {
                var curDeck = self ? _selfDeckList : _enemyDeckList;
                tip = curDeck.Count > 0
                ? lang._(TAB_BOX_HAS, curDeck.Count)
                : lang._(self ? TAB_YOUR_NO_HAVE_CARD : TAB_OPPONENT_OUT_OF_CARDS);
            }
            showTipBox(tip, deck.gameObject);
        }
        private void updateDeckTipbox()
        {
            if (_tipboxSource == table.SelfDeck.gameObject)
                showDeckTipbox(table.SelfDeck);
            else if (_tipboxSource == table.EnemyDeck.gameObject)
                showDeckTipbox(table.EnemyDeck);
            else if (_tipboxSource == table.SelfGrave.gameObject)
                showDeckTipbox(table.SelfGrave);
            else if (_tipboxSource == table.EnemyGrave.gameObject)
                showDeckTipbox(table.EnemyGrave);
        }
        private void viewDeckPanel(string title, Player player, string pileName)
        {
            ui.PilePanel.setActive(true);
            ui.PilePanel.setTitleText(title);
            _viewingDeckPlayer = player;
            _viewingDeckName = pileName;
            var idList = getDeckCardIds(player, pileName);
            viewDeckPanelCards(idList);
        }
        private void viewDeckPanelCards(IEnumerable<int> idList)
        {
            ui.PilePanel.clearCards();
            var cardList = idList.Select(id => game.getCard(id));
            foreach (var card in cardList)
            {
                var item = ui.PilePanel.addCard();
                var desc = getCardDescriptor(card);
                if (desc == null)
                {
                    var cardSnapshot = game.snapshotCard(card);
                    game.triggers.revertChanges(cardSnapshot, 0);
                    desc = updateCardDescriptor(cardSnapshot);
                }
                setCardDesc(item.card, desc);
                item.card.setFaceup(true);
            }
        }
        private void updateDeckPanel(Deck deckUI)
        {
            Player player = this.player;
            string pileName = PILE.DECK;
            if (deckUI == table.EnemyDeck || deckUI == table.EnemyGrave)
                player = game.getOpponent(this.player);
            if (deckUI == table.SelfGrave || deckUI == table.EnemyGrave)
                pileName = PILE.GRAVE;
            updateDeckPanel(player, pileName);
        }
        private void updateDeckPanel(Player player, string pileName)
        {
            if (_viewingDeckPlayer == player && _viewingDeckName == pileName)
            {
                IEnumerable<int> idList = getDeckCardIds(player, pileName);
                viewDeckPanelCards(idList);
            }
        }
        private bool shouldDeckOrderByCost(string pileName)
        {
            return pileName != PILE.GRAVE && !canSeeAllCards; // 是牌库，或者可以看到所有卡牌
        }
        private IEnumerable<int> getDeckCardIds(Player player, string pileName)
        {
            var orderByCost = shouldDeckOrderByCost(pileName);
            return getDeckCardIds(player, pileName, orderByCost);
        }
        private IEnumerable<int> getDeckCardIds(Player player, string pileName, bool orderByCost)
        {
            IEnumerable<int> cards = player == this.player
                    ? (pileName == PILE.GRAVE ? _selfGraveList : _selfDeckList)
                    : (pileName != PILE.GRAVE ? _enemyDeckList : _enemyGraveList);
            if (orderByCost)
                cards = cards.OrderBy(c => game.getCard(c).define.getCost());
            return cards;
        }
        private void zoomHand(bool expand)
        {
            table.SelfGem.setExpand(expand);
            if (expand)
                table.SelfHandList.zoomIn();
            else
                table.SelfHandList.zoomOut();
        }
        #region 更新可控制高光
        private void updateAllControlable()
        {
            _requesting = _animationQueue.Any(a => a is RequestAnimation);
            foreach (var player in game.players)
            {
                updateCardControlable(player.getMaster());
                updateCardControlable(player.getSkill());
            }
            foreach (var id in cardServantDic.Keys.ToArray())
            {
                var card = game.getCard(id);
                updateCardControlable(card);
            }
            table.TurnEndButton.setInteractable(game.getCurrentPlayer() == player && !_requesting && isLocalTurn);
        }

        private bool canCardBeControlled(Card card)
        {
            return canCardBeControlled(card, out _);
        }
        private bool canCardBeControlled(Card card, out string info)
        {
            info = string.Empty;
            if (card == null)
                return false;
            var desc = getCardDescriptor(card);
            if (desc == null)
                return false;
            var owner = desc.getOwner();
            bool highlight = owner == player && game.getCurrentPlayer() == player && isLocalTurn && !_requesting;
            var pileName = card.pile?.name;
            if (pileName == PILE.HAND ||
                (pileName == PILE.SKILL && desc.isSkill()))
            {
                var usable = card.isUsable(game, player, out info);
                return highlight && usable;//手牌/技能是可用的
            }
            if ((pileName == PILE.FIELD && card.isServant()) ||
                (pileName == PILE.MASTER && card.isMaster()))
            {
                var canAttack = card.canAttack(game);
                return highlight && canAttack;//可以攻击
            }
            return false;
        }
        private void updateCardControlable(Card card)
        {
            if (card == null)
                return;
            var desc = getCardDescriptor(card);
            if (desc == null)
                return;

            var highlight = canCardBeControlled(card) && !isSelectingTarget();
            if (tryGetMaster(desc.id, out Master master))
            {
                master.setHighlight(highlight);
            }
            else if (tryGetSkill(desc.id, out Skill skill))
            {
                skill.setHighlight(highlight);
            }
            else if (tryGetServant(desc.id, out Servant servant))
            {
                servant.setHighlight(highlight);
            }

        }
        private void updateCardControlable(int cardId)
        {
            var card = game.getCard(cardId);
            updateCardControlable(card);
        }
        #endregion
        #endregion
        #region 属性字段
        [TranslateMsg]
        private const string TAB_CARD_NUMBER = "{0}张手牌";
        [TranslateMsg]
        private const string TAB_THIS_INVALIDATION = "这不是一个有效的目标！";
        [TranslateMsg]
        private const string TAB_GRAVEYARD_HAS = "墓地里有{0}张卡牌";
        [TranslateMsg]
        private const string TAB_GRAVEYARD_NULL = "墓地是空的";
        [TranslateMsg]
        private const string TAB_YOUR_GRAVEYARD = "你的墓地";
        [TranslateMsg]
        private const string TAB_OPPO_GRAVEYARD = "对手的墓地";
        [TranslateMsg]
        private const string TAB_SHOW = "显示";
        [TranslateMsg]
        private const string TAB_HIDE = "隐藏";
        [TranslateMsg]
        private const string TAB_YOUR_BOX = "你的牌库";
        [TranslateMsg]
        private const string TAB_OPPO_DECK = "对手的牌库";
        [TranslateMsg]
        private const string TAB_BOX_HAS = "牌库里有{0}张卡牌";
        [TranslateMsg]
        private const string TAB_YOUR_NO_HAVE_CARD = "你已经没有卡牌了";
        [TranslateMsg]
        private const string TAB_OPPONENT_OUT_OF_CARDS = "你的对手已经没有卡牌了";
        [TranslateMsg("游戏中的增益列表项的增益名称模板，在同样的增益属性多于1时显示（{0}：增益名称；{1}：增益数量）")]
        public const string CARD_BUFF_LIST_MULTIPLE_TEMPLATE = "{0}×{1}";

        /// <summary>
        /// 当前是否可以控制（是否是自己的回合）
        /// </summary>
        public bool isLocalTurn { get; set; }
        /// <summary>
        /// 当前正在拖动的手牌卡牌。
        /// </summary>
        public Card usingCard { get; set; } = null;
        /// <summary>
        /// 当前正在使用/攻击的卡片的引用
        /// </summary>
        public Card targetSelectingCard { get; set; } = null;
        /// <summary>
        /// 目标选择模式
        /// </summary>
        public TargetSelectMode targetSelectMode { get; set; }
        public float cardHeight => _cardHeight;
        private bool _requesting;
        public float servantAttackThreshold => _servantAttackThreshold;
        public float masterAttackThreshold => _masterAttackThreshold;
        private CardChooseMode _cardChooseMode;
        private int[] _choosingCards;
        private DefineReference[] _choosingCardDefineRefs;
        private Dictionary<int, Servant> cardServantDic = new Dictionary<int, Servant>();
        private Dictionary<int, Secret> cardSecretDic = new Dictionary<int, Secret>();
        private Dictionary<int, HandItem> cardHandDic = new Dictionary<int, HandItem>();
        private Dictionary<int, MulliganCardItem> initDic = new Dictionary<int, MulliganCardItem>();
        private List<int> _selfDeckList = new List<int>();
        private List<int> _enemyDeckList = new List<int>();
        private List<int> _selfGraveList = new List<int>();
        private List<int> _enemyGraveList = new List<int>();
        private Dictionary<int, UICard> _movingCardDic = new Dictionary<int, UICard>();
        private CardDescriptor _selfItemDesc;
        private CardDescriptor _enemyItemDesc;
        private Player _viewingDeckPlayer;
        private string _viewingDeckName;
        [SerializeField]
        int _servantUsingPosition = -1;
        [SerializeField]
        Vector3 _tipBoxOffset = new Vector3(20, 20, -20);
        [SerializeField]
        float _servantAttackThreshold = 0.7f;
        [SerializeField]
        float _masterAttackThreshold = 0.7f;
        [SerializeField]
        float _cardHeight = 3;
        [SerializeField]
        float _verticalExtents = 2.4f;
        [SerializeField]
        int _maxBuffDisplayCount = 15;
        [SerializeField]
        int _maxBuffNameLength = 10;
        [SerializeField]
        int _maxBuffDescLength = 32;
        [SerializeField]
        Timer _tipTimer = new Timer() { duration = 2 };

        #region 查看卡牌大图
        [SerializeField]
        Timer _showCardTimer = new Timer() { duration = 0.25f };
        Servant _hoveredServant;
        Item _hoveredItem;
        Secret _hoveredHeadItem;
        HandItem _hoveredHand;
        UICard _hoveredPileCard;
        UICard _hoveredDiscoverCard;
        MulliganCardItem _hoveredMulliganItem;
        bool _isDisplayingCard;
        #endregion

        #region 衍生物&关键词弹出
        [SerializeField]
        Timer _popupCardTimer = new Timer() { duration = 0.5f };
        bool _isPopingUpCard;
        #endregion
        /// <summary>
        /// 需要更新所有卡牌的可控绿光。
        /// </summary>
        bool _allControlDirty;
        /// <summary>
        /// 需要更新可控绿光的卡牌列表。
        /// </summary>
        HashSet<int> _controlDirtyCards = new HashSet<int>();
        #endregion
    }

    public enum TargetSelectMode
    {
        None,
        Use,
        Attack
    }
}
