using System;

namespace Controllers
{
    [Serializable]
    public class ActionNodeGraphNodeInfo
    {
        public ActionNodeGraphNodeInfo(float posX, float posY)
        {
            this.posX = posX;
            this.posY = posY;
        }
        public float posX;
        public float posY;
    }
}
