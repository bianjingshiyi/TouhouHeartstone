using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BJSYGameCore;
using Game;
using Game.BattleTable;
using MukioI18n;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;
using TouhouHeartstone;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Card = TouhouCardEngine.Card;
using UICard = UI.Card;

namespace Controllers
{
    public class GameLogCtrl : MonoBehaviour
    {
        public void setGame(CardEngine game, MainManager main, BattleTableManager battleTableManager, Player localPlayer)
        {
            _game = game;
            _main = main;
            _battleTableManager = battleTableManager;
            _localPlayer = localPlayer;
            game.triggers.onEventBefore += t1 => gameTriggerEventCache.cache(onEventBefore, t1);
            game.triggers.onEventAfter += t1 => gameTriggerEventCache.cache(onEventAfter, t1);
        }
        public void reset()
        {
            logItems.Clear();
            _logPanel.shrink();
            _logScrollView.LogList.updateLogListCount(0);
        }
        public void pollEvents()
        {
            gameTriggerEventCache.poll();
        }
        #region 生命周期
        private void Awake()
        {
            _logScrollView.LogList.onEnableLogListItem += onEnableLogListItem;
            _logScrollView.LogList.onDisableLogListItem += onDisableLogListItem;
            _logScrollView.LogList.onUpdateLogListItem += onUpdateLogListItem;
        }
        #endregion
        #region 事件回调
        void onEventBefore(IEventArg obj)
        {
            switch (obj)
            {
                case HearthStoneRule.UseEventArg _:
                //case THHPlayer.SetGemEventArg _:
                //case THHPlayer.SetMaxGemEventArg _:
                case HearthStoneRule.BurnEventArg _:
                case HearthStoneRule.FatigueEventArg _:
                case THHCard.DiscardEventArg _:
                case HearthStoneRule.EnterFieldEventArg _:
                case HearthStoneRule.RebornEventArg _:
                case THHCard.AttackEventArg _:
                case THHCard.DamageEventArg _:
                case THHCard.SetArmorEventArg _:
                case THHCard.HealEventArg _:
                case THHCard.SilenceEventArg _:
                case THHCard.ShackleEventArg _:
                case THHCard.DeathEventArg _:
                case THHPlayer.DiscoverEventArg _:
                case THHPlayer.RevealDuelEventArg _:
                case THHCard.SecretTriggerEventArg _:
                    addLogItem(obj);
                    break;
                case HearthStoneRule.DrawEventArg _:
                    if (obj is IDescribableEventArg describable)
                    {
                        // string info = describable.toString(_game, localPlayer);
                        string info = lang._(
                            describable.localizeTemplateString(_game, _localPlayer),
                            describable.localizeStringArgs(_game, _localPlayer)
                        );
                        if (info != null)
                        {
                            addLogItem(obj);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        void onEventAfter(IEventArg obj)
        {
            var item = logItems.FirstOrDefault(i => i.arg == obj);
            if (item == null)
                return;
            foreach (var parentItem in logItems)
            {
                int depth = 0;
                if (isChildOf(parentItem.arg, item.arg, ref depth))
                {
                    item.level = depth;
                    break;
                }
            }
            bool isChildOf(IEventArg parent, IEventArg child, ref int depth)
            {
                if (depth > 100)
                    throw new StackOverflowException();
                if (child == null)
                    return false;
                if (parent == null)
                    return false;
                if (child.parent == null)
                    return false;
                if (child.parent == parent)
                {
                    depth++;
                    return true;
                }
                else
                {
                    depth++;
                    return isChildOf(parent, child.parent, ref depth);
                }
            }
            _logScrollView.asScrollRect.verticalNormalizedPosition = 0;
        }
        private void onEnableLogListItem(int index, RectTransform rect)
        {
            var item = rect.GetComponent<LogListItem>();
            item.onPointerEnter.add(onLogListItemEnter);
            item.onPointerExit.add(onLogListItemExit);
        }
        private void onDisableLogListItem(int index, RectTransform rect)
        {
            var item = rect.GetComponent<LogListItem>();
            item.onPointerEnter.remove(onLogListItemEnter);
            item.onPointerExit.remove(onLogListItemExit);
        }
        private async void onUpdateLogListItem(int index, RectTransform rect)
        {
            var item = rect.GetComponent<LogListItem>();
            var logItem = logItems[index];
            var obj = logItem.arg;
            item.SetLevel(logItem.level);

            Sprite icon = item.defaultIcon;
            string text = string.Empty;
            Card imageCard = null;
            CardDefine imageCardDefine = null;
            Player player = null;
            switch (obj)
            {
                case HearthStoneRule.DrawEventArg draw:
                    icon = item.drawIcon;
                    text = lang._(GAME_MANAGER_DRAW);
                    imageCard = draw.player == _localPlayer ? draw.card : draw.player.getMaster();
                    player = draw.player;
                    break;
                case HearthStoneRule.BurnEventArg burn:
                    icon = item.burnIcon;
                    text = lang._(GAME_MANAGER_BURN);
                    imageCard = burn.card;
                    player = burn.player;
                    break;
                case HearthStoneRule.FatigueEventArg fatigue:
                    icon = item.fatigueIcon;
                    text = lang._(GAME_MANAGER_FATIGUE);
                    imageCard = fatigue.player.getMaster();
                    player = fatigue.player;
                    break;
                case THHCard.DiscardEventArg discard:
                    icon = item.discardIcon;
                    text = lang._(GAME_MANAGER_DISCARD);
                    imageCard = discard.card;
                    player = discard.player;
                    break;
                case HearthStoneRule.UseEventArg use:
                    icon = item.useIcon;
                    text = lang._(GAME_MANAGER_USE);
                    imageCard = use.card.isTrap(_game) && use.player != _localPlayer ? use.player.getMaster() : use.card;
                    player = use.player;
                    break;
                case HearthStoneRule.EnterFieldEventArg token:
                    icon = item.tokenIcon;
                    text = lang._(GAME_MANAGER_TOKEN);
                    imageCard = token.card;
                    player = token.player;
                    break;
                case HearthStoneRule.RebornEventArg reborn:
                    icon = item.rebornIcon;
                    text = lang._(GAME_MANAGER_REBORN);
                    imageCard = reborn.card;
                    player = reborn.player;
                    break;
                case THHCard.AttackEventArg attack:
                    icon = item.attackIcon;
                    text = lang._(GAME_MANAGER_ATTACK);
                    imageCard = attack.card;
                    player = attack.card.getOwner();
                    break;
                case THHCard.DamageEventArg damage:
                    icon = item.damageIcon;
                    text = lang._(GAME_MANAGER_DAMAGE, damage.damagedValue);
                    imageCard = damage.target;
                    player = damage.target.getOwner();
                    break;
                case THHCard.SetArmorEventArg armor:
                    icon = item.armorIcon;
                    text = lang._(GAME_MANAGER_SET_ARMOR, armor.value);
                    imageCard = armor.card;
                    player = armor.card.getOwner();
                    break;
                case THHCard.HealEventArg heal:
                    icon = item.healIcon;
                    text = lang._(GAME_MANAGER_HEAL, heal.healedValue);
                    imageCard = heal.card;
                    player = heal.card.getOwner();
                    break;
                case THHCard.SilenceEventArg silence:
                    icon = item.silenceIcon;
                    text = lang._(GAME_MANAGER_SILENCE);
                    imageCard = silence.card;
                    player = silence.card.getOwner();
                    break;
                case THHCard.SecretTriggerEventArg secret:
                    icon = item.secretIcon;
                    text = lang._(GAME_MANAGER_SECRET);
                    imageCard = secret.card;
                    player = secret.card.getOwner();
                    break;
                case THHCard.DeathEventArg death:
                    icon = item.deathIcon;
                    text = lang._(GAME_MANAGER_DEATH);
                    imageCard = death.card;
                    player = death.player;
                    break;
                case THHPlayer.DiscoverEventArg discover:
                    text = lang._(GAME_MANAGER_DISCOVER);
                    imageCard = discover.player == _localPlayer || discover.isShow ? discover.discovered : discover.player.getMaster();
                    player = discover.player;
                    break;
                case THHPlayer.RevealDuelEventArg revealDuel:
                    text = lang._(GAME_MANAGER_REVEAL_DUEL);
                    imageCard = revealDuel.player.getMaster();
                    player = revealDuel.player;
                    break;
                case THHCard.ShackleEventArg shackle:
                    text = lang._(GAME_MANAGER_SHACKLE);
                    imageCard = shackle.card;
                    player = shackle.card.getOwner();
                    break;
            }

            item.IconImage.sprite = icon;
            item.Text.text = text;
            item.BorderImage.sprite = player == _localPlayer ? item.selfsprite : item.opposprite;
            Sprite image = null;
            if (imageCard != null)
            {
                image = await getCardSprite(imageCard);
            }
            else if (imageCardDefine != null)
            {
                image = await getCardDefineSprite(imageCardDefine);
            }
            item.Image.sprite = image;
        }
        private async void onLogListItemEnter(LogListItem item)
        {
            int itemIndex = _logScrollView.LogList.getLogItemIndex(item.rectTransform);
            var logItem = logItems[itemIndex];
            var arg = logItem.arg;
            if (arg is IDescribableEventArg describable)
            {
                var logInfoHori = _logScrollView.LogInfoHorizantal;
                logInfoHori.display();
                string info = lang._(describable.localizeTemplateString(_game, _localPlayer), describable.localizeStringArgs(_game, _localPlayer));
                info = Regex.Replace(info, @"{card:(?<poolId>-?\d+),(?<id>-?\d+)}",
                    m =>
                    {
                        if (int.TryParse(m.Groups["id"].Value, out var id) && long.TryParse(m.Groups["poolId"].Value, out var poolId))
                        {
                            var cardPoolRef = cards.getCardPoolReferenceInSet(poolId, _game?.getOption()?.cardPools);
                            return cards.getDefaultSkin(cardPoolRef, id).name;
                        }
                        return "未知卡牌";
                    });
                logInfoHori.Text.text = info;

                var eventCard = describable.getCard(_game, _localPlayer);
                if (eventCard != null)
                {
                    logInfoHori.InfoCard.display();
                    await setCardUIByEvent(logInfoHori.InfoCard.Card, eventCard, arg);
                }
                else
                    logInfoHori.InfoCard.hide();

                logInfoHori.TargetGridList.clearItems();
                var targets = describable.getTargets(_game, _localPlayer);
                if (targets != null && targets.Length > 0)
                {
                    logInfoHori.TargetGridList.gameObject.SetActive(true);
                    foreach (var target in targets)
                    {
                        if (target == null)
                            continue;
                        var targetItem = logInfoHori.TargetGridList.addItem();

                        await setCardUIByEvent(targetItem.Card, target, arg);
                    }
                }
                else
                {
                    logInfoHori.TargetGridList.gameObject.SetActive(false);
                }
                LayoutRebuilder.ForceRebuildLayoutImmediate(logInfoHori.TargetGridList.rectTransform);
                LayoutRebuilder.ForceRebuildLayoutImmediate(logInfoHori.LogInfoVerticalVerticalLayoutGroup.GetComponent<RectTransform>());
                LayoutRebuilder.ForceRebuildLayoutImmediate(logInfoHori.rectTransform);
            }
        }
        void onLogListItemExit(LogListItem item)
        {
            _logScrollView.LogInfoHorizantal.hide();
        }
        #endregion

        private void addLogItem(IEventArg arg)
        {
            var item = new LogItem()
            {
                arg = arg
            };
            logItems.Add(item);
            _logScrollView.LogList.updateLogListCount(logItems.Count);
        }
        private async Task setCardUIByEvent(UICard ui, DescribableCard card, IEventArg arg)
        {
            if (ui == null || card == null)
                return;
            if (card.card is Card cardObj)
            {
                EventRecord record = _game.triggers.getEventRecord(arg);
                if (record != null)
                {
                    CardSnapshot cardSnapshot = _game.getCardSnapshotOfEvent(cardObj, record);
                    if (cardSnapshot != null)
                    {
                        var descriptor = new CardDescriptor(_game, cardSnapshot);
                        await ui.setCard(descriptor, _main.card);
                        return;
                    }
                }
                await ui.setCardDefine(cardObj.define, _main.card);
            }
            else if (card.define is CardDefine cardDefine)
            {
                await ui.setCardDefine(cardDefine, _main.card);
            }
        }

        private Task<Sprite> getCardSprite(Card card)
        {
            return getCardDefineSprite(card.define);
        }
        private Task<Sprite> getCardDefineSprite(CardDefine cardDefine)
        {
            return cards.loadCardImage(cardDefine);
        }
        public CardManager cards => _main.card;
        public LangManager lang => _main.lang;
        public BattleTableManager tableManager => _battleTableManager;

        [TranslateMsg]
        private const string GAME_MANAGER_DEATH = "死亡";
        [TranslateMsg]
        private const string GAME_MANAGER_SECRET = "陷阱";
        [TranslateMsg]
        private const string GAME_MANAGER_SILENCE = "封印";
        [TranslateMsg]
        private const string GAME_MANAGER_SHACKLE = "禁锢";
        [TranslateMsg]
        private const string GAME_MANAGER_ATTACK = "攻击";
        [TranslateMsg("对局记录-伤害事件（{0}为失去的生命值）")]
        private const string GAME_MANAGER_DAMAGE = "-{0}";
        [TranslateMsg("对局记录-护甲值更改事件（{0}为当前护甲值）")]
        private const string GAME_MANAGER_SET_ARMOR = "=>{0}";
        [TranslateMsg("对局记录-治疗事件（{0}为获得的生命值）")]
        private const string GAME_MANAGER_HEAL = "+{0}";
        [TranslateMsg]
        private const string GAME_MANAGER_REBORN = "复活";
        [TranslateMsg]
        private const string GAME_MANAGER_TOKEN = "召唤";
        [TranslateMsg]
        private const string GAME_MANAGER_USE = "使用";
        [TranslateMsg]
        private const string GAME_MANAGER_DISCARD = "弃牌";
        [TranslateMsg]
        private const string GAME_MANAGER_FATIGUE = "疲劳";
        [TranslateMsg]
        private const string GAME_MANAGER_BURN = "爆牌";
        [TranslateMsg]
        private const string GAME_MANAGER_DRAW = "抽牌";
        [TranslateMsg]
        private const string GAME_MANAGER_DISCOVER = "发现";
        [TranslateMsg]
        private const string GAME_MANAGER_REVEAL_DUEL = "信仰对决";
        [SerializeField]
        private LogPanel _logPanel;
        [SerializeField]
        private LogScrollView _logScrollView;
        private MainManager _main;
        private BattleTableManager _battleTableManager;
        private CardEngine _game;
        private Player _localPlayer;
        ActionEventCache gameTriggerEventCache { get; } = new ActionEventCache();
        private List<LogItem> logItems = new List<LogItem>();
    }
}
