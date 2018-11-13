using UnityEngine;


namespace TouhouHeartstone.Frontend.Manager
{
    /// <summary>
    /// 前端子管理器
    /// </summary>
    public abstract class FrontendSubManager : MonoBehaviour
    {
        FrontendManager frontendManager;

        /// <summary>
        /// 前端管理器
        /// </summary>
        public FrontendManager Frontend => frontendManager;

        /// <summary>
        /// 获取同级别的管理器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T getSiblingManager<T>() where T : FrontendSubManager
        {
            return Frontend.GetSubManager<T>();
        }

        protected void Awake()
        {
            frontendManager = GetComponentInParent<FrontendManager>();
        }
    }
}
