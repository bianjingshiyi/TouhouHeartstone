namespace Game.Animations
{
    public class AreaEffect : TableEffect
    {
        protected override void updateAnim(float deltaTime)
        {
            if (sourceTransform)
            {
                transform.position = sourceTransform.position;
            }
        }
    }
    public enum TableArea
    {
        None,
        AllField,
        EnemyField,
        SelfField
    }
}
