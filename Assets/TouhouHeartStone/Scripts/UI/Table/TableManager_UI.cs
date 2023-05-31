using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BJSYGameCore.UI;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using TouhouHeartstone.Builtin;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Card = TouhouCardEngine.Card;
using UICard = UI.Card;

namespace Game
{
    public partial class TableManager
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

        #region 英雄
        /// <summary>
        /// 初始化Master的事件
        /// </summary>
        /// <param name="master"></param>

        public void initMaster(Master master)
        {
            master.onDrag.add(onDragMaster);
            master.onDragEnd.add(onDragEndMaster);
            master.onClick.add(onClickMaster);
            master.Head.clearSecret();
            master.LifePropNumber.WhiteText.invokeBeforeAnim();
            master.asAnimator.SetBool("isShackle", false);
            master.onCanAttackFalse.invokeBeforeAnim();
            master.HighlightImage.hide();
            master.AttackPropNumber.hide();
            master.asAnimator.SetBool("hasArmor", false);
            master.HealImage.hide();
            master.Image.color = new Color(1, 1, 1, 1);
            resetMasterPosition(master);
            master.rectTransform.localScale = new Vector3(1, 1, 1);
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
                master = ui.SelfMaster;
                return true;
            }
            else if (game.getOpponent(player).getMaster().id == cardId)
            {
                master = ui.EnemyMaster;
                return true;
            }
            else
            {
                master = null;
                return false;
            }
        }
        /// <summary>
        /// 设置Master的UI
        /// </summary>
        /// <param name="master"></param>
        /// <param name="card"></param>
        /// <param name="isSelectable"></param>
        public void setMaster(Master master, CardSnapshot card, bool isSelectable = false) => setMaster(master, updateCardDescriptor(card), isSelectable);
        /// <summary>
        /// 设置Master的UI
        /// </summary>
        /// <param name="master"></param>
        /// <param name="desc"></param>
        /// <param name="isSelectable"></param>
        public async void setMaster(Master master, CardDescriptor desc, bool isSelectable = false)
        {
            master.LifePropNumber.text.text = desc.getCurrentLife().ToString();
            if (desc.getAttack() > 0)
            {
                master.AttackPropNumber.text.text = desc.getAttack().ToString();
                master.asAnimator.SetBool("ShowAttack", true);
                master.AttackPropNumber.text.display();
            }
            else
            {
                master.AttackPropNumber.text.hide();
                master.asAnimator.SetBool("ShowAttack", false);
            }
            if (desc.getArmor() > 0)
            {
                master.ArmorPropNumber.text.text = desc.getArmor().ToString();
                master.ArmorPropNumber.display();
            }
            else
            {
                master.ArmorPropNumber.hide();
            }

            if (isSelectable)
            {
                // master.HighlightController = Master.Highlight.Yellow;
                master.onSelectableTrue.invokeBeforeAnim();
                master.onSelectableTrue.invokeAfterAnim();
            }
            else
            {
                master.onSelectableFalse.invokeBeforeAnim();
                master.onSelectableFalse.invokeAfterAnim();
            }

            if (desc.hasShield)
                master.ShieldImage.display();
            else
                master.ShieldImage.hide();

            if (desc.isFrozen)
            {
                master.onShackleTrue.invokeBeforeAnim();
                master.onShackleTrue.invokeAfterAnim();
            }
            else
            {
                master.onShackleFalse.invokeBeforeAnim();
                master.onShackleFalse.invokeAfterAnim();
            }
            var cards = getManager<CardManager>();
            var poolRef = cards.getCardPoolReferenceOfCard(desc.define);
            master.Image.sprite = await cards.loadCardImage(poolRef, desc.define.id);
            setCardControlDirty(desc.id);
        }
        /// <summary>
        /// 用于重置非正常退出后的英雄位置
        /// </summary>
        /// <param name="ReMaster"></param>
        public void resetMasterPosition(Master ReMaster)
        {
            ReMaster.GetComponentInChildren<PositionLerp>()._time = 0.0f;
            ReMaster.GetComponentInChildren<PositionLerp>().enabled = false;
            ReMaster.rootRectTransform.anchoredPosition = new Vector3(0, 0, 0);
        }
        /// <summary>
        /// 从Master获取对应的牌
        /// </summary>
        /// <param name="master"></param>
        /// <returns></returns>
        public Card getCard(Master master)
        {
            if (master == ui.SelfMaster)
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
                skill = ui.SelfSkill;
                return true;
            }
            else if (cardId == game.getOpponent(player).getSkill()?.id)
            {
                skill = ui.EnemySkill;
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
            skill.costPropNumber.text.text = desc.getCost().ToString();
            skill.isUsed = desc.isUsed;

            var cards = getManager<CardManager>();
            var poolRef = cards.getCardPoolReferenceOfCard(desc.define);
            skill.image.sprite = await cards.loadCardImage(poolRef, desc.define.id);

            setCardControlDirty(desc.id);
        }
        public Card getSkillCard(Skill skill)
        {
            if (skill == ui.SelfSkill)
            {
                return player.getSkill();
            }
            else if (skill == ui.EnemySkill)
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
                item = ui.SelfItem;
                return true;
            }
            else if (_enemyItemDesc?.id == cardId)
            {
                item = ui.EnemyItem;
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
                item.hide();
                if (item == ui.SelfItem)
                    _selfItemDesc = null;
                else if (item == ui.EnemyItem)
                    _enemyItemDesc = null;
                return;
            }
            item.display();
            if (item == ui.SelfItem)
                _selfItemDesc = desc;
            else if (item == ui.EnemyItem)
                _enemyItemDesc = desc;

            if (desc.getOwner() == currentAnimPlayer)
            {
                if (item.animator.gameObject.activeSelf)
                {
                    item.onItemON.invokeBeforeAnim();
                    item.onItemON.invokeAfterAnim();
                }
            }
            else
            {
                if (item.animator.gameObject.activeSelf)
                {
                    item.onItemOff.invokeBeforeAnim();
                    item.onItemOff.invokeAfterAnim();
                }
            }

            item.AttackPropNumber.text.text = desc.getAttack().ToString();
            item.DurabilityPropNumber.text.text = desc.getCurrentLife().ToString();

            var cards = getManager<CardManager>();
            var poolRef = cards.getCardPoolReferenceOfCard(desc.define);
            item.Image.sprite = await cards.loadCardImage(poolRef, desc.define.id);
        }
        #endregion 道具

        #region 手牌

        /// <summary>
        /// 创建一张手牌
        /// </summary>
        /// <param name="desc"></param>
        /// <param name="position">默认为-1也就是最右手</param>
        /// <returns></returns>
        public HandListItem createHand(ICardData card, int position = -1) => createHand(updateCardDescriptor(card), position);
        /// <summary>
        /// 创建一张手牌
        /// </summary>
        /// <param name="desc"></param>
        /// <param name="position">默认为-1也就是最右手</param>
        /// <returns></returns>
        public HandListItem createHand(CardDescriptor desc, int position = -1)
        {
            if (tryGetHand(desc.id, out var hand))
            {
                UberDebug.LogErrorChannel("UI", "手牌中已经存在" + desc + "对应UI" + hand);
                return hand;
            }
            UberDebug.LogTraceChannel("UI", "创建手牌UI：" + desc);
            HandListItem item;
            if (desc.getOwner() == player)
            {
                item = ui.SelfHandList.addItem();
                if (position >= 0)
                {
                    ui.SelfHandList.defaultItem.rectTransform.SetAsFirstSibling();
                    item.rectTransform.SetSiblingIndex(position + 1);
                }
                item.isDragable = true;
                setCardDesc(item.Card, desc, true);
            }
            else
            {
                item = ui.EnemyHandList.addItem();
                if (position >= 0)
                {
                    ui.EnemyHandList.defaultItem.rectTransform.SetAsFirstSibling();
                    item.rectTransform.SetSiblingIndex(position + 1);
                }
                item.isDragable = false;
                item.Card.setBoolFalse("isFaceUp");
                setCardDesc(item.Card, desc, false);
            }
            item.gameObject.name = desc.ToString();
            item.onDrag.set(onDragHand);
            item.onEndDrag.set(onDragHandEnd);
            item.onPointerEnter.set(onHandPointerEnter);
            item.onPointerExit.set(onHandPointerExit);
            item.onPointerMove.set(onHandPointerMove);
            cardHandDic.Add(desc.id, item);
            return item;
        }

        /// <summary>
        /// 获取UI手牌引用
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public HandListItem getHand(ICardData card) => getHand(card.id);
        /// <summary>
        /// 尝试获取UI手牌引用
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public bool tryGetHand(ICardData card, out HandListItem hand) => tryGetHand(card?.id ?? 0, out hand);

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
        public HandListItem getHand(int cardId)
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
        public bool tryGetHand(int id, out HandListItem hand)
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

                var card = getCardDescriptor(id);
                if (card?.getOwner() == player)
                {
                    ui.SelfHandList.removeItem(item);
                }
                else
                {
                    ui.EnemyHandList.removeItem(item);
                }
            }
            return false;
        }
        /// <summary>
        /// 从UI手牌引用获取对应的牌
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Card getCard(HandListItem item)
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
        public ServantItem createServant(ICardData card, int position) => createServant(updateCardDescriptor(card), position);
        /// <summary>
        /// 创建一个随从
        /// </summary>
        /// <param name="desc"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public ServantItem createServant(CardDescriptor desc, int position)
        {
            ServantItem servant;
            if (desc.getOwner() == player)
            {
                servant = ui.SelfFieldList.addItem();
                ui.SelfFieldList.defaultItem.rectTransform.SetAsFirstSibling();
            }
            else
            {
                servant = ui.EnemyFieldList.addItem();
                ui.EnemyFieldList.defaultItem.rectTransform.SetAsFirstSibling();
            }
            UberDebug.LogChannel(servant, "UI", "为" + desc + "创建" + servant);
            servant.gameObject.name = desc.ToString();
            servant.rectTransform.SetSiblingIndex(position + 1);
            setServant(servant, desc);
            servant.onClick.add(onClickServant);
            servant.onDrag.add(onDragServant);
            servant.onDragEnd.add(onDragEndServant);
            servant.onPointerEnter.set(onEnterServant);
            servant.onPointerExit.set(onExitServant);
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
        public void setServant(ServantItem servant, CardSnapshot card) => setServant(servant, updateCardDescriptor(card));
        /// <summary>
        /// 设置随从
        /// </summary>
        /// <param name="servant"></param>
        /// <param name="desc"></param>
        /// <param name="isSelectable"></param>
        public async void setServant(ServantItem servant, CardDescriptor desc)
        {
            servant.attackPropNumber.text.text = desc.getAttack().ToString();
            servant.lifePropNumber.text.text = desc.getCurrentLife().ToString();

            var tauntAnim = desc.isTaunt ? servant.onTauntTrue : servant.onTauntFalse;
            tauntAnim.invokeBeforeAnim();
            tauntAnim.invokeAfterAnim();

            var shieldAnim = desc.hasShield ? servant.onShieldTrue : servant.onShieldFalse;
            shieldAnim.invokeBeforeAnim();
            shieldAnim.invokeAfterAnim();

            var stealthAnim = desc.isStealth ? servant.onStealthTrue : servant.onStealthFalse;
            stealthAnim.invokeBeforeAnim();
            stealthAnim.invokeAfterAnim();

            var deathrattleAnim = desc.hasDeathRattle ? servant.onDyingWishTrue : servant.onDyingWishFalse;
            deathrattleAnim.invokeBeforeAnim();
            deathrattleAnim.invokeAfterAnim();

            var lethalAnim = desc.isLethal ? servant.onLethalTrue : servant.onLethalFalse;
            lethalAnim.invokeBeforeAnim();
            lethalAnim.invokeAfterAnim();

            var drainAnim = desc.isDrain ? servant.onLifeStealTrue : servant.onLifeStealFalse;
            drainAnim.invokeBeforeAnim();
            drainAnim.invokeAfterAnim();

            var frozenAnim = desc.isFrozen ? servant.onShackleTrue : servant.onShackleFalse;
            frozenAnim.invokeBeforeAnim();
            frozenAnim.invokeAfterAnim();

            var silencedAnim = desc.isSilenced ? servant.onSilenceTrue : servant.onSilenceFalse;
            silencedAnim.invokeBeforeAnim();
            silencedAnim.invokeAfterAnim();

            var cards = getManager<CardManager>();
            var poolRef = cards.getCardPoolReferenceOfCard(desc.define);
            servant.image.sprite = await cards.loadCardImage(poolRef, desc.define.id);

            setCardControlDirty(desc.id);
        }
        /// <summary>
        /// 设置随从定义
        /// </summary>
        /// <param name="servant"></param>
        /// <param name="cardDefine"></param>
        public async void setServant(ServantItem servant, CardDefine cardDefine)
        {
            servant.attackPropNumber.text.text = cardDefine.getAttack().ToString();
            servant.lifePropNumber.text.text = cardDefine.getLife().ToString();
            if (cardDefine.hasKeyword(KEYWORD.TAUNT))
            {
                servant.onTauntTrue.invokeBeforeAnim();
                servant.onTauntTrue.invokeAfterAnim();
            }
            else
            {
                servant.onTauntFalse.invokeBeforeAnim();
                servant.onTauntFalse.invokeAfterAnim();
            }
            var cards = getManager<CardManager>();
            var poolRef = cards.getCardPoolReferenceOfCard(cardDefine);
            servant.image.sprite = await cards.loadCardImage(poolRef, cardDefine.id);
        }
        /// <summary>
        /// 从卡获取对应的随从UI
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public ServantItem getServant(ICardData card) => getServant(card.id);
        /// <summary>
        /// 从卡获取对应的随从UI
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ServantItem getServant(int id)
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
        public bool tryGetServant(ICardData card, out ServantItem servant) => tryGetServant(card?.id ?? 0, out servant);
        /// <summary>
        /// 尝试从卡获取对应的随从UI
        /// </summary>
        /// <param name="card"></param>
        /// <param name="servant"></param>
        /// <returns></returns>
        public bool tryGetServant(int id, out ServantItem servant)
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
        public Card getCard(ServantItem servant)
        {
            foreach (var pair in cardServantDic)
            {
                if (pair.Value == servant)
                    return game.getCard(pair.Key);
            }
            return null;
        }
        /// <summary>
        /// 隐藏随从的占位符
        /// </summary>
        private void hideServantPlaceHolder()
        {
            ui.addChild(ui.ServantPlaceHolder.rectTransform);
            ui.ServantPlaceHolder.Servant.hide();
            ui.ServantPlaceHolder.hide();
        }
        #endregion

        #region 揭示
        public UICard createInvokeCard(ICardData card, Vector3 position, float height = -1, bool isFaceUp = true) => createInvokeCard(updateCardDescriptor(card), position, height, isFaceUp);
        public UICard createInvokeCard(CardDescriptor card, Vector3 position, float height = -1, bool isFaceUp = true)
        {
            var ui = Instantiate(cardPrefab, position, Quaternion.identity, this.ui.InvokeDisPlayRectTransform);
            setCardDesc(ui, card, isFaceUp);
            if (height < 0)
                return ui;
            ui.transform.localScale = Vector3.one * height / ui.rectTransform.rect.height;
            return ui;
        }
        public UICard createInvokeCard(CardDefine define, Vector3 position, float height = -1, bool isFaceUp = true)
        {
            var ui = Instantiate(cardPrefab, position, Quaternion.identity, this.ui.InvokeDisPlayRectTransform);
            setCardDefine(ui, define, isFaceUp);
            if (height < 0)
                return ui;
            ui.transform.localScale = Vector3.one * height / ui.rectTransform.rect.height;
            return ui;
        }
        /// <summary>
        /// 删除强制退出过程之中产生的揭示动画
        /// </summary>
        /// <param name="master"></param>
        public void DestroyInvokeCard()
        {
            Transform[] Cards = ui.InvokeDisPlayRectTransform.GetComponentsInChildren<Transform>();
            for (int i = 1; i < ui.InvokeDisPlayRectTransform.GetComponentsInChildren<Transform>().Count(); i++)
            {
                Destroy(Cards[i].gameObject);
            }
        }
        #endregion

        #region 发现
        public void displayDiscoverDialog(DefineReference[] cardRefs, string title)
        {
            ui.Discover.display();
            if (string.IsNullOrEmpty(title))
                ui.Discover.Text.text = StringTable.HINT_SELECT_A_CARD;
            else
                ui.Discover.Text.text = title;
            LayoutRebuilder.ForceRebuildLayoutImmediate(ui.Discover.Text.rectTransform);
            ui.Discover.HoriCardList.clearItems();
            foreach (var cardRef in cardRefs)
            {
                var item = ui.Discover.HoriCardList.addItem();
                var cardDefine = game.getDefine(cardRef.cardPoolId, cardRef.defineId);
                setCardDefine(item.Card, cardDefine, true);
                item.asButton.onClick.set(() =>
                {
                    game.answers.answer(player.id, new DiscoverResponse()
                    {
                        cardDefRef = cardDefine.getDefineRef()
                    });
                    closeDiscoverDialog();
                });
            }
        }
        public void displayDiscoverDialog(int[] cardIds, string title)
        {
            ui.Discover.display();
            if (string.IsNullOrEmpty(title))
                ui.Discover.Text.text = StringTable.HINT_SELECT_A_CARD;
            else
                ui.Discover.Text.text = title;
            LayoutRebuilder.ForceRebuildLayoutImmediate(ui.Discover.Text.rectTransform);
            ui.Discover.HoriCardList.clearItems();
            foreach (var cardId in cardIds)
            {
                var item = ui.Discover.HoriCardList.addItem();
                var card = game.getCard(cardId);
                if (card != null)
                {
                    setCard(item.Card, newSnapshot(card), true);
                    item.asButton.onClick.set(() =>
                    {
                        game.answers.answer(player.id, new DiscoverResponse()
                        {
                            cardId = card.id
                        });
                        closeDiscoverDialog();
                    });
                }
            }
        }
        public void closeDiscoverDialog()
        {
            ui.Discover.hide();
        }
        #endregion

        #region 陷阱
        public Secret createSecret(ICardData card) => createSecret(updateCardDescriptor(card));
        public Secret createSecret(CardDescriptor desc)
        {
            Secret secret;
            if (desc.getOwner() == player)
                secret = ui.SelfMaster.Head.addSecret();
            else
                secret = ui.EnemyMaster.Head.addSecret();
            cardSecretDic.Add(desc.id, secret);
            return secret;
        }
        public bool tryGetSecret(ICardData card, out Secret secret) => tryGetSecret(card?.id ?? 0, out secret);
        public bool tryGetSecret(int id, out Secret secret)
        {
            return cardSecretDic.TryGetValue(id, out secret);
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
                DestroyImmediate(secret.gameObject);
                ui.EnemyMaster.Head.UpdateSercret();
                ui.SelfMaster.Head.UpdateSercret();
                return true;
            }
            return false;
        }
        #endregion

        #region 初始换牌
        public Card getInitCard(InitReplaceCardListItem init)
        {
            foreach (var pair in initDic)
            {
                if (pair.Value == init)
                    return game.getCard(pair.Key);
            }
            return null;
        }
        public void setInitCard(InitReplaceCardListItem init, CardSnapshot card)
        {
            setCard(init.Card, card, true);
            init.MarkImage.enabled = false;
            if (initDic.ContainsKey(card.id))
            {
                initDic[card.id] = init;
            }
            else
            {
                initDic.Add(card.id, init);
            }
        }
        public void showInitReplaceDialog(InitReplaceRequest request)
        {
            var dialog = ui.InitReplaceDialog;
            if (!dialog.isDisplaying)
            {
                dialog.display();

                var cardList = dialog.InitReplaceCardList;
                var initPile = player.getInit();
                cardList.clearItems();
                cardList.updateItems(initPile,
                    (item, card) =>
                    {
                        return getInitCard(item) == card;
                    },
                    (item, card) =>
                    {
                        item.asButton.onClick.set(() =>
                        {
                            item.MarkImage.enabled = !item.MarkImage.enabled;
                        });
                        setInitCard(item, newSnapshot(card));
                    });

                cardList.sortItems((a, b) => initPile.indexOf(getInitCard(a)) - initPile.indexOf(getInitCard(b)));

                dialog.WaitingText.SetActive(false);

                dialog.ConfirmButtonBlack.gameObject.SetActive(true);
                dialog.ConfirmButtonBlack.asButton.onClick.set(() =>
                {
                    player.cmdInitReplace(game, cardList.Where(item => item.MarkImage.enabled).Select(item => getInitCard(item)).ToArray());
                    dialog.ConfirmButtonBlack.gameObject.SetActive(false);
                    dialog.WaitingText.SetActive(true);
                });
            }
        }
        #endregion
        public UICard createCard(ICardData card, Vector3 position, float height = -1, bool isFaceUp = true) => createCard(updateCardDescriptor(card), position, height, isFaceUp);
        public UICard createCard(CardDescriptor desc, Vector3 position, float height = -1, bool isFaceUp = true)
        {
            var ui = Instantiate(cardPrefab, position, Quaternion.identity, this.ui.FieldsImage.rectTransform);
            setCardDesc(ui, desc, isFaceUp);
            if (height < 0)
                return ui;
            ui.transform.localScale = Vector3.one * height / ui.rectTransform.rect.height;
            return ui;
        }
        public void setCard(UICard ui, CardSnapshot card, bool isFaceUp = true) => setCardDesc(ui, updateCardDescriptor(card), isFaceUp);
        /// <summary>
        /// 设置卡片UI
        /// </summary>
        /// <param name="ui"></param>
        /// <param name="desc"></param>
        /// <param name="isFaceup"></param>
        public async void setCardDesc(UICard ui, CardDescriptor desc, bool isFaceup = true)
        {
            var cards = getManager<CardManager>();
            await ui.setCard(desc, cards, isFaceup);
            setCardDescriptor(desc);
        }
        public async void setCardDefine(UICard ui, CardDefine define, bool isFaceUp = true)
        {
            var cards = getManager<CardManager>();
            await ui.setCardDefine(define, cards, isFaceUp);
        }
        /// <summary>
        /// 取消攻击
        /// </summary>
        public void cancelAttack()
        {
            //缩小动画
            //rectTransform.localScale = Vector3.one;
            ui.AttackArrowImage.hide();
            removeHighlights();
        }
        /// <summary>
        /// 显示文本提示
        /// </summary>
        /// <param name="tip">翻译的**源文本**</param>
        public void showTip(string tip)
        {
            ui.TipText.gameObject.SetActive(true);
            ui.TipText.text = lang._(tip);
            _tipTimer.start();
        }
        public async void displayLargeCard(bool isRight, CardDescriptor desc)
        {
            var pos = ui.CardDisplayerObj.transform.localPosition;
            pos.x = isRight ? 250 : -250;
            ui.CardDisplayerObj.transform.localPosition = pos;

            setCardDesc(ui.LargeCard, desc, true);

            var buffViewDatas = await getCardBuffViewDatas(desc.getBuffs());
            ui.showCardDisplayer(true);
            ui.LargeCard.display();
            ui.setCardDisplayerBuffs(buffViewDatas);
        }
        public void hideLargeCard()
        {
            ui.LargeCard.hide();
            ui.showCardDisplayer(false);
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
                    master.onSelectableTrue.invokeBeforeAnim();
                    master.onSelectableTrue.invokeAfterAnim();
                }
                else if (tryGetServant(target, out var servant))
                {
                    servant.onSelectableTrue.invokeBeforeAnim();
                    servant.onSelectableTrue.invokeAfterAnim();
                }
            }
        }
        /// <summary>
        /// 关闭所有目标的高亮（可选择）
        /// </summary>
        private void removeHighlights()
        {
            ui.SelfMaster.onSelectableFalse.invokeBeforeAnim();
            ui.SelfMaster.onSelectableFalse.invokeAfterAnim();
            ui.EnemyMaster.onSelectableFalse.invokeBeforeAnim();
            ui.EnemyMaster.onSelectableFalse.invokeAfterAnim();
            foreach (var servant in ui.SelfFieldList)
            {
                servant.onSelectableFalse.invokeBeforeAnim();
                servant.onSelectableFalse.invokeAfterAnim();
            }
            foreach (var servant in ui.EnemyFieldList)
            {
                servant.onSelectableFalse.invokeBeforeAnim();
                servant.onSelectableFalse.invokeAfterAnim();
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
                if (pointer.pointerCurrentRaycast.gameObject.GetComponentInParent<ServantItem>() is ServantItem targetServant)
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


        public Projectile createProjectile(Projectile prefab, RectTransform from, RectTransform to)
        {
            Projectile projectile = Instantiate(prefab, from.position, Quaternion.LookRotation(to.position - from.position, Vector3.forward), ui.FieldsImage.transform);
            projectile.transform.SetSiblingIndex(to.getBranch(ui.FieldsImage.transform).GetSiblingIndex() + 1);
            _projectileList.Add(projectile);
            return projectile;
        }
        #endregion
        #region 私有方法

        #region 事件回调
        void onTurnEndButtonClick()
        {
            resetUse(true, true);
            player.cmdTurnEnd(game);
        }
        void onClickNoWhere(Table table, PointerEventData pointer)
        {
            tryCancelUse();
        }
        #region 道具
        private void onItemPointerExit(Item item, PointerEventData pointer)
        {
            _showCardTimer.reset();
            _hoveredItem = null;
            hideLargeCard();
        }

        private void onItemPointerEnter(Item item, PointerEventData pointer)
        {
            _showCardTimer.start();
            _hoveredItem = item;
            _pointer = pointer;
        }
        #endregion 道具

        #region 手牌
        /// <summary>
        /// 手牌拖拽事件
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="pointer"></param>
        void onDragHand(HandListItem hand, PointerEventData pointer)
        {
            if (!hand.isDragable)
                return;
            if (!canControl)//无法进行控制
            {
                resetUse(true, true);
                return;
            }
            Card card = getCard(hand);
            if (usingCard != null && usingCard != card)
            {
                resetUse(true, true);
                return;
            }
            usingCard = card;
            //拖拽卡片
            hand.Card.rectTransform.position = pointer.position;
            if (ui.SelfHandList.rectTransform.rect.Contains(ui.SelfHandList.rectTransform.InverseTransformPoint(pointer.position)))
            {
                //如果移动回手牌区域，恢复正常大小
                hand.Card.rectTransform.localScale = Vector3.one;
                //移除随从占位
                if (usingCard.isServant())
                    hideServantPlaceHolder();
            }
            else
            {
                //移动到手牌区以外的地方视作打算使用
                if (!usingCard.isUsable(game, player, out string info))
                {
                    //卡牌不可用，停止拖拽并提示
                    showTip(info);
                    resetUse(true, true);
                }
                else
                {
                    //手牌在战场上大小和随从牌一致
                    hand.Card.rectTransform.localScale = Vector3.one * .4f / ui.SelfHandList.rectTransform.localScale.y;
                    if (usingCard.define.isServant())
                    {
                        //如果手牌是随从，那么在场上的时候会有一个占位符，预览这个随从放下去的位置。
                        ui.SelfFieldList.defaultItem.rectTransform.SetAsFirstSibling();
                        ui.ServantPlaceHolder.rectTransform.sizeDelta = ui.SelfFieldList.defaultItem.rectTransform.sizeDelta;
                        ui.SelfFieldList.addChild(ui.ServantPlaceHolder.rectTransform);
                        ui.ServantPlaceHolder.display();
                        ui.ServantPlaceHolder.rectTransform.SetSiblingIndex(calcIndexInField(pointer.position) + 1);
                    }
                    else if (usingCard.define.isSpell())
                    {
                        if (usingCard.isNeedTarget(game))
                        {
                            if (!isSelectingTarget)
                            {
                                isSelectingTarget = true;//TODO:这个地方一定要重写，怎么能这样的？isSelectingTarget就应该被usingCard取代
                                highlightTargets(usingCard.getAvaliableTargets(game));//高亮所有可选目标
                                ui.SelfHandList.shrink();//手牌缩小
                            }
                            hand.Card.hide();//将那张牌暂时隐藏
                            if (tryGetMaster(usingCard.getOwner().getMaster(), out var castMaster))
                                //拖出箭头
                                displayArrow(castMaster.rectTransform, castMaster.rectTransform.position, pointer.position);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 手牌拖拽结束
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="pointer"></param>
        void onDragHandEnd(HandListItem hand, PointerEventData pointer)
        {
            if (hand.GetComponentInParent<HandList>() != ui.SelfHandList)//你不能用别人的卡
                return;
            if (!canControl)//不是你的回合，不生效
            {
                resetUse(true, true);
                return;
            }
            usingCard = getCard(hand);
            if (ui.SelfHandList.rectTransform.rect.Contains(ui.SelfHandList.rectTransform.InverseTransformPoint(pointer.position)))
            {
                //如果松开，取消使用
                resetUse(true, true);
            }
            else
            {
                if (!usingCard.isUsable(game, player, out string info))
                {
                    //卡牌不可用
                    showTip(info);
                    resetUse(true, true);
                }
                else if (usingCard.isServant())
                {
                    //松开鼠标，确认使用随从牌
                    int index = calcIndexInField(pointer.position);
                    ui.ServantPlaceHolder.rectTransform.SetSiblingIndex(index + 1);
                    if (usingCard.getAvaliableTargets(game) is Card[] targets && targets.Length > 0)
                    {
                        usingPosition = index;
                        //进入选择目标状态，固定手牌到占位上，高亮可以选择的目标
                        _handToFieldAnim = new HandToFieldAnim(this, hand, ui.SelfFieldList, index);
                        isSelectingTarget = true;
                        highlightTargets(targets);
                        ui.SelfSkill.isUsable = false;
                        ui.SelfHandList.shrink();
                    }
                    else
                    {
                        //使用无目标随从牌
                        player.cmdUse(game, usingCard, index);
                        resetUse(false, false);
                    }
                }
                else if (usingCard.isSpell())
                {
                    if (usingCard.isNeedTarget(game))
                    {
                        //获取鼠标停留的那张卡，就是目标
                        if (isOnTarget(pointer, out var target))
                        {
                            if (usingCard.isValidTarget(game, target))
                            {
                                //合法目标，生效
                                player.cmdUse(game, usingCard, 0, target);
                                resetUse(true, false);
                            }
                            else
                            {
                                //非法目标，跳出非法信息
                                showTip(info);
                                resetUse(true, true);
                                hand.Card.display();
                            }
                        }
                        else
                        {
                            cancelAttack();
                            resetUse(true, true);
                            hand.Card.display();
                        }

                    }
                    else
                    {
                        //使用无目标随从牌
                        player.cmdUse(game, usingCard, 0);
                        resetUse(false, false);
                    }
                }
                else if (usingCard.isItem())
                {
                    if (usingCard.isNeedTarget(game))
                    {
                        usingPosition = 0;
                        //进入选择目标状态，固定手牌到占位上，高亮可以选择的目标
                        isSelectingTarget = true;
                        highlightTargets(usingCard.getAvaliableTargets(game));
                        ui.SelfSkill.isUsable = false;
                        ui.SelfHandList.shrink();
                    }
                    else
                    {
                        //使用无目标随从牌
                        player.cmdUse(game, usingCard, 0);
                        resetUse(false, false);
                    }
                }
            }
            //removeHighlights();
        }

        /// <summary>
        /// 手牌悬停事件
        /// </summary>
        /// <param name="handList"></param>
        /// <param name="pointer"></param>
        void onHandPointerEnter(HandListItem handList, PointerEventData pointer)
        {
            handList.onHoverCard.invokeBeforeAnim();
            handList.onHoverCard.invokeAfterAnim();
            if (!ui.SelfHandList.isExpanded &&
                player.getHand().count > 0)
            {
                ui.TipBox.display(lang._(TAB_CARD_NUMBER, getCard(handList).getOwner().getHand().count), pointer.position + _tipBoxOffset);
            }
        }
        void onHandPointerMove(HandListItem handList, Vector2 pointer)
        {
            ui.TipBox.updatePos(pointer);
        }
        void onHandPointerExit(HandListItem handList, PointerEventData pointer)
        {
            handList.onexitHoverCard.invokeBeforeAnim();
            handList.onexitHoverCard.invokeAfterAnim();
            ui.TipBox.hide();
        }
        #endregion

        #region 英雄
        /// <summary>
        /// 点击Master卡处理
        /// </summary>
        /// <param name="master"></param>
        /// <param name="pointer"></param>
        void onClickMaster(Master master, PointerEventData pointer)
        {
            // （目标选择模式）
            if (isSelectingTarget)
            {
                if (usingCard.isValidTarget(game, getCard(master)))
                {
                    player.cmdUse(game, usingCard, usingPosition, getCard(master));
                    resetUse(false, false);
                }
                else
                {
                    showTip(TAB_THIS_INVALIDATION);
                    resetUse(true, true);
                }
            }
        }
        /// <summary>
        /// 英雄拖拽处理
        /// </summary>
        /// <param name="master"></param>
        /// <param name="pointer"></param>
        void onDragMaster(Master master, PointerEventData pointer)
        {
            if (!canControl)//不是你的回合
                return;
            TouhouCardEngine.Card card = getCard(master);
            if (card.getOwner() != player)//不是你的英雄
                return;
            if (!card.canAttack(game))
                return;
            //拉动距离也应该有一个阈值
            if (Vector2.Distance(master.rectTransform.position, pointer.position) > master.attackThreshold)
            {
                //播放一个变大的动画？
                //servant.rectTransform.localScale = Vector3.one * 1.1f;
                //显示指针
                displayArrow(master.rectTransform, master.rectTransform.position, pointer.position);
                //高亮标记所有目标
                highlightTargets(game.findAllCardsInField(c => card.isAttackable(game, player, c, out _)));
            }
            else
            {
                cancelAttack();
            }
        }
        /// <summary>
        /// 英雄拖拽结束处理
        /// </summary>
        /// <param name="master"></param>
        /// <param name="pointer"></param>
        void onDragEndMaster(Master master, PointerEventData pointer)
        {
            if (!canControl)//不是你的回合
                return;
            Card card = getCard(master);
            if (card.owner != player)//不是你的英雄
                return;
            if (!card.canAttack(game))//不能攻击
                return;
            //如果在随从上面
            if (isOnTarget(pointer, out var target))
            {
                if (card.isAttackable(game, player, target, out var tip))
                    player.cmdAttack(game, card, target);
                else
                    showTip(tip);
            }
            //取消选中和攻击
            cancelAttack();
        }
        #endregion

        #region 技能
        /// <summary>
        /// 技能点击处理
        /// </summary>
        void onClickSkill()
        {
            if (!canControl)
                return;
            if (tryCancelUse())
                return;
            if (usingCard != null)//已经在用别的牌了，不能点技能
                return;
            if (player.getSkill().isUsable(game, player, out var info))
            {
                if (!player.getSkill().isNeedTarget(game))
                    player.cmdUse(game, player.getSkill());
            }
            else
            {
                showTip(info);
            }
        }
        /// <summary>
        /// 技能拖拽
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="pointer"></param>
        void onDragSkill(Skill skill, PointerEventData pointer)
        {
            //TODO:按帧记录Replay
            if (!canControl)
                return;
            // Stage2: 当前正在拖拽，判断距离以决定是否退出目标选择模式
            if (usingCard == player.getSkill())
            {
                if (Vector3.Distance(skill.rectTransform.position, pointer.position) < skillDragThreshold)
                    cancelSkill();
                else
                    displayArrow(skill.rectTransform, skill.rectTransform.position, pointer.position);
                return;
            }
            // Stage1: 当前没有拖拽，进入目标选择模式
            if (usingCard != null)
                return;//已经在用别的牌了
            if (Vector3.Distance(skill.rectTransform.position, pointer.position) < skillDragThreshold)
                return;
            if (player.getSkill().isUsable(game, player, out var info))
            {
                if (player.getSkill().isNeedTarget(game))
                {
                    usingCard = player.getSkill();
                    highlightTargets(player.getSkill().getAvaliableTargets(game));
                    displayArrow(skill.rectTransform, skill.rectTransform.position, pointer.position);
                }
            }
            else
                showTip(info);
        }
        /// <summary>
        /// 技能结束拖拽
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="pointer"></param>
        void onDragSkillEnd(Skill skill, PointerEventData pointer)
        {
            if (!canControl)
            {
                cancelSkill();
                return;
            }
            if (usingCard != null && usingCard != player.getSkill())
                return;//在用其他卡片
            if (usingCard != player.getSkill())
            {
                cancelSkill();
                return;
            }
            if (isOnTarget(pointer, out var target))
            {
                if (player.getSkill().isUsable(game, player, out var info) && player.getSkill().isValidTarget(game, target))
                    player.cmdUse(game, player.getSkill(), 0, target);
                else
                    showTip(info);
            }
            cancelSkill();
        }
        private void onSkillPointerExit(Skill arg1, PointerEventData arg2)
        {
            ui.TipBox.hide();
        }

        private void onSelfSkillPointerEnter(Skill skill, PointerEventData pointer)
        {
            ui.TipBox.display(getSkin(player.getSkill()).desc, pointer.position + _tipBoxOffset);
        }

        private void onEnemySkillPointerEnter(Skill skill, PointerEventData pointer)
        {
            ui.TipBox.display(getSkin(game.getOpponent(player).getSkill()).desc, pointer.position + _tipBoxOffset);
        }

        private void onSkillPointerMove(Skill arg1, Vector2 pointer)
        {
            ui.TipBox.updatePos(pointer);
        }
        #endregion

        #region 仆从
        /// <summary>
        /// 点击随从卡
        /// </summary>
        /// <param name="servant"></param>
        /// <param name="pointer"></param>
        void onClickServant(ServantItem servant, PointerEventData pointer)
        {
            // （目标选择模式）
            if (isSelectingTarget)
            {
                if (usingCard.isValidTarget(game, getCard(servant)))
                {
                    player.cmdUse(game, usingCard, usingPosition, getCard(servant));
                    resetUse(false, false);
                }
                else
                {
                    showTip(TAB_THIS_INVALIDATION);
                    resetUse(true, true);
                }
            }
        }

        void onEnterServant(ServantItem servant, PointerEventData pointer)
        {
            _showCardTimer.start();
            _servant = servant;
            _pointer = pointer;
            //displayLargeCard(pointer.position.x < Screen.width / 2, getCard(servant));
        }
        void onExitServant(ServantItem servant, PointerEventData pointer)
        {
            _showCardTimer.reset();
            _servant = null;
            hideLargeCard();
        }
        /// <summary>
        /// 随从拖拽处理
        /// </summary>
        /// <param name="servant"></param>
        /// <param name="pointer"></param>
        void onDragServant(ServantItem servant, PointerEventData pointer)
        {
            if (!canControl)//不是你的回合
                return;
            Card card = getCard(servant);
            if (card.getOwner() != player)//不是你的卡
                return;
            if (!card.canAttack(game))
                return;
            //拉动距离也应该有一个阈值
            if (Vector2.Distance(servant.rectTransform.position, pointer.position) > servant.attackThreshold)
            {
                //播放一个变大的动画？
                //servant.rectTransform.localScale = Vector3.one * 1.1f;
                //显示指针
                displayArrow(ui.SelfFieldList.rectTransform, servant.rectTransform.position, pointer.position);
                //高亮标记所有目标
                highlightTargets(game.findAllCardsInField(c => card.isAttackable(game, player, c, out _)));
            }
            else
            {
                cancelAttack();
            }
        }
        /// <summary>
        /// 随从拖拽结束处理
        /// </summary>
        /// <param name="servant"></param>
        /// <param name="pointer"></param>
        void onDragEndServant(ServantItem servant, PointerEventData pointer)
        {
            if (!canControl)//不是你的回合
                return;
            Card card = getCard(servant);
            if (card.owner != player)//不是你的卡
                return;
            if (!card.canAttack(game))//不能攻击
                return;
            //如果在随从或英雄上面
            if (isOnTarget(pointer, out var target))
            {
                if (card.isAttackable(game, player, target, out var tip))
                    player.cmdAttack(game, card, target);
                else
                    showTip(tip);
            }
            //取消选中和攻击
            cancelAttack();
        }
        #endregion

        #region 牌库

        void onDeckClick()
        {
            ui.PilePanel.display(lang._(TAB_YOUR_BOX));
            ui.PilePanel.VerticalOverflowCardList.clearItems();
            foreach (var card in player.getDeck().OrderBy(c => c.define.getCost()))
            {
                var item = ui.PilePanel.VerticalOverflowCardList.addItem();
                setCard(item.Card, newSnapshot(card), true);
            }
        }
        void onDeckPointerEnter(UI.Deck deck, PointerEventData pointer)
        {
            bool self = deck == ui.SelfDeck;
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
            ui.TipBox.display(tip, pointer.position + _tipBoxOffset);
        }
        void onDeckPointerExit(UI.Deck deck, PointerEventData pointer)
        {
            ui.TipBox.hide();
        }
        private void onDeckPointerMove(UI.Deck arg1, Vector2 arg2)
        {
            ui.TipBox.updatePos(arg2);
        }
        #endregion

        #region 墓地

        void onGraveClick()
        {
            ui.PilePanel.display(lang._(TAB_YOUR_GRAVEYARD));
            ui.PilePanel.VerticalOverflowCardList.clearItems();
            foreach (var card in player.getGrave())
            {
                var item = ui.PilePanel.VerticalOverflowCardList.addItem();
                setCard(item.Card, newSnapshot(card), true);
            }
        }
        void onGravePointerEnter(UI.Deck deck, PointerEventData pointer)
        {
            var curGrave = (deck == ui.SelfGraveDeck ? player : game.getOpponent(player)).getGrave();
            string tip = curGrave.count > 0 ?
                lang._(TAB_GRAVEYARD_HAS, curGrave.count) :
                lang._(TAB_GRAVEYARD_NULL);
            ui.TipBox.display(tip, pointer.position + _tipBoxOffset);
        }
        void onGravePointerExit(UI.Deck deck, PointerEventData pointer)
        {
            ui.TipBox.hide();
        }
        #endregion

        #region 发现
        private void onDiscoverHideButtonClick()
        {
            if (ui.Discover.PanelImage.gameObject.activeSelf)
            {
                ui.Discover.ButtonButtonBlack.asButton.setText(lang._(TAB_SHOW));
                ui.Discover.PanelImage.hide();
            }
            else
            {
                ui.Discover.ButtonButtonBlack.asButton.setText(lang._(TAB_HIDE));
                ui.Discover.PanelImage.display();
                LayoutRebuilder.ForceRebuildLayoutImmediate(ui.Discover.Text.rectTransform);
                DiscoverRequest request = game.answers.getRequest<DiscoverRequest>(player.id);
                if (request.cardRefArray != null)
                    displayDiscoverDialog(request.cardRefArray, ui.Discover.Text.text);
                else
                    displayDiscoverDialog(request.cardIdArray, ui.Discover.Text.text);
            }
        }
        #endregion

        #endregion
        /// <summary>
        /// 取消正在执行的技能UI
        /// </summary>
        private void cancelSkill()
        {
            usingCard = null;
            removeHighlights();
            hideArrow();
        }
        private bool tryCancelUse()
        {
            if (usingCard != null && usingCard.define.isServant() && isSelectingTarget)
            {
                resetUse(true, true);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 显示箭头
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        void displayArrow(RectTransform source, Vector2 from, Vector2 to)
        {
            ui.AttackArrowImage.display();
            ui.AttackArrowImage.rectTransform.SetSiblingIndex(source.GetSiblingIndex() - 1);
            ui.AttackArrowImage.rectTransform.position = from;
            ui.AttackArrowImage.rectTransform.up = (to - from).normalized;
            ui.AttackArrowImage.rectTransform.setHeight(Vector3.Distance(from, to) / ui.getCanvas().transform.localScale.y);
        }
        /// <summary>
        /// 隐藏箭头
        /// </summary>
        void hideArrow()
        {
            ui.AttackArrowImage.hide();
        }
        /// <summary>
        /// 计算随从空位位置
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private int calcIndexInField(Vector2 position)
        {
            int index = 0;
            var servants = ui.SelfFieldList.getItems();
            if (servants.Length > 0)
            {
                //需要选择空位，计算空位
                for (int i = 0; i < servants.Length; i++)
                {
                    if (servants[i].rectTransform.position.x < position.x)
                        index = i + 1;
                }
            }
            return index;
        }
        /// <summary>
        /// 重置使用卡牌状态
        /// </summary>
        /// <param name="resetItem">是否重置手牌</param>
        /// <param name="resetPlaceHolder">是否重置随从占位</param>
        private void resetUse(bool resetItem, bool resetPlaceHolder)
        {
            if (usingCard != null && resetItem)
            {
                HandListItem usingHand = getHand(usingCard);
                usingHand.Card.display();
                var desc = getCardDescriptor(usingCard);
                setCardDesc(usingHand.Card, desc, true);
                usingHand.Card.rectTransform.localScale = Vector3.one;
                usingHand.Card.rectTransform.localPosition = Vector2.zero;
            }
            _handToFieldAnim = null;
            usingCard = null;
            if (resetPlaceHolder)
                hideServantPlaceHolder();
            hideArrow();
            isSelectingTarget = false;
            removeHighlights();
        }
        private async Task<CardBuffListItemViewData[]> getCardBuffViewDatas(IEnumerable<Buff> buffs)
        {
            // 增益列表。
            var cards = getManager<CardManager>();

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
            foreach (var player in game.players)
            {
                updateCardControlable(player.getMaster());
                updateCardControlable(player.getSkill());
                foreach (Card servant in player.getField())
                {
                    updateCardControlable(servant);
                }
            }
        }

        private void updateCardControlable(Card card)
        {
            if (card == null)
                return;
            var owner = card.getOwner();
            bool highlight = owner == player && game.getCurrentPlayer() == player && canControl && !isSelectingTarget;

            if (card.isMaster())
            {
                if (tryGetMaster(card.id, out Master master))
                {
                    var anim = highlight && card.canAttack(game) ? master.onCanAttackTrue : master.onCanAttackFalse;
                    anim.invokeBeforeAnim();
                    anim.invokeAfterAnim();
                }
            }
            else if (card.isSkill())
            {
                if (tryGetSkill(card.id, out Skill skill))
                {
                    bool skillUsable = highlight && //是自己的回合
                        card.isUsable(game, player, out _);//技能是可用的

                    skill.isUsable = skillUsable;
                    skill.asButton.interactable = skillUsable;
                }
            }
            else if (card.isServant())
            {
                if (tryGetServant(card.id, out ServantItem servant))
                {
                    var servantAnim = highlight && card.canAttack(game) ? servant.onCanAttackTrue : servant.onCanAttackFalse;
                    servantAnim.invokeBeforeAnim();
                    servantAnim.invokeAfterAnim();
                }
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



        private Dictionary<string, string> _TranslateTagDic;
        public Dictionary<string, string> TranslateTagDic
        {
            get
            {
                if (_TranslateTagDic == null)
                {
                    _TranslateTagDic = new Dictionary<string, string>();
                    var T = typeof(TranslateTags);
                    FieldInfo[] fields = T.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance);
                    for (int i = 0; i < fields.Length; i++)
                    {
                        _TranslateTagDic.Add(fields[i].Name, fields[i].GetValue(null).ToString());
                        //Debug.Log("这里是字典结果" + fields[i].Name + fields[i].GetValue(null).ToString());
                    }
                }
                return _TranslateTagDic;
            }
            set { _TranslateTagDic = value; }
        }
        Dictionary<int, ServantItem> cardServantDic { get; } = new Dictionary<int, ServantItem>();
        /// <summary>
        /// 当前手牌列表
        /// </summary>
        Dictionary<int, HandListItem> cardHandDic { get; } = new Dictionary<int, HandListItem>();
        Dictionary<int, Secret> cardSecretDic { get; } = new Dictionary<int, Secret>();
        Dictionary<int, InitReplaceCardListItem> initDic { get; } = new Dictionary<int, InitReplaceCardListItem>();
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
        ServantItem _servant;
        PointerEventData _pointer;
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
}
