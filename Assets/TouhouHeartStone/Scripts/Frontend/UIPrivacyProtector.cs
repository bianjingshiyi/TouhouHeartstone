
using UnityEngine;

namespace TouhouHeartstone.Frontend
{
    public class UIPrivacyProtector : UIPopup
    {
        [SerializeField]
        TextMeshExtend textMesh;

        public void SetCurrentUserID(int id)
        {
            textMesh.text = id.ToString() + "的回合";
        }
    }
}
