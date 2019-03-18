using UnityEngine;

using System;

namespace TouhouHeartstone.OldFrontend
{
    public class DeskEntity : MonoBehaviour
    {
        [NonSerialized]
        int instanceID;

        [SerializeField]
        int typeID;

        /// <summary>
        /// 卡片对应的类型ID
        /// </summary>
        public int TypeID => typeID;

        /// <summary>
        /// 卡片的ID
        /// </summary>
        public int InstanceID => instanceID;

        /// <summary>
        /// 设置id
        /// </summary>
        /// <param name="id"></param>
        public void SetInstanceID(int id)
        {
            instanceID = id;
        }
    }
}
