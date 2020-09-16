using UnityEngine;
namespace UI
{
    partial class Head
    {
        [SerializeField]
        Secret _secretPrefab;
        public Secret addSecret()
        {
            foreach (var p in getPArray())
            {
                if (p.childCount < 1)
                    return Instantiate(_secretPrefab, p);
            }
            throw new SpaceNotEnoughException();
        }
        public bool removeSecret(Secret secret)
        {
            foreach (var p in getPArray())
            {
                if (p.childCount > 0 && p.GetChild(0) == secret.transform)
                {
                    Destroy(secret.gameObject);
                    return true;
                }
            }
            return false;
        }
        public void clearSecret()
        {
            foreach (var p in getPArray())
            {
                for (int i = 0; i < p.childCount; i++)
                {
                    Destroy(p.GetChild(i).gameObject);
                }
            }
        }
        private RectTransform[] getPArray()
        {
            return new RectTransform[] { P1Image.rectTransform, P2Image.rectTransform, P3Image.rectTransform, P4Image.rectTransform, P5Image.rectTransform };
        }
    }
}