using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BJSYGameCore;

namespace Tests
{
    public class EditorTests
    {
        [Test]
        public void assetBundleTest()
        {
            Object asset = Resources.Load("TestPicture");
            var manifest = BundleInfoKeeperEditor.buildAssetBundle(asset, "Test", "Picture", Application.streamingAssetsPath + "/AssetBundles");
            foreach (var bundleName in manifest.GetAllAssetBundles())
            {
                AssetBundle bundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/AssetBundles/" + bundleName);
                foreach (var name in bundle.GetAllAssetNames())
                {
                    Debug.Log(name);
                }
            }
        }
    }
}
