using System.Collections.Generic;
using System.Threading.Tasks;
using BJSYGameCore;
using Controllers;
using TouhouCardEngine.Shared;

namespace Game
{
    public partial class EditorManager : Manager
    {
        #region 公有方法

        #region 编辑器环境
        public async Task reloadCardPoolEnv(CardPoolInfo info)
        {
            cardPoolEnv.Clear();
            cardPoolEnv.Add(info.ToReference());
            if (info.dependencyRefs != null)
                cardPoolEnv.AddRange(info.dependencyRefs);
            foreach (var poolRef in cardPoolEnv)
            {
                await game.card.getOrLoadCardPool(poolRef, true);
            }
            updateBuffDefines();
        }
        public void clearCardPoolEnv()
        {
            cardPoolEnv.Clear();
            updateBuffDefines();
        }
        public ICardPoolReference[] getEnvCardPoolRefs()
        {
            return cardPoolEnv.ToArray();
        }
        public ICardPoolReference getCardPoolReference(long poolId)
        {
            return game.card.getCardPoolReferenceInEnv(poolId, getEnvCardPoolRefs());
        }
        public ICardPoolReference getCurrentCardPoolReference()
        {
            return getCardPoolReference(currentCardPoolId);
        }
        public void setCurrentCardPoolId(long id)
        {
            currentCardPoolId = id;
        }
        #endregion

        #endregion

        #region 属性字段
        public long currentCardPoolId { get; private set; }
        private List<ICardPoolReference> cardPoolEnv = new List<ICardPoolReference>();
        #endregion
    }
}
