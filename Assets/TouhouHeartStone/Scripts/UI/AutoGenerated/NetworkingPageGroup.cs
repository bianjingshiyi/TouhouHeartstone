//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace UI
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    using BJSYGameCore.UI;
    
    public partial class NetworkingPageGroup : UIPageGroup
    {
        protected override void Awake()
        {
            base.Awake();
            this.autoBind();
            this.onAwake();
        }
        public void autoBind()
        {
            this._LANPanel = this.transform.Find("LANPanel").GetComponent<LANPanel>();
            this._RoomPanel = this.transform.Find("RoomPanel").GetComponent<RoomPanel>();
        }
        private Main _parent;
        public Main parent
        {
            get
            {
                return this.transform.parent.parent.GetComponent<Main>();
            }
        }
        [SerializeField()]
        private LANPanel _LANPanel;
        public LANPanel LANPanel
        {
            get
            {
                if ((this._LANPanel == null))
                {
                    this._LANPanel = this.transform.Find("LANPanel").GetComponent<LANPanel>();
                }
                return this._LANPanel;
            }
        }
        [SerializeField()]
        private RoomPanel _RoomPanel;
        public RoomPanel RoomPanel
        {
            get
            {
                if ((this._RoomPanel == null))
                {
                    this._RoomPanel = this.transform.Find("RoomPanel").GetComponent<RoomPanel>();
                }
                return this._RoomPanel;
            }
        }
        public override UIObject[] getPages()
        {
            return new UIObject[] {
                    this.LANPanel,
                    this.RoomPanel};
        }
        partial void onAwake();
    }
}
