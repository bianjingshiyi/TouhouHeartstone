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

        public bool UpdateSercret()
        {
            for(int Start=0;Start<getPArray().Length;Start++)
            {
                if (getPArray()[Start].childCount==0)
                {
                    for(int end = Start+1; end < getPArray().Length; end++)
                    {
                        if (getPArray()[end].childCount > 0)
                        {
                            getPArray()[end].GetChild(0).SetParent(getPArray()[Start],false);
                            break;
                        }
                    }
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