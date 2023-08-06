using UnityEngine;

namespace Game
{
    static class MeshRendererHelper
    {
        public static void setSprite(this MeshRenderer mesh, Sprite sprite, MaterialPropertyBlock propertyBlock = null)
        {
            if (propertyBlock == null)
            {
                propertyBlock = new MaterialPropertyBlock();
            }

            mesh.GetPropertyBlock(propertyBlock);
            Texture texture = sprite ? sprite.texture : Texture2D.whiteTexture;
            propertyBlock.SetTexture("_MainTex", texture);
            mesh.SetPropertyBlock(propertyBlock);
        }
        public static void setTableImageSprite(this MeshRenderer mesh, Sprite sprite, MaterialPropertyBlock propertyBlock = null)
        {
            if (propertyBlock == null)
            {
                propertyBlock = new MaterialPropertyBlock();
            }

            mesh.GetPropertyBlock(propertyBlock);
            float x = 1, y = 1, z = 0, w = 0;

            Texture texture;
            if (sprite)
            {
                texture = sprite.texture;
                float sprAspect = sprite.rect.width / sprite.rect.height;
                x = 1 / sprAspect;
                y = 1;
                z = (1 - x) * 0.5f;
                w = (1 - y) * 0.5f;
            }
            else
            {
                texture = Texture2D.whiteTexture;
            }
            Vector4 vector = new Vector4(x, y, z, w);
            propertyBlock.SetTexture("_MainTex", texture);
            propertyBlock.SetVector("_MainTex_ST", vector);
            mesh.SetPropertyBlock(propertyBlock);
        }
    }
}