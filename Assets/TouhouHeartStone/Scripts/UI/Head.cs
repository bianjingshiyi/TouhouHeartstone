using UnityEngine;
namespace UI
{
    partial class Head
    {
        [SerializeField]
        Secret _secretPrefab;
        public Secret addSecret()
        {
            if (P1Image.transform.childCount < 1)
                return Instantiate(_secretPrefab, P1Image.transform);
            else if (P2Image.transform.childCount < 1)
                return Instantiate(_secretPrefab, P2Image.transform);
            else if (P3Image.transform.childCount < 1)
                return Instantiate(_secretPrefab, P3Image.transform);
            else if (P4Image.transform.childCount < 1)
                return Instantiate(_secretPrefab, P4Image.transform);
            else if (P5Image.transform.childCount < 1)
                return Instantiate(_secretPrefab, P5Image.transform);
            else
                throw new SpaceNotEnoughException();
        }
        public bool removeSecret(Secret secret)
        {
            if (P1Image.transform.GetChild(0) == secret.transform)
            {
                Destroy(secret.gameObject);
                return true;
            }
            else if (P2Image.transform.GetChild(0) == secret.transform)
            {
                Destroy(secret.gameObject);
                return true;
            }
            else if (P3Image.transform.GetChild(0) == secret.transform)
            {
                Destroy(secret.gameObject);
                return true;
            }
            else if (P4Image.transform.GetChild(0) == secret.transform)
            {
                Destroy(secret.gameObject);
                return true;
            }
            else if (P5Image.transform.GetChild(0) == secret.transform)
            {
                Destroy(secret.gameObject);
                return true;
            }
            else
                return false;
        }
    }
}