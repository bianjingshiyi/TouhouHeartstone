using System.Collections.Generic;
using System.Linq;
using Controllers;
using TouhouCardEngine;
using TouhouCardEngine.Shared;

namespace Game
{
    public static class CardPoolHelper
    {
        /// <summary>
        /// 获取目标卡池ID依赖的所有卡池ID。
        /// </summary>
        /// <param name="id">目标卡池ID。</param>
        /// <param name="cardManager">卡牌管理器。</param>
        /// <returns>目标ID卡池依赖的所有卡池ID（不包括目标卡池）。</returns>
        public static long[] GetCardPoolDependencies(this long id, CardManager cardManager)
        {
            HashSet<long> dependencies = new HashSet<long>(); 
            addCardPoolDeps(id);

            void addCardPoolDeps(long poolId)
            {
                var poolInfo = cardManager.getCardPoolInfo(poolId);
                if (poolInfo != null && poolInfo.dependencyRefs != null)
                {
                    foreach (var poolRef in poolInfo.dependencyRefs)
                    {
                        if (dependencies.Add(poolRef.cardPoolId))
                        {
                            addCardPoolDeps(poolRef.cardPoolId);
                        }
                    }
                }
            }
            return dependencies.ToArray();
        }

        /// <summary>
        /// 获取依赖目标卡池ID的所有卡池ID。
        /// </summary>
        /// <param name="id">目标卡池ID。</param>
        /// <param name="cardManager">卡牌管理器。</param>
        /// <param name="cardPoolFullSet">卡池ID全集，从这个范围内寻找依赖卡池。</param>
        /// <returns>依赖于目标ID卡池的所有卡池ID（不包括目标卡池）。</returns>
        public static long[] GetDependedCardPools(this long id, CardManager cardManager, IEnumerable<long> cardPoolFullSet)
        {
            HashSet<long> pools = new HashSet<long>();
            addDependedCardPool(id);

            void addDependedCardPool(long poolId)
            {
                foreach (var checkId in cardPoolFullSet)
                {
                    var poolInfo = cardManager.getCardPoolInfo(checkId);
                    if (poolInfo == null || poolInfo.dependencyRefs == null)
                        continue;
                    if (poolInfo.dependencyRefs.Any(p => p.cardPoolId == poolId))
                    {
                        if (pools.Add(checkId))
                        {
                            addDependedCardPool(checkId);
                        }
                    }
                }
            }

            return pools.ToArray();
        }
        /// <summary>
        /// 获取目标卡池依赖的所有卡池。
        /// </summary>
        /// <param name="pack">目标卡池。</param>
        /// <param name="cardManager">卡牌管理器。</param>
        /// <returns>目标卡池依赖的所有卡池（不包括目标卡池）。</returns>
        public static CardPoolInfoPack[] GetDependencies(this CardPoolInfoPack pack, CardManager cardManager)
        {
            HashSet<CardPoolInfoPack> dependencies = new HashSet<CardPoolInfoPack>();
            addCardPoolDeps(pack);

            void addCardPoolDeps(CardPoolInfoPack pool)
            {
                if (pool != null && pool.dependencies != null)
                {
                    foreach (var poolRef in pool.dependencies)
                    {
                        var poolInfo = cardManager.getCardPoolInfo(poolRef);
                        if (poolInfo == null)
                            continue;
                        var poolPack = poolInfo.ToCardPoolInfoPack();

                        if (dependencies.Add(poolPack))
                        {
                            addCardPoolDeps(poolPack);
                        }
                    }
                }
            }
            return dependencies.ToArray();
        }


        /// <summary>
        /// 获取目标卡池依赖的，特定集合内所有卡池。
        /// </summary>
        /// <param name="pack">目标卡池。</param>
        /// <param name="cardPoolFullSet">卡池全集，从这个范围内寻找依赖的卡池。</param>
        /// <returns>目标卡池依赖的所有卡池（不包括目标卡池）。</returns>
        public static CardPoolInfoPack[] GetDependenciesInSet(this CardPoolInfoPack pack, IEnumerable<CardPoolInfoPack> cardPoolFullSet)
        {
            HashSet<CardPoolInfoPack> dependencies = new HashSet<CardPoolInfoPack>();
            addCardPoolDeps(pack);

            void addCardPoolDeps(CardPoolInfoPack pool)
            {
                if (pool != null && pool.dependencies != null)
                {
                    foreach (var poolRef in pool.dependencies)
                    {
                        var poolPack = cardPoolFullSet.FirstOrDefault(p => p.CompareCardPool(poolRef));
                        if (poolPack == null)
                            continue;

                        if (dependencies.Add(poolPack))
                        {
                            addCardPoolDeps(poolPack);
                        }
                    }
                }
            }
            return dependencies.ToArray();
        }

        /// <summary>
        /// 获取依赖目标卡池的所有卡池。
        /// </summary>
        /// <param name="pool">目标卡池。</param>
        /// <param name="cardPoolFullSet">卡池全集，从这个范围内寻找依赖卡池。</param>
        /// <returns>依赖于目标卡池的所有卡池（不包括目标卡池）。</returns>
        public static CardPoolInfoPack[] GetDependedCardPools(this CardPoolInfoPack pool, IEnumerable<CardPoolInfoPack> cardPoolFullSet)
        {
            HashSet<CardPoolInfoPack> pools = new HashSet<CardPoolInfoPack>();
            addCardPoolDeps(pool);

            void addCardPoolDeps(CardPoolInfoPack poolPack)
            {
                foreach (var pack in cardPoolFullSet)
                {
                    if (pack.dependencies.Any(r => poolPack.CompareCardPool(r)))
                    {
                        if (pools.Contains(pack))
                            return;

                        if (pools.Add(pack))
                        {
                            addCardPoolDeps(pack);
                        }
                    }
                }
            }

            return pools.ToArray();
        }

        /// <summary>
        /// 获取目标卡池列表依赖的所有卡池。
        /// </summary>
        /// <param name="pools">目标卡池列表。</param>
        /// <param name="cardManager">卡牌管理器。</param>
        /// <returns>卡尺列表依赖的所有卡池（不包括目标卡池）。</returns>
        public static CardPoolInfoPack[] GetDependencies(this IEnumerable<CardPoolInfoPack> pools, CardManager cardManager)
        {
            HashSet<CardPoolInfoPack> dependencies = new HashSet<CardPoolInfoPack>();
            foreach (var pool in pools)
            {
                foreach (var dep in pool.GetDependencies(cardManager))
                {
                    dependencies.Add(dep);
                }
            }
            return dependencies.ToArray();
        }
        public static CardPoolInfoPack ToCardPoolInfoPack(this CardPoolInfo info)
        {
            return new CardPoolInfoPack(info.cardPoolId, info.cardPoolName, info.editTime, info.dependencyRefs ?? new CardPoolReference[0]);
        }
        public readonly static CardPoolInfoPack BuiltinCardPoolInfoPack = new CardPoolInfoPack(0, StringTable.CARDPOOL_NAME_BUILTIN, 0);
        
    }
}
