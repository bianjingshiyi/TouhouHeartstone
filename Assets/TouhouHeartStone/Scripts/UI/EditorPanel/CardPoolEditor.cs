using System.Threading.Tasks;
using Controllers;

namespace UI
{
    /// <summary>
    /// 用于编辑卡池的编辑器UI基类。
    /// </summary>
    abstract class CardPoolEditor : GameObjectCtrl<MainSceneCtrl>
    {
        public abstract Task display(CardPoolDisplayInfo info);
        public virtual void hide()
        {
            gameObject.SetActive(false);
        }
    }
    public class CardPoolDisplayInfo
    {
        public long cardPoolId;
        public object displayObj;
    }
}
