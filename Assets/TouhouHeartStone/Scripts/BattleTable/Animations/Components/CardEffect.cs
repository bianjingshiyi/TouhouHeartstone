namespace Game.Animations
{
    public class CardEffect : TableEffect
    {
        protected override void updateAnim(float deltaTime)
        {
            if (sourceTransform)
            {
                transform.position = sourceTransform.position;
            }
        }
    }
}
