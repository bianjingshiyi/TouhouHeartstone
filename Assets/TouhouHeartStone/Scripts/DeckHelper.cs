using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouhouHeartstone;

namespace Game
{
    public static class DeckHelper
    {
        /// <summary>
        /// 获取一个套牌内的卡牌的所有卡池（包括卡池引用的其他卡池）。
        /// </summary>
        /// <param name="deck">套牌。</param>
        /// <param name="cardManager">卡牌管理器对象。</param>
        /// <returns>卡池ID列表。</returns>
        public static long[] getFullCardPools(this Deck deck, CardManager cardManager)
        {
            long[] poolsId = deck.getCardPools();

            // 将卡池中对其他卡池的引用加入到列表中。
            HashSet<long> targetPools = new HashSet<long>();
            foreach (long poolId in poolsId)
            {
                addCardPoolRefs(poolId);
            }

            void addCardPoolRefs(long poolId)
            {
                if (targetPools.Contains(poolId))
                    return;

                targetPools.Add(poolId);
                if (cardManager.tryGetCardPool(poolId, out CardPool pool))
                {
                    foreach (var poolRef in pool.otherPoolReferences)
                    {
                        addCardPoolRefs(poolRef);
                    }
                }
            }
            return targetPools.ToArray();
        }
    }
}
