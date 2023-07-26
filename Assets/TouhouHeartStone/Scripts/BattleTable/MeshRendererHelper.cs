using UnityEngine;

namespace Game
{
    static class MeshRendererHelper
    {
        public static void setColor(this MeshRenderer meshRenderer, Color color)
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            meshRenderer.GetPropertyBlock(block);
            block.SetColor("_Tint", color);
            meshRenderer.SetPropertyBlock(block);
        }
        public static void setTexture(this MeshRenderer meshRenderer, Texture texture)
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            meshRenderer.GetPropertyBlock(block);
            block.SetTexture("_MainTex", texture);
            meshRenderer.SetPropertyBlock(block);
        }
    }
}