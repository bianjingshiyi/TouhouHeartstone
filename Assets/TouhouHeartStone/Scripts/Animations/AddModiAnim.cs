using UI;
namespace Game
{
    class AddModiAnim : EventAnimation<TouhouCardEngine.Card.AddModiEventArg>
    {
        AnimAnim _anim;
        public override bool update(TableManager table, TouhouCardEngine.Card.AddModiEventArg eventArg)
        {
            //if (table.tryGetServant(eventArg.card, out var servant))
            //{
            //    servant.onAddModi.beforeAnim.Invoke();
            //    if (!string.IsNullOrEmpty(servant.onAddModi.animName))
            //    {
            //        if (_anim == null)
            //            _anim = new AnimAnim(servant.animator, servant.onAddModi.animName);
            //        if (!_anim.update(table))
            //            return false;
            //    }
            //    servant.onAddModi.afterAnim.Invoke();
            //}
            return true;
        }
    }
}
