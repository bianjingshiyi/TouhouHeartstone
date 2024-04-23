namespace Game.Animations
{
    public class CameraEffect : TableEffect
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
