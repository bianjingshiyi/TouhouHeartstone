using TouhouHeartstone.Backend;
namespace TouhouHeartstone.Frontend.Model.Witness
{
    public abstract class WitnessHandler
    {
        public abstract string Name { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="witness"></param>
        /// <param name="deck"></param>
        /// <param name="callback"></param>
        /// <returns>如果返回false，则表明没有处理callback</returns>
        protected abstract bool witnessSuccessHandler(EventWitness witness, DeckController deck, GenericAction callback = null);

        /// <summary>
        /// 处理各种Witness
        /// </summary>
        /// <param name="witness"></param>
        /// <param name="deck"></param>
        /// <param name="callback"></param>
        public virtual void HandleWitness(EventWitness witness, DeckController deck, GenericAction callback = null)
        {
            var error = witness.getVar<ErrorCode>("code", false);
            if (error != ErrorCode.none)
            {
                if (!witnessFailHandler(witness, error, deck, callback))
                    callback?.Invoke(this, null);
            }
            else
            {
                if (!witnessSuccessHandler(witness, deck, callback))
                    callback?.Invoke(this, null);
            }
        }

        /// <summary>
        /// Witness错误处理器
        /// </summary>
        /// <param name="witness"></param>
        /// <param name="error"></param>
        /// <param name="deck"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        protected virtual bool witnessFailHandler(EventWitness witness, ErrorCode error, DeckController deck, GenericAction callback = null)
        {
            UberDebug.LogWarningChannel("View", "Witness遇到了错误：\n {0} {1}", witness, error);
            return false;
        }

    }
}
