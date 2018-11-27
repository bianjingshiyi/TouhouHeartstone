using UnityEngine;

using XLua;

namespace Tests
{
    public class XLuaTest : MonoBehaviour
    {
        protected void Awake()
        {
            luaEnv = new LuaEnv();
        }
        protected void Start()
        {
            luaEnv.DoString(
            @"
                CS.UnityEngine.Debug.Log('禁用对UnityEngine的访问')
                CS.UnityEngine = nil
            ", "XLuaTest");
            luaEnv.DoString(
            @"
                local log = CS.UnityEngine.Debug.Log
                local obj = CS.Tests.LuaObject()
                log(obj.integer)
                log(obj.real)
                obj:method()
            ", "XLuaTest");
        }
        protected void Update()
        {
            luaEnv.Tick();
        }
        protected void OnDestroy()
        {
            luaEnv.Dispose();
        }
        LuaEnv luaEnv { get; set; }
    }
    [LuaCallCSharp]
    class LuaObject
    {
        public int integer = 1;
        private float real = 2;
        public void method()
        {
            Debug.Log("调用方法");
        }
    }
}