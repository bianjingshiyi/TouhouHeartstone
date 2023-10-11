using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public void updateCardBySnapshot(CardSnapshot card, EventRecord record)
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
                updateItemMaster(card, record);
            }
            else if (tryGetSkill(card, out var skill))
            {
                setSkill(skill, card);
            }
            else if (tryGetHand(card, out var hand))
            {
                setCard(hand.card, card, isHandVisible(hand));
            }
        }
        /// <summary>
        /// 重置使用卡牌状态
        /// </summary>
        /// <param name="resetHandItem">是否重置手牌</param>
        /// <param name="resetServantPreview">是否重置随从占位</param>
        /// <param name="finishSelectTarget">是否要完成目标选择而非取消目标选择</param>
        public void resetUse(bool resetHandItem = true, bool resetServantPreview = true, bool finishSelectTarget = false)
        {
            if (resetHandItem)
            {
                resetHand(usingCard);
                var handList = table.SelfHandList;
                handList.ignoreAlignHand = null;
            }
            usingCard = null;
            _servantPreviewAnim = null;
            if (resetServantPreview)
            {
                table.SelfField.setPreviewPosition(-1);
            }
            if (finishSelectTarget)
                finishTargetSelect();
            else
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
        public int getMasterAttack(ICardData card) => getMasterAttack(card.id);
        public int getMasterAttack(int cardId) => getMasterAttack(getCardDescriptor(cardId));
        public int getMasterAttack(CardDescriptor desc)
        {
            return desc.getAttack() + getMasterItemAttack(desc);
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
            master.setLife(desc.getCurrentLife());

            var attack = getMasterAttack(desc);
            master.setAttackDisplay(attack > 0);
            master.setAttack(attack);

            var armor = desc.getArmor();
            master.setArmorDisplay(armor > 0);
            master.setArmor(armor);

            master.setShield(desc.hasShield);
            master.setStealth(desc.isStealth);
            master.setShackle(desc.isFrozen);
            var cards = cardManager;
            var poolRef = cards.getCardPoolReferenceOfCard(desc.define);
            master.setImageSprite(await cards.loadCardImage(poolRef, desc.define.id));
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
            skill.setCost(desc.getCost());
            skill.setOpen(!desc.isUsed);

            var cards = cardManager;
            var poolRef = cards.getCardPoolReferenceOfCard(desc.define);
            skill.setImageSprite(await cards.loadCardImage(poolRef, desc.define.id));

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

            var cards = cardManager;
            var poolRef = cards.getCardPoolReferenceOfCard(desc.define);
            item.setImageSprite(await cards.loadCardImage(poolRef, desc.define.id));
        }
        /// <summary>
        /// 更新道具的拥有者英雄。
        /// </summary>
        /// <param name="itemId">道具的卡牌ID。</param>
        /// <param name="record">当前的事件记录。</param>
        public void updateItemMaster(int itemId, EventRecord record)
        {
            updateItemMaster(game.getCard(itemId), record);
        }
        /// <summary>
        /// 更新道具的拥有者英雄。
        /// </summary>
        /// <param name="item">道具。</param>
        /// <param name="record">当前的事件记录。</param>
        public void updateItemMaster(ICardData item, EventRecord record)
        {
            var master = item.getOwner().getMaster();
            if (tryGetMaster(master, out var masterUI))
            {
                var snapshot = game.getCardSnapshotOfEvent(master, record);
                setMaster(masterUI, snapshot);
            }
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
        #endregion 道具

        #region 手牌
        public bool isHandVisible(HandItem hand)
        {
            return table.SelfHandList.contains(hand);
        }

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
            if (tryGetHand(desc.id, out var hand))
            {
                UberDebug.LogErrorChannel("UI", "手牌中已经存在" + desc + "对应UI" + hand);
                return hand;
            }
            UberDebug.LogTraceChannel("UI", "创建手牌UI：" + desc);
            HandItem item;
            bool selfHand = desc.getOwner() == player;
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
            setCardDesc(item.card, desc, selfHand);

            var skin = cardManager.getCardSkin(desc.define);
            item.gameObject.name = skin?.name ?? $"Card<{desc.id}>";
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
        /// 重置手牌的各种属性，如拖拽等。
        /// </summary>
        /// <param name="handCard"></param>
        public void resetHand(ICardData handCard)
        {
            if (tryGetHand(handCard, out var usingHand))
            {
                usingHand.card.setVisible(true);
                usingHand.setSpellReady(false);
                var desc = getCardDescriptor(handCard.id);
                setCardDesc(usingHand.card, desc, true);
                usingHand.card.transform.localScale = Vector3.one;
                usingHand.card.transform.localPosition = Vector3.zero;
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
        public Servant createServant(CardDescriptor desc, int position)
        {
            bool isSelf = desc.getOwner() == player;
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
            UberDebug.LogChannel(servant, "UI", "为" + desc + "创建" + servant);
            var skin = cardManager.getCardSkin(desc.define);
            servant.gameObject.name = skin?.name ?? $"Card<{desc.id}>";
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
            servant.setTaunt(desc.isTaunt);
            servant.setShield(desc.hasShield);
            servant.setStealth(desc.isStealth);
            servant.setLastword(desc.hasDeathRattle);
            servant.setLethal(desc.isLethal);
            servant.setLifeDrain(desc.isDrain);
            servant.setShackle(desc.isFrozen);
            servant.setSealed(desc.isSilenced);

            var cards = cardManager;
            var poolRef = cards.getCardPoolReferenceOfCard(desc.define);
            servant.setImageSprite(await cards.loadCardImage(poolRef, desc.define.id));
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
            servant.setLastword(cardDefine.hasDeathRattles());
            servant.setLethal(cardDefine.hasKeyword(KEYWORD.POISONOUS));
            servant.setLifeDrain(cardDefine.hasKeyword(KEYWORD.DRAIN));
            servant.setShackle(cardDefine.hasKeyword(KEYWORD.FROZEN) || cardDefine.hasKeyword(KEYWORD.PERMANENT_FROZEN));
            servant.setSealed(false);

            var cards = cardManager;
            var poolRef = cards.getCardPoolReferenceOfCard(cardDefine);
            servant.setImageSprite(await cards.loadCardImage(poolRef, cardDefine.id));
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
                cardServantDic.Remove(id);
                bool selfRemoved = table.SelfField.removeServant(item);
                bool enemyRemoved = table.EnemyField.removeServant(item);
                item.destroy();
                return true;
            }
            return false;
        }
        #endregion

        #region 揭示
        public UICard createInvokeCard(ICardData card, Vector3 position, float height = -1, bool isFaceUp = true) => createInvokeCard(updateCardDescriptor(card), position, height, isFaceUp);
        public UICard createInvokeCard(CardDescriptor desc, Vector3 position, float height = -1, bool isFaceUp = true)
        {
            var worldPos = TableToWorldPosition(position);
            var card = Instantiate(table.CardTemplate, worldPos, Quaternion.identity, table.InvokeDisplayParent).GetComponent<UICard>();
            card.transform.localRotation = Quaternion.identity;
            setCardDesc(card, desc, isFaceUp);
            if (height < 0)
                return card;
            card.transform.localScale = Vector3.one * height / _cardHeight;
            return card;
        }
        public UICard createInvokeCard(CardDefine define, Vector3 position, float height = -1, bool isFaceUp = true)
        {
            var worldPos = TableToWorldPosition(position);
            var card = Instantiate(table.CardTemplate, worldPos, Quaternion.identity, table.InvokeDisplayParent).GetComponent<UICard>();
            card.transform.localRotation = Quaternion.identity;
            setCardDefine(card, define, isFaceUp);
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

        #region 发现
        public void displayDiscoverDialog(DefineReference[] cardRefs, string title)
        {
            table.Discover.setActive(true);
            table.Discover.setTitleText(string.IsNullOrEmpty(title) ? StringTable.HINT_SELECT_A_CARD : title);
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
            table.Discover.setTitleText(string.IsNullOrEmpty(title) ? StringTable.HINT_SELECT_A_CARD : title);
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
        public Secret createSecret(CardDescriptor desc)
        {
            var master = desc.getOwner() == player ? table.SelfMaster : table.EnemyMaster;
            var headSlots = master.HeadSlots;
            Secret secret = headSlots.createItem();
            headSlots.addItem(secret);
            cardSecretDic.Add(desc.id, secret);
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
        public void setSecret(Secret secret, ICardData card)
        {
            secret.setSecretOpen(currentAnimPlayer != player);
            secret.setMission(card.define.hasKeyword(KEYWORD.MISSION));
        }
        public bool removeSecret(ICardData card) => removeSecret(card.id);
        public bool removeSecret(CardDescriptor desc)
        {
            return removeSecret(desc.id);
        }
        public bool removeSecret(int cardId)
        {
            if (tryGetSecret(cardId, out var secret))
            {
                cardSecretDic.Remove(cardId);
                var desc = getCardDescriptor(cardId);
                var master = desc.getOwner() == player ? table.SelfMaster : table.EnemyMaster;
                var headSlots = master.HeadSlots;
                headSlots.removeItem(secret);
                secret.destroy();
                return true;
            }
            return false;
        }
        #endregion

        #region 特效
        public TableEffect createEffect(DefineReference defRef, VisualEffectInfo info)
        {
            var define = gameManager.getAnimationDefine(defRef);
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
                setCard(item.card, snapshot, true);
                setInitCard(item, card.id);
            }
            dialog.setWaiting(false);
        }
        #endregion
        public Transform getCardTransform(Card card)
        {
            if (tryGetServant(card, out Servant servant))
            {
                return servant.transform;
            }
            else if (tryGetMaster(card, out Master master))
            {
                return master.transform;
            }
            else if (tryGetItem(card, out Item item))
            {
                return item.transform;
            }
            else if (tryGetHand(card, out HandItem hand))
            {
                return hand.transform;
            }
            else if (tryGetSecret(card, out Secret secret))
            {
                return secret.transform;
            }
            else if (tryGetSkill(card, out Skill skill))
            {
                return skill.transform;
            }
            else if (tryGetDeadCardAnchor(card.id, out Transform trans))
            {
                return trans;
            }
            return null;
        }
        public UICard createCard(ICardData card, Vector3 position, float height = -1, bool isFaceUp = true) => createCard(updateCardDescriptor(card), position, height, isFaceUp);
        public UICard createCard(CardDescriptor desc, Vector3 position, float height = -1, bool isFaceUp = true)
        {
            var worldPos = TableToWorldPosition(position);
            var card = Instantiate(table.CardTemplate, worldPos, Quaternion.identity, table.DisplayerCardsParent).GetComponent<UICard>();
            card.transform.localEulerAngles = Vector3.zero;
            setCardDesc(card, desc, isFaceUp);
            if (height >= 0)
                card.transform.localScale = Vector3.one * height / _cardHeight;
            return card;
        }
        public UICard createCard(CardDefine define, Vector3 position, float height = -1, bool isFaceUp = true)
        {
            var worldPos = TableToWorldPosition(position);
            var card = Instantiate(table.CardTemplate, worldPos, Quaternion.identity, table.DisplayerCardsParent).GetComponent<UICard>();
            card.transform.localEulerAngles = Vector3.zero;
            setCardDefine(card, define, isFaceUp);
            if (height >= 0)
                card.transform.localScale = Vector3.one * height / _cardHeight;
            return card;
        }
        public void setCard(UICard table, CardSnapshot card, bool isFaceUp = true) => setCardDesc(table, updateCardDescriptor(card), isFaceUp);
        /// <summary>
        /// 设置卡片UI
        /// </summary>
        /// <param name="card"></param>
        /// <param name="desc"></param>
        /// <param name="isFaceup"></param>
        public async void setCardDesc(UICard card, CardDescriptor desc, bool isFaceup = true)
        {
            var cards = cardManager;
            await card.setCard(desc, cards, this, isFaceup);
            setCardDescriptor(desc);
        }
        public async void setCardDefine(UICard card, CardDefine define, bool isFaceUp = true)
        {
            var cards = cardManager;
            await card.setCardDefine(define, cards, this, isFaceUp);
        }
        /// <summary>
        /// 显示文本提示
        /// </summary>
        /// <param name="tip">翻译的**源文本**</param>
        public void showTip(string tip)
        {
            ui.TipBox.display();
            ui.TipText.text = lang._(tip);
            _tipTimer.start();
        }
        public async void showCardDisplayer(Vector3 targetPosition, CardDescriptor desc)
        {
            var pos = ui.CardDisplayerObj.transform.localPosition;
            pos.x = ui.CardDisplayerObj.transform.parent.InverseTransformPoint(targetPosition).x;
            ui.CardDisplayerObj.transform.localPosition = pos;

            ui.CardDisplayer.setVisible(true);
            setCardDesc(ui.CardDisplayer.card, desc, true);

            var buffViewDatas = await getCardBuffViewDatas(desc.getBuffs());
            ui.CardDisplayer.setBuffs(buffViewDatas);
        }
        public void hideCardDisplayer()
        {
            ui.CardDisplayer.setVisible(false);
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

        public Projectile createProjectile(Projectile prefab, Transform from, Transform to)
        {
            Projectile projectile = Instantiate(prefab, from.position, Quaternion.LookRotation(to.position - from.position, Vector3.forward), table.transform);
            _projectileList.Add(projectile);
            return projectile;
        }


        #endregion
        #region 私有方法

        #region 事件回调
        private void onTurnEndClickCallback(TurnEndButton turnEndButton)
        {
            if (isSelectingTarget())
                return;
            sounds.PlaySE("Button");
            resetUse();
            player.cmdTurnEnd(game);
        }
        private void onTurnEndPointerEnterCallback(TurnEndButton turnEndButton)
        {
            turnEndButton.setHighlighted(true);
        }
        private void onTurnEndPointerExitCallback(TurnEndButton turnEndButton)
        {
            turnEndButton.setHighlighted(false);
        }
        void onClickNoWhere(BattleTable table, PointerEventData pointer)
        {
            cancelTargetSelect();
        }
        #region 道具
        private void onItemPointerEnterCallback(Item item, PointerEventData pointer)
        {
            _showCardTimer.start();
            _hoveredItem = item;
        }

        private void onItemPointerExitCallback(Item item, PointerEventData pointer)
        {
            _showCardTimer.reset();
            _hoveredItem = null;
            hideCardDisplayer();
        }
        #endregion 道具

        #region 手牌
        private void onHandListBlockerClickCallback(HandList list)
        {
            if (list != table.SelfHandList) return;

            if (list.isZoomIn)
                list.zoomOut();
        }
        private void onHandItemClickCallback(HandItem hand, HandList list, PointerEventData pointer)
        {
            if (list != table.SelfHandList)
                return;

            var card = getCard(hand);
            if (usingCard == card) // 点击正在拖动的卡牌时不会缩进
                return;

            if (list.isZoomIn)
                list.zoomOut();
            else
                list.zoomIn();
        }
        private void onHandItemBeginDragCallback(HandItem hand, HandList list, PointerEventData pointer)
        {
            if (!hand.isDraggable)
                return;
            Card card = getCard(hand);
            if (isSelectingTargetForOtherCards(card))//已经在用别的牌了
                return;
            usingCard = card;
            list.ignoreAlignHand = hand;
        }
        private void onHandItemDragCallback(HandItem hand, HandList list, PointerEventData pointer)
        {
            if (!hand.isDraggable)
                return;
            Card card = getCard(hand);
            if (isSelectingTargetForOtherCards(card)) //不存在使用中的卡牌
                return;
            if (usingCard != card)
            {
                resetUse();
                return;
            }

            bool setPosition = true;
            // 拖回手牌
            var worldPos = pointer.pointerCurrentRaycast.worldPosition;
            var pos = WorldToTablePosition(worldPos);
            var inverseTransPosition = table.SelfHandList.transform.InverseTransformPoint(worldPos);
            if (table.SelfHandList.containsPosition(inverseTransPosition))
            {
                //移除随从占位
                if (card.isServant())
                    table.SelfField.setPreviewPosition(-1);
            }
            // 卡牌不可用
            else if (!canCardBeControlled(card, out string info))
            {
                //停止拖拽并提示
                showTip(info);
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
            //拖拽卡片
            if (setPosition)
            {
                var handPos = pos;
                handPos.y = hand.transform.position.y;
                hand.transform.position = TableToWorldPosition(pos);
            }
        }
        /// <summary>
        /// 手牌拖拽结束
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="pointer"></param>
        private void onHandItemEndDragCallback(HandItem hand, HandList list, PointerEventData pointer)
        {
            if (list != table.SelfHandList)//你不能用别人的卡
                return;
            Card card = getCard(hand);
            if (usingCard != card)
            {
                resetUse();
                return;
            }
            // 拖回手牌
            var worldPos = pointer.pointerCurrentRaycast.worldPosition;
            var pos = WorldToTablePosition(worldPos);
            var inverseTransPosition = table.SelfHandList.transform.InverseTransformPoint(worldPos);
            if (table.SelfHandList.containsPosition(inverseTransPosition))
            {
                //如果松开，取消使用
                resetUse();
                return;
            }
            //卡牌不可用
            if (!canCardBeControlled(card, out string info))
            {
                showTip(info);
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
                    _servantUsingPosition = index;
                    _servantPreviewAnim = new HandToFieldAnim(pos, getCardDescriptor(card), index);
                    addAnim(_servantPreviewAnim);
                    table.SelfHandList.zoomOut();//手牌缩小
                    removeHand(card);
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
                    table.SelfHandList.zoomOut();//手牌缩小
                    removeHand(card);
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
            if (list == table.SelfHandList)
                hand.setHovered(true);
            Player targetPlayer = list == table.SelfHandList ? player : game.getOpponent(player);
            if (!table.SelfHandList.isZoomIn && targetPlayer.getHand().count > 0)
            {
                var pos = pointer.pointerCurrentRaycast.worldPosition;
                var tablePos = WorldToTablePosition(pos);
                tablePos.y = 0;
                showTipBox(lang._(TAB_CARD_NUMBER, getCard(hand).getOwner().getHand().count), tablePos, hand.gameObject);
            }
        }
        private void onHandItemPointerExitCallback(HandItem hand, HandList list, PointerEventData pointer)
        {
            hand.setHovered(false);
            ui.TipBox.hide();
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
            Card card = getCard(master);
            if (isSelectingTargetForOtherCards(card))//当前正在操作的卡牌不是这个英雄
                return;
            if (!canCardBeControlled(card))
                return;
            //拉动距离也应该有一个阈值
            var pos = WorldToTablePosition(pointer.pointerCurrentRaycast.worldPosition);
            var masterPos = WorldToTablePosition(master.transform.position);
            if (Vector3.Distance(masterPos, pos) > masterAttackThreshold)
            {
                var targets = game.findAllCardsInField(c => card.isAttackable(game, player, c, out _));
                enterTargetSelectMode(TargetSelectMode.Attack, card, targets);
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
            Card card = getCard(master);
            if (!isSelectingTargetForCard(card))//当前正在操作的卡牌不是这个英雄，或者没有正在操作的卡牌
                return;
            if (!canCardBeControlled(card))
                return;
            //如果在随从上面
            if (isOnTarget(pointer, out var target))
            {
                if (card.isAttackable(game, player, target, out var tip))
                {
                    player.cmdAttack(game, card, target);
                    resetUse(finishSelectTarget: true);
                    return;
                }
                else
                    showTip(tip);
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
            var skillCard = player.getSkill();
            if (isSelectingTargetForOtherCards(skillCard))//已经在用别的牌了，不能点技能
                return;
            if (!canCardBeControlled(skillCard, out string info))
            {
                showTip(info);
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
            //TODO:按帧记录Replay
            if (!canControl)
                return;
            var skillCard = player.getSkill();
            if (isSelectingTargetForOtherCards(skillCard)) //正在操作的卡牌不是当前技能
                return;
            // 不可用。
            if (!canCardBeControlled(skillCard, out var info))
            {
                showTip(info);
                return;
            }
            var pos = WorldToTablePosition(pointer.pointerCurrentRaycast.worldPosition);
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
            {
                cancelTargetSelect();
                return;
            }
            var skillCard = player.getSkill();
            if (!isSelectingTargetForCard(skillCard))
                return;//在用其他卡片
            if (!canCardBeControlled(skillCard, out string info))
            {
                showTip(info);
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
                    showTip(invalidMsg);
            }
            cancelTargetSelect();
        }
        private void onSkillPointerExitCallback(Skill skill, PointerEventData pointer)
        {
            ui.TipBox.hide();
        }

        private void onSkillPointerEnterCallback(Skill skill, PointerEventData pointer)
        {
            string desc = getSkin(skill == table.SelfSkill ? player.getSkill() : game.getOpponent(player).getSkill()).desc;
            var pos = pointer.pointerCurrentRaycast.worldPosition;
            var tablePos = WorldToTablePosition(pos);
            tablePos.y = 0;
            showTipBox(desc, tablePos, skill.gameObject);
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
            // （目标选择模式）
            trySelectTargetForBattlecry(getCard(master));
        }

        private void onServantPointerEnterCallback(Servant servant, Field field, PointerEventData pointer)
        {
            _showCardTimer.start();
            _hoveredServant = servant;
        }
        private void onServantPointerExitCallback(Servant servant, Field field, PointerEventData pointer)
        {
            _showCardTimer.reset();
            _hoveredServant = null;
            hideCardDisplayer();
        }
        /// <summary>
        /// 随从拖拽处理
        /// </summary>
        /// <param name="servant"></param>
        /// <param name="pointer"></param>
        private void onServantDragCallback(Servant servant, Field field, PointerEventData pointer)
        {
            Card card = getCard(servant);
            if (!canCardBeControlled(card))
                return;
            if (isSelectingTargetForOtherCards(card))//正在操作的卡牌并非该仆从。
                return;
            //拉动距离也应该有一个阈值
            var pos = WorldToTablePosition(pointer.pointerCurrentRaycast.worldPosition);
            var servantPos = WorldToTablePosition(servant.transform.position);
            if (Vector3.Distance(servantPos, pos) > servantAttackThreshold)
            {
                var targets = game.findAllCardsInField(c => card.isAttackable(game, player, c, out _));
                enterTargetSelectMode(TargetSelectMode.Attack, card, targets);
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
            Card card = getCard(servant);
            if (!isSelectingTargetForCard(card))//正在操作的卡牌并非该仆从，或者没有正在操作的卡牌。
                return;
            if (!canCardBeControlled(card))
                return;
            //如果在随从或英雄上面
            if (isOnTarget(pointer, out var target))
            {
                if (card.isAttackable(game, player, target, out var tip))
                {
                    player.cmdAttack(game, card, target);
                    resetUse(finishSelectTarget: true);
                    return;
                }
                else
                    showTip(tip);
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
            ui.PilePanel.setActive(true);
            ui.PilePanel.setTitleText(lang._(TAB_YOUR_BOX));
            ui.PilePanel.clearCards();
            foreach (var card in player.getDeck().OrderBy(c => c.define.getCost()))
            {
                var item = ui.PilePanel.addCard();
                setCard(item.card, newSnapshot(card), true);
            }
        }
        private void onDeckPointerEnterCallback(Deck deck, PointerEventData pointer)
        {
            bool self = deck == table.SelfDeck;
            var curDeck = (self ? player : game.getOpponent(player)).getDeck();
            string tip;
            if (curDeck.count > 0)
            {
                tip = lang._(TAB_BOX_HAS, curDeck.count);
            }
            else
            {
                tip = lang._(self ? TAB_YOUR_NO_HAVE_CARD : TAB_OPPONENT_OUT_OF_CARDS);
            }
            var pos = pointer.pointerCurrentRaycast.worldPosition;
            var tablePos = WorldToTablePosition(pos);
            tablePos.y = 0;
            showTipBox(tip, tablePos, deck.gameObject);
        }
        private void onDeckPointerExitCallback(Deck deck, PointerEventData pointer)
        {
            ui.TipBox.hide();
        }
        #endregion

        #region 墓地


        private void onGraveClickCallback(Deck deck, PointerEventData pointer)
        {
            if (isSelectingTarget())
                return;
            ui.PilePanel.setActive(true);
            ui.PilePanel.setTitleText(lang._(TAB_YOUR_GRAVEYARD));
            ui.PilePanel.clearCards();
            foreach (var card in player.getGrave())
            {
                var item = ui.PilePanel.addCard();
                setCard(item.card, newSnapshot(card), true);
            }
        }
        private void onGravePointerEnterCallback(Deck deck, PointerEventData pointer)
        {
            var curGrave = (deck == table.SelfGrave ? player : game.getOpponent(player)).getGrave();
            string tip = curGrave.count > 0 ?
                lang._(TAB_GRAVEYARD_HAS, curGrave.count) :
                lang._(TAB_GRAVEYARD_NULL);
            var pos = pointer.pointerCurrentRaycast.worldPosition;
            var tablePos = WorldToTablePosition(pos);
            tablePos.y = 0;
            showTipBox(tip, tablePos, deck.gameObject);
        }
        private void onGravePointerExitCallback(Deck deck, PointerEventData pointer)
        {
            ui.TipBox.hide();
        }
        #endregion

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
            table.SelfHandList.zoomOut();//手牌缩小
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
                showTip(invalidMsg);
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
                else if (tryGetServant(source, out Servant servant))
                {
                    sourceTransform = servant.transform;
                }
                else if (tryGetMaster(source, out Master master))
                {
                    sourceTransform = master.transform;
                }
                else if (tryGetSkill(source, out Skill skill))
                {
                    sourceTransform = skill.transform;
                }
                else if (tryGetHand(source, out HandItem hand))
                {
                    sourceTransform = hand.transform;
                }
            }
            //拖出箭头
            var sourcePos = WorldToTablePosition(sourceTransform.position);
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
            // 如果没有手牌。
            if (!tryGetHand(card, out _))
            {
                int handPosition = card.pile.indexOf(card);
                // 强制完成随从的入场预览动画。
                if (_servantPreviewAnim != null)
                {
                    _servantPreviewAnim.forceFinish(this);
                    _servantPreviewAnim = null;
                    addAnim(new FieldToHandAnim(card.id, handPosition));
                }
                else
                {
                    // 如果有这个随从，播放回手动画。
                    if (tryGetServant(card, out _))
                    {
                        addAnim(new FieldToHandAnim(card.id, handPosition));
                    }
                    else //没有这个随从，直接创建手牌。
                    {
                        createHand(getCardDescriptor(card), handPosition);
                    }
                }
            }
            return true;
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
                    showTip(invalidMsg);
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
            // TODO: 需要改为“是否正在使用点击来选择目标”
            if (targetSelectMode != TargetSelectMode.Use)
                return false;
            return targetSelectingCard != null && (targetSelectingCard.isServant() || targetSelectingCard.isItem());
        }

        #endregion
        private async Task<CardBuffListItemViewData[]> getCardBuffViewDatas(IEnumerable<Buff> buffs)
        {
            // 增益列表。
            var cards = cardManager;

            // 合并同样的增益。
            var buffCountList = new List<(DefineReference defRef, int count)>();
            DefineReference curBuffRef = null;
            foreach (var buff in buffs)
            {
                var defineRef = buff is GeneratedBuff genBuff ? genBuff.defineRef : new DefineReference(0, buff.id);
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
                    sprite = cards.getDefaultSprite();
                    name = lang._(CARD_BUFF_UNKNOWN_NAME);
                    desc = lang._(CARD_BUFF_UNKNOWN_DESC);
                }
                else
                {
                    var skin = cards.getBuffSkin(buffDefine);
                    sprite = await cards.loadImage(skin.imagePath, defineRef.cardPoolId);
                    name = skin.name ?? string.Empty;
                    desc = skin.desc ?? string.Empty;
                }
                if (name.Length > _maxBuffNameLength)
                    name = lang._(CARD_BUFF_LIST_TRIM_TEMPLATE, name.Substring(0, _maxBuffNameLength));
                if (desc.Length > _maxBuffDescLength)
                    desc = lang._(CARD_BUFF_LIST_TRIM_TEMPLATE, desc.Substring(0, _maxBuffDescLength));
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
            table.TurnEndButton.setInteractable(game.getCurrentPlayer() == player && !_requesting && canControl);
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
            bool highlight = owner == player && game.getCurrentPlayer() == player && canControl && !_requesting;
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
        private const string TAB_SHOW = "显示";
        [TranslateMsg]
        private const string TAB_HIDE = "隐藏";
        [TranslateMsg]
        private const string TAB_YOUR_BOX = "你的牌库";
        [TranslateMsg]
        private const string TAB_BOX_HAS = "牌库里有{0}张卡牌";
        [TranslateMsg]
        private const string TAB_YOUR_NO_HAVE_CARD = "你已经没有卡牌了";
        [TranslateMsg]
        private const string TAB_OPPONENT_OUT_OF_CARDS = "你的对手已经没有卡牌了";
        [TranslateMsg("游戏中的增益列表项的未知增益名称")]
        public const string CARD_BUFF_UNKNOWN_NAME = "未知增益";
        [TranslateMsg("游戏中的增益列表项的未知增益描述")]
        public const string CARD_BUFF_UNKNOWN_DESC = "不知道这是干什么的……";
        [TranslateMsg("游戏中的增益列表项的增益名称模板，在同样的增益属性多于1时显示（{0}：增益名称；{1}：增益数量）")]
        public const string CARD_BUFF_LIST_MULTIPLE_TEMPLATE = "{0}×{1}";
        [TranslateMsg("游戏中的增益列表项的增益文本模板，在过长时显示（{0}：被截断的增益文字）")]
        public const string CARD_BUFF_LIST_TRIM_TEMPLATE = "{0}...";

        /// <summary>
        /// 当前是否可以控制（是否是自己的回合）
        /// </summary>
        public bool canControl { get; set; }
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
        private CardDescriptor _selfItemDesc;
        private CardDescriptor _enemyItemDesc;
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
        int _maxBuffDisplayCount = 15;
        [SerializeField]
        int _maxBuffNameLength = 10;
        [SerializeField]
        int _maxBuffDescLength = 32;
        [SerializeField]
        List<Projectile> _projectileList = new List<Projectile>();
        [SerializeField]
        BJSYGameCore.Timer _tipTimer = new BJSYGameCore.Timer() { duration = 2 };
        [SerializeField]
        BJSYGameCore.Timer _showCardTimer = new BJSYGameCore.Timer() { duration = 0.25f };
        Servant _hoveredServant;
        Item _hoveredItem;
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
