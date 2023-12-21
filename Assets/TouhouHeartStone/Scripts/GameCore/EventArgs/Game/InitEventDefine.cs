using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouhouCardEngine;
using TouhouCardEngine.Interfaces;

namespace TouhouHeartstone
{
    public class InitEventDefine : EventDefine
    {
        [Obsolete]
        public override void Record(CardEngine game, EventArg arg, EventRecord record)
        {
        }

        public override async Task execute(IEventArg arg)
        {
            var game = arg.game;
            var gamePlayers = game.players;
            //决定玩家行动顺序
            var optionPlayers = game.getOption().sortedPlayers;
            if (optionPlayers == null || optionPlayers.Length != gamePlayers.Length)
            {
                if (optionPlayers != null && optionPlayers.Length != gamePlayers.Length)
                    game.logger?.logWarn("游戏参数玩家行动顺序长度与实际数量不匹配");
                List<Player> remainedList = new List<Player>(gamePlayers);
                Player[] sortedPlayers = new Player[remainedList.Count];
                for (int i = 0; i < sortedPlayers.Length; i++)
                {
                    int index = game.randomInt(0, remainedList.Count - 1);
                    sortedPlayers[i] = remainedList[index];
                    remainedList.RemoveAt(index);
                }
                game.setProp(GAME.SORTEDPLAYER, sortedPlayers);
            }
            else
            {
                game.setProp(GAME.SORTEDPLAYER, optionPlayers.Select(id => gamePlayers.FirstOrDefault(p => p.id == id)).ToArray());
            }

            //创建主人公和技能卡
            var sortPlayers = game.getSortedPlayers();
            Card[] masterCards = sortPlayers.Select(p => { return p.getMaster(); }).ToArray();
            foreach (Card card in masterCards)
            {
                await card.setCurrentLife(game, card.getLife(game));
            }

            //洗牌，然后抽初始卡牌
            for (int i = 0; i < sortPlayers.Length; i++)
            {
                Player player = sortPlayers[i];
                Pile deck = player.getDeck();
                if (deck.count < 1)
                    continue;
                if (game.getOption().shuffle)
                    deck.shuffle(game);
                int count = i == 0 ? 3 : 4;
                var initCards = deck.Where(c => c.hasKeyword(game, KEYWORD.MISSION))
                    .randomTake(game, count).ToList();
                initCards.AddRange(deck.Except(initCards)
                    .Reverse()
                    .Take(count - initCards.Count()));
                initCards.Reverse();
                await deck.moveTo(game, initCards, player.getInit());
                await game.updateMasterItemAttack(player);
            }


            var orderLog = string.Join("、", sortPlayers.Select(p => p.ToString()));
            var masterLog = string.Join("，", sortPlayers.Select(p => $"{p.getMaster()}({p.getSkill()})"));
            var cardsLog = string.Join("；", sortPlayers.Select(p => string.Join("、", p.getInit().Select(c => c.ToString()))));
            game.logger.logTrace($"游戏初始化，玩家行动顺序：{orderLog}，玩家英雄：{masterLog}，初始卡牌：{cardsLog}");
        }

        public override string toString(EventArg arg)
        {
            return "初始化";
        }
        public static Task<EventArg> doEvent(CardEngine game)
        {
            var define = game.getEventDefine<InitEventDefine>();
            var arg = new EventArg(game, define);
            return game.triggers.doEvent(arg);
        }
    }
}
