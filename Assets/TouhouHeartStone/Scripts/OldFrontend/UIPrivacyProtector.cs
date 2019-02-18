
using UnityEngine;
using IGensoukyo.Utilities;

namespace TouhouHeartstone.OldFrontend
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
